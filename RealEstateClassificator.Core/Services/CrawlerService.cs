using AutoMapper;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using RealEstateClassificator.Common.ValueObjects;
using RealEstateClassificator.Core.Dto;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.CoreSettings.DefaultSettings;
using RealEstateClassificator.Dal.Entities;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace RealEstateClassificator.Core.Services;

/// <summary>
/// Сборщик карточек объявлений Авито.
/// </summary>
public class CrawlerService : ICrawler
{
    private static int MaxAdsCountOnListPages => 2500;

    private static string CreateAvitoUrl(string pathAndQuery) => $"https://www.avito.ru{pathAndQuery}";

    // отсекаются карточки, с пустым "id", тк там рекламный блок.
    private bool ActualCardFilter(CardDto card) => !string.IsNullOrEmpty(card.FilterProperty);

    private readonly IWebDriver _webDriver;
    private readonly IMapper _mapper;
    private bool _lastPage;
    private int _pageNumber = 1;
    private string _nextPageUrl = string.Empty;

    public CrawlerService(IMapper mapper)
    {
        _webDriver = WebDriver.SetupWebDriver();
        _mapper = mapper;
    }

    public async IAsyncEnumerable<IEnumerable<Card>> GetCardsFromNextPageOrUrls()
    {
        await Task.Delay(1);
        int previousEmptyPages = 0;
        int reloadEmptyPages = 0;

        if (_pageNumber == 1)
        {
            _nextPageUrl = GetStartUrl();
        }

        do
        {
            var currentUrl = _nextPageUrl;
            (var page, var adsCount) = GetPage();

            if (adsCount > MaxAdsCountOnListPages)
            {
                var urls = GetUrlsSplitByPrice(adsCount);
                yield return Enumerable.Empty<Card>();
                yield break;
            }
            else
            {
                var cards = _mapper.Map<List<Card>>(page);

                if (previousEmptyPages > 0)
                {
                    // убедится, что открылась следующая страница, а не редирект на 1. ИЛИ если это уже 6 пустая, то выйти.
                    if (CurrentPageFromUrl() != _pageNumber || previousEmptyPages > 5)
                    {
                        yield break;
                    }
                }

                // при пустой странице, перезапускаем браузер с новым прокси, тк это баг Авито.
                if (!cards.Any() && reloadEmptyPages < 10 && _pageNumber != 1)
                {
                    reloadEmptyPages++;
                    _nextPageUrl = currentUrl;
                    continue;
                }

                // на регионах, где совсем нет карточек (костыль, тк иначе нельзя определить карточек нет потому что нет, или это баг авито)
                if (!cards.Any() && _pageNumber == 1)
                {
                    yield return cards;
                    yield break;
                }

                _pageNumber++;
                reloadEmptyPages = 0;

                // иногда открывается страница без карточек, надо открыть следующую, и убедиться, что она тоже пустая
                if (!cards.Any())
                {
                    previousEmptyPages++;
                    continue;
                }

                yield return cards;
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

    private (List<CardDto> Cards, int AdsCount) GetPage()
    {
        int mainElementNotFoundCounter = 0, adsCount = 0;
        IEnumerable<CardDto> page;

        do
        {
            try
            {
                LoadPage(_nextPageUrl, 10);
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

    private (IEnumerable<CardDto> Cards, int AdsCount) GetPageData()
    {
        var payload = GetJDataFromPage();

        if (payload is null)
        {
            return (Array.Empty<CardDto>(), 0);
        }

        var nextPageUrl = payload.SelectToken(AvitoParsingSettings.NextPageUrlJPath)?.Value<string>();

        if (string.IsNullOrEmpty(nextPageUrl))
        {
            _lastPage = true;
        }
        else
        {
            _nextPageUrl = CreateAvitoUrl(nextPageUrl);
        }

        var adsCount = payload.SelectToken(AvitoParsingSettings.TotalAdsCount)?.Value<int>() ?? 0;

        return (_mapper.Map<CardDto[]>(payload).Where(ActualCardFilter).ToArray(), adsCount);
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
                if (content?.StartsWith("window.__initialData__") ?? false)
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

        var priceRanges = RangeSplitHelper.StepRanges(new PriceRange(1, 100_000_000), rangeCount * 2, pricePeek);

        return priceRanges.Select(_ => AvitoUrlHelper.GenerateUrl(_, startUrl));
    }

    private IEnumerable<PriceRange> SplitRange(string url, int steps)
        => RangeSplitHelper.StepRanges(AvitoUrlHelper.GetRangeFromUrl(url), steps);

    private string GetStartUrl()
        => AvitoParsingSettings.StartUrl
        .Replace("{Region}", "ulyanovskaya_oblast")
        .Replace("{RealEstateType}", "kvartiry/prodam");
}
