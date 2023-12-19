using AutoMapper;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using RealEstateClassificator.Core.Dto;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Core.Settings;
using RealEstateClassificator.Dal.Entities;
using RealEstateClassificator.Dal.Interfaces;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Web;

namespace RealEstateClassificator.Core.Services;

public class CardParserService : ICardParserService
{
    private readonly ICommandRepository<Card> _commandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IWebDriver _webDriver;

    public CardParserService(ICommandRepository<Card> commandRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _commandRepository = commandRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _webDriver = WebDriver.SetupWebDriver();
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
        LoadPage(card.Url);

        if (Is404Page())
        {
            return;
        }

        var jsonData = GetJDataFromPage();

        if (jsonData is null)
        {
            return;
        }

        var cardDto = _mapper.Map<CardDto>(jsonData!.SelectToken(AvitoParsingSettings.JsonMapping.AdSelector));
        var parsedCard = _mapper.Map<Card>(cardDto);
        card = EnrichCard(card, parsedCard);
        _commandRepository.CreateAsync(card);
        _unitOfWork.Commit();
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

    private Card EnrichCard(Card card, Card parsedCard) =>
        new()
        {
            Id = card.Id,
            Url = card.Url,
            Price = card.Price,
            Description = card.Description,
            City = parsedCard.City,
            District = parsedCard.District,
            Address = parsedCard.Address,
            Floor = parsedCard.Floor,
            Floors = parsedCard.Floors,
            Rooms = parsedCard.Rooms,
            TotalArea = parsedCard.TotalArea,
            LivingArea = parsedCard.LivingArea,
            KitchenArea = parsedCard.KitchenArea,
            Renovation = parsedCard.Renovation,
            CombinedBathrooms = parsedCard.CombinedBathrooms,
            SeparateBathrooms = parsedCard.SeparateBathrooms,
            BalconiesCount = parsedCard.BalconiesCount,
            DistanceToCity = parsedCard.DistanceToCity,
            BuiltYear = parsedCard.BuiltYear,
            PassengerLiftsCount = parsedCard.PassengerLiftsCount,
            CargoLiftsCount = parsedCard.CargoLiftsCount,
            IsStudio = parsedCard.IsStudio
        };
}

