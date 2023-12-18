using AutoMapper;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using RealEstateClassificator.Core.Dto;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Core.Settings;
using RealEstateClassificator.Dal.Entities;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Web;

namespace RealEstateClassificator.Core.Services;

public class CardParserService : ICardParserService
{

    private readonly IWebDriver _webDriver;
    private readonly IMapper _mapper;

    public CardParserService(IMapper mapper)
    {
        _webDriver = WebDriver.SetupWebDriver();
        _mapper = mapper;
    }

    public void ParseRealEstates(IEnumerable<Card> cards)
    {
        foreach (var card in cards)
        {
            GetRealEstate(card);
        }
    }

    private void GetRealEstate(Card card)
    {
        JObject? jsonData = null;


        LoadPage(card.Url);

        if (Is404Page())
        {
            //удаляем
        }

        jsonData = GetJDataFromPage();


        if (jsonData == null)
        {
            //удаляем
        }

        card = _mapper.Map<Card>(jsonData!.SelectToken(AvitoParsingSettings.JsonMapping.AdSelector));
        //сохраняем карту
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

    private bool Is404Page()
    {
        return _webDriver.Title == "Ошибка 404. Страница не найдена";
    }
}

