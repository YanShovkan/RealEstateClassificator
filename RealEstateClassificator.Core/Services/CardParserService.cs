using AutoMapper;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using RealEstateClassificator.Core.Dto;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Dal.Entities;
using System.Text.RegularExpressions;
using System.Web;

namespace RealEstateClassificator.Core.Services;

public class CardParserService : ICardParserService
{

    private readonly IWebDriver _webDriver;
    private readonly IMapper _mapper;
    private string _nextPageUrl = string.Empty;
    private int _pageNumber = 1;

    public CardParserService(IMapper mapper)
    {
        _webDriver = WebDriver.SetupWebDriver();
        _mapper = mapper;
    }

    public void ParseRealEstates(IEnumerable<Card> cards)
    {
        var cardsDto = new List<CardDto>();
        foreach (var card in cards)
        {
            _nextPageUrl = card.Url;
            var page = GetPage();
            cardsDto.AddRange(page);
        }
    }

    private void LoadPage(string url)
    {
        int i = 1;

        do
        {
            try
            {
                _webDriver.Navigate().GoToUrl(url);
            }
            catch { }

            if (IsBlokingPage())
            {
                i++;
            }
            else
            {
                break;
            }
        }
        while (i < 3);
    }

    private IEnumerable<CardDto> GetPage()
    {
        LoadPage(_nextPageUrl);
        return GetPageData();
    }


    private IEnumerable<CardDto> GetPageData()
    {
        var payload = GetJDataFromPage();

        if (payload is null)
        {
            return Array.Empty<CardDto>();
        }

        _nextPageUrl = $"https://www.avito.ru/ulyanovskaya_oblast/kvartiry/prodam-ASgBAgICAUSSA8YQ?p={_pageNumber}";

        return _mapper.Map<CardDto[]>(payload).ToArray();
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
}

