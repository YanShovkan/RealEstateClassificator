using AutoMapper;
using Newtonsoft.Json.Linq;
using RealEstateClassificator.Common.Enums;
using RealEstateClassificator.Core.Dto;
using RealEstateClassificator.Core.Settings;
using RealEstateClassificator.CoreSettings.DefaultSettings;
using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Core.Profiles;

public class CardProfile : Profile
{
    public CardProfile()
    {
        CreateMap<CardDto, Card>()
            .ForMember(_ => _.Price, opt => opt.MapFrom(_ => Convert.ToInt64(_.Price)))
            .ForMember(_ => _.Floor, opt => opt.MapFrom(_ => Convert.ToInt32(_.Floor)))
            .ForMember(_ => _.Floors, opt => opt.MapFrom(_ => Convert.ToInt32(_.Floors)))
            .ForMember(_ => _.Rooms, opt => opt.MapFrom(_ => Convert.ToInt32(_.Rooms)))
            .ForMember(_ => _.TotalArea, opt => opt.MapFrom(_ => Convert.ToDouble(_.TotalArea)))
            .ForMember(_ => _.LivingArea, opt => opt.MapFrom(_ => Convert.ToDouble(_.LivingArea)))
            .ForMember(_ => _.KitchenArea, opt => opt.MapFrom(_ => Convert.ToDouble(_.KitchenArea)))
            .ForMember(_ => _.Renovation, opt => opt.MapFrom(_ => ParseRenovationType(_.Renovation)))
            .ForMember(_ => _.CombinedBathrooms, opt => opt.MapFrom(_ => ParseCombinedBathrooms(_.CombinedBathrooms)))
            .ForMember(_ => _.SeparateBathrooms, opt => opt.MapFrom(_ => ParseSeparateBathrooms(_.SeparateBathrooms)))
            .ForMember(_ => _.BalconiesCount, opt => opt.MapFrom(_ => ParseBalconiesCount(_.BalconiesCount)))
            .ForMember(_ => _.DistanceToCity, opt => opt.MapFrom(_ => Convert.ToDouble(_.DistanceToCity)))
            .ForMember(_ => _.BuiltYear, opt => opt.MapFrom(_ => Convert.ToInt32(_.BuiltYear)))
            .ForMember(_ => _.PassengerLiftsCount, opt => opt.MapFrom(_ => Convert.ToInt32(_.PassengerLiftsCount)))
            .ForMember(_ => _.CargoLiftsCount, opt => opt.MapFrom(_ => Convert.ToInt32(_.CargoLiftsCount)))
            .ForMember(_ => _.IsStudio, opt => opt.MapFrom(_ => ParseIsStudio(_.IsStudio)));

        CreateMap<JObject, CardDto[]>()
           .ConvertUsing((src, _, ctx) => ctx.Mapper.Map<CardDto[]>(src.SelectToken(AvitoParsingSettings.JsonMapping.ItemListSelector)?.Children().ToArray()));

        var cardMap = CreateMap<JToken, CardDto>();

        foreach (var (destinationKey, sourcePath) in AvitoParsingSettings.JsonMapping.MembersMap)
        {
            MapProperty(cardMap, destinationKey, sourcePath);
        }
    }


    private RenovationType ParseRenovationType(string? renovation) =>
        renovation switch
        {
            "косметический" => RenovationType.Cosmetic,
            "евро" => RenovationType.Euro,
            "евроремонт" => RenovationType.Euro,
            "дизайнерский" => RenovationType.Design,
            "требуется ремонт" => RenovationType.Required,
            "требует ремонта" => RenovationType.Required,
            "обычный" => RenovationType.Standard,
            "хороший" => RenovationType.Standard,
            "чистовая" => RenovationType.Finishing,
            "черновая" => RenovationType.Rough,
            _ => RenovationType.None
        };

    private int ParseCombinedBathrooms(string? combinedBathrooms) =>
        combinedBathrooms switch
        {
            "совмещенный" => 1,
            _ => 0
        };

    private int ParseSeparateBathrooms(string? separateBathrooms) =>
       separateBathrooms switch
       {
           "раздельный" => 1,
           _ => 0
       };

    private int ParseBalconiesCount(string? balconiesCount) =>
       balconiesCount switch
       {
           "Балкон и лоджия" => 2,
           "балкон, лоджия" => 2,
           "Балкон или лоджия" => 1,
           "балкон" => 1,
           "лоджия" => 1,
           _ => 0
       };

    private bool ParseIsStudio(string? IsStidio) =>
        IsStidio switch
    {
        "да" => true,
        _ => false
    };

    private static string?[] SelectValue(JToken source, string jsonPath) =>
        source.SelectTokens(jsonPath).SelectMany(ConvertToken).ToArray();

    private static string?[] ConvertToken(JToken source) =>
        source switch
        {
            JArray array => array.ToObject<string[]>() ?? Array.Empty<string>(),
            _ => [source.ToString()]
        };

    private static void MapProperty<TDestination>(IMappingExpression<JToken, TDestination> map, string destinationKey, string sourcePath, Func<string?[], object?>? afterMap = default)
    {
        if (typeof(TDestination).GetProperty(destinationKey) is not null)
        {
            if (afterMap is null)
            {
                map.ForMember(destinationKey, _ => _.MapFrom(src => SelectValue(src, sourcePath).FirstOrDefault()));
            }
            else
            {
                map.ForMember(destinationKey, _ => _.MapFrom(src => afterMap(SelectValue(src, sourcePath))));
            }
        }
    }
}