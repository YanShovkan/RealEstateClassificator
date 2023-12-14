using AutoMapper;
using Newtonsoft.Json.Linq;
using RealEstateClassificator.Common.ValueObjects;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.CoreSettings.DefaultSettings;
using RealEstateClassificator.Dal.Entities;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
using System.Web;

namespace RealEstateClassificator.Core.Services;

/// <summary>
/// Сборщик карточек объявлений Авито.
/// </summary>
public class Crawler : ICrawler
{
    private static int MaxPageNumber => 100;

    private static int MaxAdsCountOnListPages => 4500;

    private static int AdsCountOnListPage => 50;

    private static string CreateAvitoUrl(string pathAndQuery) => $"https://www.avito.ru{pathAndQuery}";

    // отсекаются карточки, с пустым "id", тк там рекламный блок.
    private bool ActualCardFilter(Card card) => !string.IsNullOrEmpty(card.FilterProperty);

    private readonly IWebDriver _webDriver;
    private readonly IMapper _mapper;
    private bool lastPage = false;
    private int pageNumber = 1;
    string NextPageUrl = string.Empty;

    public Crawler(IWebDriver webDriver, IMapper mapper)
    {
        _webDriver = webDriver;
        _mapper = mapper;
    }

    public IEnumerable<(IEnumerable<Card> Cards, string FirstUrl, IEnumerable<string>? Urls, int? AdsCount)> GetCardsFromNextPageOrUrls()
    {
        int previousEmptyPages = 0;
        int reloadEmptyPages = 0;
        string firstUrl = string.Empty;

        if (pageNumber == 1)
        {
            NextPageUrl = GetStartUrl();
            firstUrl = NextPageUrl;
        }


        do
        {
            var currentUrl = NextPageUrl;
            (var page, var adsCount) = GetPage();

            int? adsCountInClassified = pageNumber == 1 ? adsCount : null;

            if (adsCount > MaxAdsCountOnListPages)
            {
                var urls = GetUrlsSplitByPrice(adsCount);
                yield return (Enumerable.Empty<Card>(), firstUrl, urls, adsCountInClassified);
                yield break;
            }
            else
            {
                var cards = NormalizePage(page);

                if (previousEmptyPages > 0)
                {
                    // убедится, что открылась следующая страница, а не редирект на 1. ИЛИ если это уже 6 пустая, то выйти.
                    if (CurrentPageFromUrl() != pageNumber || previousEmptyPages > 5)
                    {
                        yield break;
                    }
                }

                // при пустой странице, перезапускаем браузер с новым прокси, тк это баг Авито.
                if (!cards.Any() && reloadEmptyPages < 10 && pageNumber != 1)
                {
                    reloadEmptyPages++;
                    NextPageUrl = currentUrl;
                    continue;
                }

                // на регионах, где совсем нет карточек (костыль, тк иначе нельзя определить карточек нет потому что нет, или это баг авито)
                if (!cards.Any() && pageNumber == 1)
                {
                    yield return (cards, firstUrl, null, adsCountInClassified);
                    yield break;
                }

                pageNumber++;
                reloadEmptyPages = 0;

                // иногда открывается страница без карточек, надо открыть следующую, и убедиться, что она тоже пустая
                if (!cards.Any())
                {
                    previousEmptyPages++;
                    continue;
                }

                yield return (cards, firstUrl, null, adsCountInClassified);
            }
        }
        while (true);
    }

    private void LoadPage(string url, int attemptCount)
    {
        int i = 1;

        do
        {
            try
            {
                _webDriver.Navigate().GoToUrl(url);
                _webDriver.Stop();
            }
            catch (WebDriverTimeoutException)
            {
                // не всегда нормальная ситуация, подвисает загрузка каких-то элементов, для парсинга не мешает.
                if (string.IsNullOrEmpty(_webDriver.Title))
                {
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
            if (IsBlokingPage())
            {
                i++;
            }
            else
            {
                break;
            }

        }
        while (i < attemptCount);
    }

    private bool IsBlokingPage()
    {
        try
        {
            var title = _webDriver.Title;
        }
        catch
        {
            return true;
        }
        return false;
    }

    private (List<Card> Cards, int AdsCount) GetPage()
    {
        int mainElementNotFoundCounter = 0, adsCount = 0;
        IEnumerable<Card> page;

        do
        {
            try
            {
                LoadPage(NextPageUrl, 10);
                (page, adsCount) = GetPageData();

                if (!page.Any())
                {
                    break;
                }
            }
            catch (Exception) when (mainElementNotFoundCounter++ < 100)
            {
                continue;
            }
            catch
            {
                throw;
            }

            break;
        }
        while (true);

        return (page.ToList(), adsCount);
    }

    private List<Card> NormalizePage(List<Card> page)
    {
        var cards = new List<Card>();
        foreach (var card in page)
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

    private IEnumerable<string> GetUrlsSplitByPrice(int adsCount)
    {
        var startUrl = GetStartUrl();
        var rangeCount = adsCount / MaxAdsCountOnListPages;
        long pricePeek = 3_500_000;

        var priceRanges = RangeSplitHelper.StepRanges(new PriceRange<long>(1, 100_000_000), rangeCount * 2, pricePeek);

        return priceRanges.Select(_ => GenerateUrl(_, startUrl));
    }

    private IEnumerable<PriceRange> SplitRange(string url, int steps)
    {
        var result = RangeSplitHelper.StepRanges(GetRangeFromUrl(url), steps);

        if (result.Count() == 1)
        {
            var item = result.Single();
            _logger.Error("Не удалось разбить интервал от {From} до {To}", item.From, item.To);
        }

        return result;
    }

    private (IEnumerable<Card> Cards, int AdsCount) GetPageData()
    {
        var payload = GetJDataFromPage();

        if (payload is null)
        {
            return (Array.Empty<Card>(), 0);
        }

        var nextPageUrl = payload.SelectToken(AvitoParsingSettings.NextPageUrlJPath)?.Value<string>();

        if (string.IsNullOrEmpty(nextPageUrl))
        {
            lastPage = true;
        }
        else
        {
            NextPageUrl = CreateAvitoUrl(nextPageUrl);
        }

        var adsCount = payload.SelectToken(AvitoParsingSettings.TotalAdsCount)?.Value<int>() ?? 0;

        return (_mapper.Map<Card[]>(payload).Where(ActualCardFilter).ToArray(), adsCount);
    }

    private JObject? GetJDataFromPage()
    {
        var scriptElements = _webDriver.FindElements(By.TagName("script"));
        var rawPayload = FindScript(scriptElements);

        if (rawPayload is null)
        {
            return null;
        }

        var payloadRegex = new Regex(@"^window\.__initialData__\s*=\s""(.+?)"";");
        var match = payloadRegex.Match(rawPayload);
        var urlEncodedPayloadJson = match.Groups[1].Value;
        var payloadJson = HttpUtility.UrlDecode(urlEncodedPayloadJson);

        if (string.IsNullOrEmpty(payloadJson))
        {
            return null;
        }

        return JObject.Parse(payloadJson);
    }

    private string? FindScript(IEnumerable<IWebElement> source)
    {
        foreach (var element in source)
        {
            try
            {
                var content = element.GetAttribute("innerHTML");
                if (content?.StartsWith("window.__initialData__"))
                {
                    return content!;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private PriceRange GetRangeFromUrl(string url) =>
        AvitoUrlHelper.GetRangeFromUrl(url);

    private string GenerateUrl(PriceRange range, string startUrl) =>
        AvitoUrlHelper.GenerateUrl(range, startUrl, realEstateType);

    private string GetStartUrl()
        => AvitoParsingSettings.StartUrl
        .Replace("{Region}", "ulyanovskaya_oblast")
        .Replace("{RealEstateType}", "kvartiry/prodam");
}
