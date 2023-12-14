using System.Diagnostics.Tracing;
using System.Web;
using AutoMapper;
using Microsoft.Extensions.Options;
using MoreLinq;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using Samolet.Dp.Common.GlobalContracts.ValueObjects.Range;
using Samolet.Dp.Common.Utilities;
using Samolet.Dp.RealEstateClassifiedParser.Core.CardNormalizator;
using Samolet.Dp.RealEstateClassifiedParser.Core.Counters;
using Samolet.Dp.RealEstateClassifiedParser.Core.Entities;
using Samolet.Dp.RealEstateClassifiedParser.Core.Exceptions;
using Samolet.Dp.RealEstateClassifiedParser.Core.Extensions;
using Samolet.Dp.RealEstateClassifiedParser.Core.LocalProxy;
using Samolet.Dp.RealEstateClassifiedParser.Core.Mapping;
using Samolet.Dp.RealEstateClassifiedParser.Core.Reporters;
using Samolet.Dp.RealEstateClassifiedParser.Core.WebDriver;
using Samolet.Dp.RealEstateClassifiedParser.IntegrationEvents;
using Samolet.Dp.RealEstateClassifiedParser.ValueObjects.Enums;
using Samolet.Dp.RealEstateClassifiedParser.ValueObjects.Settings;
using Samolet.Dp.RealEstateClassifiedParser.ValueObjects.ValueObjects;
using Serilog;

namespace Samolet.Dp.RealEstateClassifiedParser.Core.Crawlers;

/// <inheritdoc/>
public abstract class ClassifiedCardCrawler<TSettings> : IClassifiedCardCrawler where TSettings : ClassifiedParsingSettings
{
    protected readonly IMapper _mapper;

    private readonly ILogger _logger;
    private readonly IWebDriverCollection _webDriverCollection;

    /// <summary>
    /// Драйвер браузера.
    /// </summary>
    protected IWebDriver _webDriver;

    /// <summary>
    /// Настройки парсинга.
    /// </summary>
    protected TSettings _settings;

    /// <summary>
    /// Общие настройки парсинга.
    /// </summary>
    protected CommonParsingSettings _commonSettings;

    /// <summary>
    /// Провайдер текущей даты.
    /// </summary>
    protected IDateTimeProvider _dateTimeProvider;

    protected ICardNormalizator _cardNormalizator;

    protected IReportBuilder _reportBuilder;

    protected bool _changeIp = false;

    protected bool lastPage = false;

    private int pageNumber = 1;

    /// <summary>
    /// Максимальное количество страниц со списками объявлений.
    /// </summary>
    protected abstract int MaxPageNumber { get; }

    protected abstract int AdsCountOnListPage { get; }

    protected string TrafficCounterName => $"chrome_traffic_{Classified}".ToLower();

    protected string BlocksCounterName => $"blocks_{Classified}".ToLower();

    protected readonly IAppMetrics _appMetrics;
    protected readonly ILocalProxy _localProxy;

    /// <summary>
    /// Максимальное количество объявлений на всех списочных страницах классифайда.
    /// </summary>
    protected abstract int MaxAdsCountOnListPages { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ClassifiedCardCrawler{TSettings}"/>.
    /// </summary>
    /// <param name="webDriverCollection">Коллекиця драйверов браузера.</param>
    /// <param name="settings">Настройки парсинга.</param>
    /// <param name="commonSettings">Общие настроек парсинга.</param>
    /// <param name="dateTimeProvider">Провайдер текущей даты.</param>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="reportBuilder">Builder отчета о работе.</param>
    /// <param name="appMetrics">Метрики приложения.</param>
    /// <param name="localProxy">Локальный прокси.</param>
    /// <param name="deduplicationOptionSnapshot">Настройки резолюции для вычисления H3Index.</param>
    public ClassifiedCardCrawler(
        IWebDriverCollection webDriverCollection,
        TSettings settings,
        CommonParsingSettings commonSettings,
        IDateTimeProvider dateTimeProvider,
        ILogger logger,
        IReportBuilder reportBuilder,
        IAppMetrics appMetrics,
        ILocalProxy localProxy,
        IOptionsSnapshot<DeduplicationOptions> deduplicationOptionSnapshot)
    {
        _webDriverCollection = webDriverCollection;
        _webDriver = webDriverCollection.GetWebDriver(Classified);
        _settings = settings;
        _commonSettings = commonSettings;
        _dateTimeProvider = dateTimeProvider;
        _cardNormalizator = new CardNormalizator.CardNormalizator(settings.NormalizationRules, deduplicationOptionSnapshot);
        _logger = logger;
        _reportBuilder = reportBuilder;
        _localProxy = localProxy;

        _mapper = new MapperConfiguration(_ =>
        {
            _.AddProfile(new CardMappingProfile(settings.JsonMapping, Classified));
            _.AddProfile(new CianCardProfile(dateTimeProvider));
        }).CreateMapper();

        _appMetrics = appMetrics;
    }

    /// <summary>
    /// Получает URL первой страницы.
    /// </summary>
    /// <param name="command">Команда на запуск парсинга карточек.</param>
    /// <returns>Стартовый URL.</returns>
    protected string GetStartUrl(BaseCardTaskIntegrationEvent command)
        => _settings!.StartUrl
        .Replace("{Region}", _settings!.RegionCodes[command.Region])
        .Replace("{RealEstateType}", _settings!.RealEstateTypeCodes[command.RealEstateType]);

    /// <summary>
    /// Получает страницу карточек и переходит на следующую страницу.
    /// </summary>
    /// <param name="command">Команда на запуск парсинга карточек.</param>
    /// <returns>Перечисление карточек.</returns>
    protected abstract (IEnumerable<Card> Cards, int AdsCount) GetPageData(BaseCardTaskIntegrationEvent command);

    protected string NextPageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Классифайд парсера.
    /// </summary>
    protected abstract Classified Classified { get; }

    /// <summary>
    /// Фильтр карточек при парсинге.
    /// </summary>
    /// <param name="card">Карта.</param>
    /// <returns>Включать ли карточку, в результирующий набор.</returns>
    protected virtual bool ActualCardFilter(Card card) => true;

    /// <inheritdoc/>
    public (IEnumerable<Card> ClassifiedCards, int AdsCount) GetNewCards(CardsParseTaskIntegrationEvent command, IEnumerable<string> existingCardClassifiedIds)
    {
        int adsCountInClassified = 0;
        var shouldCrawlNextPage = false;
        var cards = new List<Card>();

        try
        {
            NextPageUrl = GetStartUrl(command);
        }
        catch (Exception ex)
        {
            _reportBuilder.AddError(ex);
            _logger.Error(ex, "Ошибка формирования URL.");
            return (cards, adsCountInClassified);
        }

        Func<Card, bool> cardLivenessSpecification = (card) => CardLivenessSpecification(card, existingCardClassifiedIds);

        do
        {
            (var page, var adsCount) = GetPage(command);
            cards.AddRange(NormalizePage(command, page, cardLivenessSpecification));
            if (pageNumber == 1)
            {
                adsCountInClassified = adsCount;
            }

            pageNumber++;
            shouldCrawlNextPage = page.Window(_commonSettings.StopCount).All(_ => _.Any(cardLivenessSpecification)) && pageNumber <= MaxPageNumber && adsCount != 0;
        }
        while (shouldCrawlNextPage);

        return (cards, adsCountInClassified);
    }

    /// <inheritdoc/>
    public IEnumerable<IEnumerable<Card>> GetCardsFromNextPage(CardUpdateTaskIntegrationEvent command)
    {
        bool CardLivenessSpecification(Card card) => card.DateTimeCreatedClassified.TryParseClassifiedDateTime(out var date) && date >= command.MinDate;

        do
        {
            if (pageNumber == 1)
            {
                if (!ValidateCommand(command))
                {
                    yield break;
                }

                NextPageUrl = GetStartUrl(command);
            }

            (var page, _) = GetPage(command);
            var cards = NormalizePage(command, page, CardLivenessSpecification);

            pageNumber++;
            yield return cards;

            if (!(page.Window(_commonSettings.StopCount).All(_ => _.Any(CardLivenessSpecification)) && pageNumber <= MaxPageNumber))
            {
                yield break;
            }
        }
        while (true);
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<(IEnumerable<Card> Cards, string FirstUrl, IEnumerable<string>? Urls, int? AdsCount)> GetCardsFromNextPageOrUrlsAsync(CardUpdateTaskIntegrationEvent command)
    {
        bool CardLivenessSpecification(Card card) =>
            (!string.IsNullOrEmpty(card.DateTimeUpdatedClassified) ? card.DateTimeUpdatedClassified : card.DateTimeCreatedClassified).TryParseClassifiedDateTime(out var date)
            && date >= command.MinDate;

        await Task.Delay(0);
        int? adsCountInClassified = null;
        int previousEmptyPages = 0;
        int reloadEmptyPages = 0;
        string firstUrl = string.Empty;
        bool firstPageReloaded = false;

        if (pageNumber == 1)
        {
            if (!ValidateCommand(command))
            {
                yield break;
            }

            NextPageUrl = GetStartUrl(command);
            firstUrl = NextPageUrl;
        }

        if (command.Url != null)
        {
            NextPageUrl = command.Url;
        }

        var maxPageNumber = 0;

        do
        {
            var currentUrl = NextPageUrl;
            (var page, var adsCount) = GetPage(command);

            if (pageNumber == 1)
            {
                maxPageNumber = page.Count == 0 ? 1 : (adsCount / AdsCountOnListPage) + 1;
            }

            adsCountInClassified = command.Url == null && pageNumber == 1 ? adsCount : null;

            if (adsCount > MaxAdsCountOnListPages)
            {
                _logger.Information("Превышено допустимое количество объявлений на страницах, найдено {AdsCount} при доустимых {MaxAdsCountOnListPages}.", adsCount, MaxAdsCountOnListPages);
                var urls = GetUrlsSplitByPrice(adsCount, command);
                _logger.Information("Сформировано {UrlsCount} ссылок. {@Urls}.", urls.Count(), urls);

                yield return (Enumerable.Empty<Card>(), firstUrl, urls, adsCountInClassified);
                yield break;
            }
            else
            {
                var cards = NormalizePage(command, page, CardLivenessSpecification);

                if (previousEmptyPages > 0)
                {
                    // убедится, что открылась следующая страница, а не редирект на 1. ИЛИ если это уже 6 пустая, то выйти.
                    if (CurrentPageFromUrl() != pageNumber || previousEmptyPages > 5)
                    {
                        yield break;
                    }
                }

                // при пустой странице, перезапускаем браузер с новым прокси, тк это баг Авито.
                if (command.Classified == Classified.Avito && !cards.Any() && reloadEmptyPages < 10 && pageNumber != 1)
                {
                    reloadEmptyPages++;
                    _logger.Debug("Пустая страница, ip будет сменён {Url}.", currentUrl);
                    _localProxy.SetNewIp();
                    _appMetrics.AddBlock(Classified);
                    NextPageUrl = currentUrl;
                    _webDriver = _webDriverCollection.GetWebDriver(Classified);
                    continue;
                }

                // на регионах, где совсем нет карточек (костыль, тк иначе нельзя определить карточек нет потому что нет, или это баг авито)
                if (!cards.Any() && pageNumber == 1)
                {
                    _logger.Information("В регионе {Region} для типа ОН {RealEstateType} классифайд {Classified} нет карточек.", command.Region, command.RealEstateType, command.Classified);
                    yield return (cards, firstUrl, null, adsCountInClassified);
                    yield break;
                }

                pageNumber++;
                reloadEmptyPages = 0;

                // иногда открывается страница без карточек, надо открыть следующую, и убедиться, что она тоже пустая
                if (!cards.Any())
                {
                    _logger.Information("Пустая страница после попыток ретрая {Url}.", currentUrl);
                    previousEmptyPages++;
                    continue;
                }

                yield return (cards, firstUrl, null, adsCountInClassified);

                if (lastPage || pageNumber > maxPageNumber || !page.Window(_commonSettings.StopCount).All(_ => _.Any(CardLivenessSpecification)))
                {
                    // переоткрытие 1 страницы списка, для подбора поднявшихся карточек.
                    if (pageNumber > _commonSettings.ReloadFirstPageOverPages && !firstPageReloaded)
                    {
                        NextPageUrl = firstUrl;
                        firstPageReloaded = true;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }
        while (true);
    }

    private int CurrentPageFromUrl()
    {
        var uriQuery = new Uri(_webDriver.Url).Query;
        var pageNumberString = HttpUtility.ParseQueryString(uriQuery).Get("p");
        if (string.IsNullOrEmpty(pageNumberString) || !int.TryParse(pageNumberString, out int pageNumber))
        {
            return -1;
        }

        return pageNumber;
    }

    private IEnumerable<string> GetUrlsSplitByPrice(int adsCount, CardUpdateTaskIntegrationEvent command)
    {
        var startUrl = GetStartUrl(command);
        var rangeCount = adsCount / MaxAdsCountOnListPages;
        long pricePeek = command.RealEstateType switch
        {
            RealEstateType.Apartment => 3_500_000,
            RealEstateType.Room => 500_000,
            RealEstateType.Garage => 300_000,
            RealEstateType.CountryEstate => 500_000,
            RealEstateType.Land => 300_000,
            RealEstateType.CommercialRealEstate => 5_000_000,
            _ => 3_500_000
        };

        var priceRanges = command.SeparatedList ?
            SplitRange(command.Url!, rangeCount + 1) :
            RangeSplitHelper.StepRanges(new Range<long>(1, 100_000_000), rangeCount * 2, pricePeek);

        return priceRanges.Select(_ => GenerateUrl(_, startUrl, command.RealEstateType));
    }

    private IEnumerable<Range<long>> SplitRange(string url, int steps)
    {
        var result = RangeSplitHelper.StepRanges(GetRangeFromUrl(url), steps);

        if (result.Count() == 1)
        {
            var item = result.Single();
            _logger.Error("Не удалось разбить интервал от {From} до {To}", item.From, item.To);
        }

        return result;
    }

    protected abstract Range<long> GetRangeFromUrl(string url);

    protected abstract string GenerateUrl(Range<long> range, string startUrl, RealEstateType realEstateType);

    private (List<Card> Cards, int AdsCount) GetPage(BaseCardTaskIntegrationEvent command)
    {
        int mainElementNotFoundCounter = 0, adsCount = 0;
        IEnumerable<Card> page;

        do
        {
            try
            {
                LoadPage(NextPageUrl, 100, command);
                (page, adsCount) = GetPageData(command);

                if (!page.Any())
                {
                    break;
                }
            }
            catch (Exception e) when ((e is MainElementNotFoundException or MaxLoadAttemptsException) && mainElementNotFoundCounter++ < _commonSettings.FindMainElementAttemptsCountOnListPage)
            {
                _logger.Information("Не удалось получить главный элемент на странице {Url}, попытка {MainElementNotFoundCounter}.", NextPageUrl, mainElementNotFoundCounter);
                _localProxy.SetNewIp();
                _webDriver = _webDriverCollection.GetWebDriver(Classified);
                continue;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при загрузке страницы. Операция парсинга карточек прервана.");
                _reportBuilder.AddError(e);
                throw;
            }

            break;
        }
        while (true);

        return (page.ToList(), adsCount);
    }

    private List<Card> NormalizePage(BaseCardTaskIntegrationEvent command, List<Card> page, Func<Card, bool> cardLivenessSpecification)
    {
        var cards = new List<Card>();
        foreach (var card in page.Where(cardLivenessSpecification))
        {
            _cardNormalizator.Normalize(card);
            AddPropertiesToCard(card, command);

            if (!card.RealEstateType.Equals(RealEstateType.Undefined))
            {
                cards.Add(card);
            }
        }

        return cards;
    }

    private void LogTrafficUsage(BaseCardTaskIntegrationEvent? command)
    {
        try
        {
            var resources = _webDriver.GetResourcesTransferSize();

            var totalTraffic = resources.Sum(_ => _.TransferSize);
            _logger.Debug("Использовано трафика: {TotalTransferSize}", totalTraffic);

            _appMetrics.AddTraffic(Classified, command?.JobTriggerType ?? JobTriggerType.AdParsing, totalTraffic);

            foreach (var group in resources.GroupBy(_ => new Uri(_.Name).Host))
            {
                _logger.Debug("Host: {Host} Трафик: {TransferSize}", group.Key, group.Sum(_ => _.TransferSize));
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Ошибка при получении статистики браузера");
        }
    }

    protected virtual void AddSpecialPropertiesToCard(Card card, BaseCardTaskIntegrationEvent command)
    {
        if (card.RealEstateType == RealEstateType.Garage)
        {
            card.Attributes.LotArea = null;
        }
    }

    private void LoadPage(string url, int attemptCount, BaseCardTaskIntegrationEvent? command = default)
    {
        _logger.Information("Загрузка страницы {Url}.", url);
        int i = 1;
        List<string> blockedIp = new();
        string currentIp = string.Empty;
        string errorMsg = string.Empty;

        do
        {
            try
            {
                _webDriver.Navigate().GoToUrl(url);
                if (Classified == Classified.Avito)
                {
                    _webDriver.Stop();
                }
            }
            catch (WebDriverTimeoutException)
            {
                // не всегда нормальная ситуация, подвисает загрузка каких-то элементов, для парсинга не мешает.
                if (Classified == Classified.Avito && string.IsNullOrEmpty(_webDriver.Title))
                {
                    // авито, при блокировке просто не отправляет ответ
                    _logger.Debug("Страница авито не загружена. Целевая страница {Url}", url);
                    _localProxy.SetNewIp();
                    _webDriver = _webDriverCollection.GetWebDriver(Classified);
                    i++;
                    continue;
                }
            }
            catch (WebDriverException)
            {
                // ex.Message.Contains("unknown error: net::ERR_PROXY_CONNECTION_FAILED")  - проблема прокси, возможно кончился трафик
                // 504, обновляем.
                i++;
                continue;
            }
            finally
            {
                LogTrafficUsage(command);
            }

            if (IsBlokingPage(out currentIp, out errorMsg) || _changeIp)
            {
                _localProxy.SetNewIp();

                _logger.Debug("Страница блокировки {ErrorMsg}. Попытка {AttemptCount}. Целевая страница {Url}", errorMsg, i, url);
                _appMetrics.AddBlock(Classified);
                _webDriver.Manage().Cookies.DeleteAllCookies();
                blockedIp.Add(currentIp);
                _webDriver = _webDriverCollection.GetWebDriver(Classified);
                i++;
                _changeIp = false;
            }
            else
            {
                break;
            }
        }
        while (i < attemptCount);

        if (blockedIp.Any())
        {
            var ips = blockedIp.Distinct().Where(_ => !string.IsNullOrEmpty(_));
            _reportBuilder.AddProxyBlocking(i, ips);
        }

        if (i == attemptCount)
        {
            _logger.Error("Сработала блокировка {Attempts} раз.", attemptCount);
            throw new MaxLoadAttemptsException(url);
        }
    }

    protected abstract bool IsBlokingPage(out string currentIp, out string errorMsg);

    protected virtual bool Is404Page() => false;

    /// <inheritdoc/>
    public ParsedAd GetNewAd(string url, BaseCardTaskIntegrationEvent? command = default)
    {
        _logger.Information("Получение нового объявления по {Url} в {DateTimeParsing}", url, _dateTimeProvider.UtcNow);
        return GetAd(url, _settings.AdNewMaxAttemptCount, command);
    }

    private ParsedAd GetAd(string url, int attemptCount, BaseCardTaskIntegrationEvent? command = default)
    {
        _logger.Debug("Получение объявления по {URL}", url);
        int mainElementNotFoundCounter = 0;
        JObject? jsonData = null;

        do
        {
            try
            {
                LoadPage(url, attemptCount, command);

                if (Is404Page())
                {
                    _logger.Information("Объявление по {URL} удалено или не актуально.", url);
                    return new ParsedAd(ParseAdStatus.Deleted);
                }

                jsonData = GetJDataFromPage();
                break;
            }
            catch (MainElementNotFoundException)
            {
                mainElementNotFoundCounter++;
                _logger.Information("Не удалось получить главный элемент на странице {AdUrl}, попытка {MainElementNotFoundCounter}.", url, mainElementNotFoundCounter);
                _localProxy.SetNewIp();
                _webDriver = _webDriverCollection.GetWebDriver(Classified);
                continue;
            }
            catch (WebDriverException e)
            {
                _logger.Error(e, "Ошибка веб драйвера"); // причины пока не ясны
            }
            catch (MaxLoadAttemptsException e)
            {
                _reportBuilder.AddReport($"На странице {url} достигнуто максимальное количество блокировок.");
                _logger.Warning(e, "На странице {Url} достигнуто максимальное количество блокировок.", url);
                return new ParsedAd(ParseAdStatus.Blocked);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при загрузке страницы");
                return new ParsedAd(ParseAdStatus.Failed);
            }
        }
        while (mainElementNotFoundCounter < _commonSettings.FindMainElementAttemptsCountOnAdPage);

        if (jsonData == null)
        {
            return new ParsedAd(ParseAdStatus.Failed);
        }

        if (IsAdClosed(jsonData))
        {
            _logger.Information("Объявление по {URL} удалено или не актуально.", url);
            return new ParsedAd(ParseAdStatus.Deleted);
        }

        var newAd = _mapper.Map<Card>(jsonData!.SelectToken(_settings.JsonMapping.AdSelector));
        newAd.Attributes.UpdateAttributes(_mapper.Map<JToken, AdAttributes>(jsonData));
        _cardNormalizator.Normalize(newAd);
        if (command is not null)
        {
            AddPropertiesToCard(newAd, command);
        }

        _logger.Debug("Объявлеие по {URL} обработано удачно.", url);
        return new ParsedAd(ParseAdStatus.Success, newAd.Attributes, newAd.MediaFiles, newAd.RealEstateSubType, newAd.AdType);
    }

    protected abstract bool IsAdClosed(JObject jObject);

    /// <summary>
    /// Получить JObject текущей страницы.
    /// </summary>
    /// <returns>Атрибуты.</returns>
    protected abstract JObject? GetJDataFromPage();

    private bool ValidateCommand(BaseCardTaskIntegrationEvent command)
    {
        // что-то подумать с регистрозависимостью
        if (!_settings!.RegionCodes.ContainsKey(command.Region))
        {
            _logger.Error("Для классифайда {Classified} не задан регион {Region}", command.Classified, command.Region);
            _reportBuilder.Add($"Для классифайда {command.Classified} не задан регион {command.Region}", EventLevel.Critical);
            return false;
        }

        if (!_settings!.RealEstateTypeCodes.ContainsKey(command.RealEstateType))
        {
            _logger.Error("Для классифайда {Classified} не задан тип недвижимости {RealEstateType}", command.Classified, command.RealEstateType);
            _reportBuilder.Add($"Для классифайда {command.Classified} не задан тип недвижимости {command.RealEstateType}", EventLevel.Critical);
            return false;
        }

        return true;
    }

    private bool CardLivenessSpecification(Card card, IEnumerable<string> existingCardClassifiedIds) =>
        !string.IsNullOrEmpty(card.Url)
        && (!string.IsNullOrEmpty(card.DateTimeUpdatedClassified) ? card.DateTimeUpdatedClassified : card.DateTimeCreatedClassified).TryParseClassifiedDateTime(out var date)
        && date >= _dateTimeProvider.UtcNow.Date.AddDays(-_commonSettings.ParseCardsDepthDays + 1)
        && !existingCardClassifiedIds.Contains(card.AdClassifiedId);

    private void AddPropertiesToCard(Card card, BaseCardTaskIntegrationEvent command)
    {
        card.Region = command.Region;
        card.Status = CardStatus.Actual;
        card.DateTimeParsing = _dateTimeProvider.UtcNow;
        card.DateTimeLastParsing = _dateTimeProvider.UtcNow;
        AddSpecialPropertiesToCard(card, command);
    }
}

/// <summary>
/// Сборщик карточек объявлений Авито.
/// </summary>
public class Crawler
{
    private static readonly ILogger _logger = Log.ForContext<AvitoCardCrawler>();

    protected override Classified Classified => Classified.Avito;

    protected override int MaxPageNumber => 100;

    protected override int MaxAdsCountOnListPages => 4500;

    protected override int AdsCountOnListPage => 50;

    /// <inheritdoc cref="ClassifiedCardCrawler{AvitoParsingSettings}"/>
    public AvitoCardCrawler(IWebDriverCollection webDriverCollection,
        AvitoParsingSettings settings,
        CommonParsingSettings commonSettings,
        IDateTimeProvider dateTimeProvider,
        IReportBuilder reportBuilder,
        IAppMetrics appMetrics,
        ILocalProxy localProxy,
        IOptionsSnapshot<DeduplicationOptions> deduplicationOptionSnapshot)
        : base(webDriverCollection, settings, commonSettings, dateTimeProvider, _logger, reportBuilder, appMetrics, localProxy, deduplicationOptionSnapshot)
    {
    }

    private static string CreateAvitoUrl(string pathAndQuery) => $"https://www.avito.ru{pathAndQuery}";

    // отсекаются карточки, с пустым "id", тк там рекламный блок.
    protected override bool ActualCardFilter(Card card) => !string.IsNullOrEmpty(card.FilterProperty);

    /// <inheritdoc/>
    protected override (IEnumerable<Card> Cards, int AdsCount) GetPageData(BaseCardTaskIntegrationEvent command)
    {
        var payload = GetJDataFromPage();
        if (payload == null)
        {
            _logger.Warning("Не удалось получить данные карточек на странице {Url}", _webDriver.Url);
            _reportBuilder.Add($"Не удалось получить данные карточек на странице {_webDriver.Url}", EventLevel.Critical);
            return (Array.Empty<Card>(), 0);
        }

        var nextPageUrl = payload.SelectToken(_settings!.NextPageUrlJPath)?.Value<string>();

        if (string.IsNullOrEmpty(nextPageUrl))
        {
            lastPage = true;
        }
        else
        {
            NextPageUrl = CreateAvitoUrl(nextPageUrl);
        }

        var adsCount = payload.SelectToken(_settings!.TotalAdsCount)?.Value<int>() ?? 0;

        return (_mapper.Map<Card[]>(payload).Where(ActualCardFilter).ToArray(), adsCount);
    }

    protected override JObject? GetJDataFromPage()
    {
        var scriptElements = _webDriver.FindElements(By.TagName("script"));
        var rawPayload = FindScript(scriptElements);

        if (rawPayload is null)
        {
            _logger.Warning("Не удалось получить данные карточек на странице {Url}", _webDriver.Url);
            return null;
        }

        var payloadRegex = new Regex(@"^window\.__initialData__\s*=\s""(.+?)"";");
        var match = payloadRegex.Match(rawPayload);
        var urlEncodedPayloadJson = match.Groups[1].Value;
        var payloadJson = HttpUtility.UrlDecode(urlEncodedPayloadJson);

        if (string.IsNullOrEmpty(payloadJson))
        {
            throw new MainElementNotFoundException(_webDriver.Url);
        }

        return JObject.Parse(payloadJson);
    }

    private string FindScript(IEnumerable<IWebElement> source)
    {
        foreach (var element in source)
        {
            try
            {
                var content = element.GetAttribute("innerHTML");
                if (content?.StartsWith("window.__initialData__") ?? false)
                {
                    return content;
                }
            }
            catch
            {
            }
        }

        throw new MainElementNotFoundException(_webDriver.Url);
    }

    protected override bool IsAdClosed(JObject jObject)
    {
        return jObject.SelectToken(_settings.ClosedAdAttributeJPath)?.Value<bool>() ?? true;
    }

    protected override bool IsBlokingPage(out string currentIp, out string errorMsg)
    {
        currentIp = string.Empty; // авито ip не показывает.
        errorMsg = string.Empty;

        try
        {
            errorMsg = _webDriver.Title;
        }
        catch
        {
            return true;
        }

        return errorMsg == "Доступ ограничен: проблема с IP";
    }

    protected override Range<long> GetRangeFromUrl(string url)
    {
        return AvitoUrlHelper.GetRangeFromUrl(url);
    }

    protected override string GenerateUrl(Range<long> range, string startUrl, RealEstateType realEstateType)
    {
        return AvitoUrlHelper.GenerateUrl(range, startUrl, realEstateType);
    }

    protected override bool Is404Page()
    {
        return _webDriver.Title == "Ошибка 404. Страница не найдена";
    }
}
