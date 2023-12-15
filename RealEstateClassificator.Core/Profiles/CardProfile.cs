using AutoMapper;
using Newtonsoft.Json.Linq;
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
            .ForMember(_ => _.Floors, opt => opt.MapFrom(_ => Convert.ToInt32(_.Floors)));

        CreateMap<JObject, CardDto[]>()
           .ConvertUsing((src, _, ctx) => ctx.Mapper.Map<CardDto[]>(src.SelectToken(AvitoParsingSettings.JsonMapping.ItemListSelector)?.Children().ToArray()));

        // Normalize string array
        CreateMap<string[], string>()
            .ConvertUsing(_ => string.Join(' ', _));

        CreateMap<string, List<string>>()
            .ConvertUsing(_ => new() { _ });

        var cardMap = CreateMap<JToken, CardDto>();
    }

    private static string?[] SelectValue(JToken source, string jsonPath) =>
        source.SelectTokens(jsonPath).SelectMany(ConvertToken).ToArray();

    private static string?[] ConvertToken(JToken source) =>
        source switch
        {
            JArray array => array.ToObject<string[]>() ?? Array.Empty<string>(),
            _ => new[] { source.ToString() }

            // _ => new[] { source.Value<string?>() }
        };

    private static object? SingleOrArray(IEnumerable<string?> source) =>
        !source.Any() ? null : source.Count() == 1 ? source.Single() : source.ToArray();

    private static void MapProperty<TDestination>(IMappingExpression<JToken, TDestination> map, string destinationKey, string sourcePath, Func<string?[], object?>? afterMap = default)
    {
        if (typeof(TDestination).GetProperty(destinationKey) is not null)
        {
            if (afterMap is null)
            {
                map.ForMember(destinationKey, _ => _.MapFrom(src => SelectValue(src, sourcePath)));
            }
            else
            {
                map.ForMember(destinationKey, _ => _.MapFrom(src => afterMap(SelectValue(src, sourcePath))));
            }
        }
    }
}