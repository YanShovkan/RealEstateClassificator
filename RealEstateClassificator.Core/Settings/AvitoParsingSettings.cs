using RealEstateClassificator.Core.Dto;

namespace RealEstateClassificator.Core.Settings;
public static class AvitoParsingSettings
{
    public static int AdNewMaxAttemptCount => 20;
    public static int AdExistMaxAttemptCount => 30;

    public static Guid Id = new Guid("b5bd35ee-778f-4438-b7d9-158d66da5aaf");
    public static string NextPageUrlJPath = "$..data.catalog.pager.next";
    public static string StartUrl = "https://www.avito.ru/{Region}/{RealEstateType}?s=104";
    public static string TotalAdsCount = "$..data.mainCount";
    public static string ClosedAdAttributeJPath = "$..buyerItem.closedItem";

    public static JsonMapping JsonMapping = new(
            "$..data.catalog.items",
            "$..buyerItem",
            new()
            {
                { nameof(CardDto.FilterProperty), "$.id" },
                { nameof(CardDto.Url), "$.urlPath" },
                { nameof(CardDto.Address), "$.item.address" },
                { nameof(CardDto.Description), "$.description" },
                { nameof(CardDto.Price), "$.priceDetailed.value" },
                { nameof(CardDto.Rooms), "$.paramsDto.items[?(@.title =~ /^(Количество комнат)|(Комнат в квартире)$/)].description" },
                { nameof(CardDto.IsStudio), "$.paramsDto.items[?(@.title =~ /^(Количество комнат)|(Комнат в квартире)$/)].description" },
                { nameof(CardDto.Renovation), "$.paramsDto.items[?(@.title =~ /Ремонт|Отделка/)].description" },
                { nameof(CardDto.CombinedBathrooms), Param("Санузел") },
                { nameof(CardDto.SeparateBathrooms), Param("Санузел") },
                { nameof(CardDto.PassengerLiftsCount), HouseParam("Пассажирский лифт") },
                { nameof(CardDto.CargoLiftsCount), HouseParam("Грузовой лифт") },
                { nameof(CardDto.City), "$..location.name" },
                { nameof(CardDto.District), "$..geo['references', 'geoReferences'][0].content" },
                { nameof(CardDto.TotalArea), "$.ga[?(@.categorySlug != 'zemelnye_uchastki')]..['area', 'house_area']" },
                { nameof(CardDto.LivingArea), "$.ga[?(@.area)].area_live" },
                { nameof(CardDto.KitchenArea), "$.ga[?(@.area)].area_kitchen" },
                { nameof(CardDto.BalconiesCount), Param("Балкон или лоджия") },
                { nameof(CardDto.Floor), Param("Этаж") },
                { nameof(CardDto.Floors), "$.ga..floors_count" },
                { nameof(CardDto.BuiltYear), AnyParam("Год постройки") },
                { nameof(CardDto.DistanceToCity), Param("Расстояние до центра города") }
            });

    private static string Param(string paramTitle) =>
        $"$.paramsDto.items[?(@.title == '{paramTitle}')].description";

    private static string HouseParam(string paramTitle) =>
        $"$.item.houseParams.data.items[?(@.title == '{paramTitle}')].description";

    private static string AnyParam(string paramTitle) =>
        $"$..items[?(@.title == '{paramTitle}')].description";
}
