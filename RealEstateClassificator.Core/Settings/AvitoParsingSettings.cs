using RealEstateClassificator.Common.Enums;
using RealEstateClassificator.Core.Settings;
using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.CoreSettings.DefaultSettings;
public static class AvitoParsingSettings
{
    public static int AdNewMaxAttemptCount => 20;
    public static int AdExistMaxAttemptCount => 30;

    private const string RegexDouble = "[^0-9.,]";

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
                { nameof(Card.FilterProperty), "$.id" },
                { nameof(Card.Url), "$.urlPath" },
                { nameof(Card.MediaFiles), "$.images[*]['864x648']" },
                { nameof(Card.Address), "$.item.address" },
                { nameof(Card.Description), "$.description" },
                { nameof(Card.Price), "$.priceDetailed.value" },
                { nameof(Card.ConstructionType), "$.ga[?(@.itemID)]['house_type','garage_type','material_sten']" },
                { nameof(Card.Rooms), "$.paramsDto.items[?(@.title =~ /^(Количество комнат)|(Комнат в квартире)$/)].description" },
                { nameof(Card.IsStudio), "$.paramsDto.items[?(@.title =~ /^(Количество комнат)|(Комнат в квартире)$/)].description" },
                { nameof(Card.Renovation), "$.paramsDto.items[?(@.title =~ /Ремонт|Отделка/)].description" },
                { nameof(Card.CeilingHeight), Param("Высота потолков") },
                { nameof(Card.CombinedBathrooms), Param("Санузел") },
                { nameof(Card.SeparateBathrooms), Param("Санузел") },
                { nameof(Card.PassengerLiftsCount), HouseParam("Пассажирский лифт") },
                { nameof(Card.CargoLiftsCount), HouseParam("Грузовой лифт") },
                { nameof(Card.City), "$..location.name" },
                { nameof(Card.District), "$..geo['references', 'geoReferences'][0].content" },
                { nameof(Card.TotalArea), "$.ga[?(@.categorySlug != 'zemelnye_uchastki')]..['area', 'house_area']" },
                { nameof(Card.LivingArea), "$.ga[?(@.area)].area_live" },
                { nameof(Card.KitchenArea), "$.ga[?(@.area)].area_kitchen" },
                { nameof(Card.BalconiesCount), Param("Балкон или лоджия") },
                { nameof(Card.LoggiasCount), Param("Балкон или лоджия") },
                { nameof(Card.Floor), Param("Этаж") },
                { nameof(Card.Floors), "$.ga..floors_count" },
                { nameof(Card.BuiltYear), AnyParam("Год постройки") },
                { nameof(Card.WindowsView), Param("Окна") },
                { nameof(Card.Security), Param("Охрана") },
                { nameof(Card.DistanceToCity), Param("Расстояние до центра города") }
            });

    public static List<NormalizationRule> NormalizationRules = new()
        {
            new NormalizationRule(nameof(Card.Url), NormalizationRuleType.AddTextBefore, "https://www.avito.ru"),
            new NormalizationRule(nameof(Card.TotalArea), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(Card.LivingArea), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(Card.KitchenArea), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(Card.CeilingHeight), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(Card.PassengerLiftsCount), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(Card.Rooms), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(Card.CargoLiftsCount), NormalizationRuleType.Regex, RegexDouble),
            NormalizationRule.ToEnum(nameof(Card.ConstructionType),
                new()
                {
                    { "Панельный", nameof(WallMaterialType.Prefabricated) },
                    { "Кирпичный", nameof(WallMaterialType.Brick) },
                    { "Кирпич", nameof(WallMaterialType.Brick) },
                    { "Монолитный", nameof(WallMaterialType.Monolithic) },
                    { "Монолитнокирпичный", nameof(WallMaterialType.MonolithicBrick) },
                    { "2308811", nameof(WallMaterialType.MonolithicBrick) },
                    { "Металл", nameof(WallMaterialType.Metal) },
                    { "Металлический", nameof(WallMaterialType.Metal) },
                    { "бревно", nameof(WallMaterialType.Log) },
                    { "Брус", nameof(WallMaterialType.Timber) },
                    { "Деревянный", nameof(WallMaterialType.Wooden) },
                    { "Железобетонный", nameof(WallMaterialType.Ferroconcrete) },
                    { "Ж/б панели", nameof(WallMaterialType.Ferroconcrete) },
                    { "Экспериментальные материалы", nameof(WallMaterialType.Experimental) },
                    { "Газоблоки", nameof(WallMaterialType.GasBlock) },
                    { "Блочный", nameof(WallMaterialType.Block) },
                    { "Пеноблоки", nameof(WallMaterialType.FoamBlock) },
                    { "Сэндвич-панели", nameof(WallMaterialType.SandwichPanel) },
                }),
            NormalizationRule.ToEnum(nameof(Card.Renovation),
                new()
                {
                    { "косметический", nameof(RenovationType.Cosmetic) },
                    { "евро", nameof(RenovationType.Euro) },
                    { "евроремонт", nameof(RenovationType.Euro) },
                    { "дизайнерский", nameof(RenovationType.Design) },
                    { "требуется ремонт", nameof(RenovationType.Required) },
                    { "требует ремонта", nameof(RenovationType.Required) },
                    { "обычный", nameof(RenovationType.Standard) },
                    { "хороший", nameof(RenovationType.Standard) },
                    { "чистовая", nameof(RenovationType.Finishing) },
                    { "черновая", nameof(RenovationType.Rough) },
                    { "офисная", nameof(RenovationType.Office) },
                    { "без отделки", nameof(RenovationType.None) },
                }),
            NormalizationRule.EnumToInt(nameof(Card.BalconiesCount),
                new()
                {
                    { "Балкон и лоджия", 1 },
                    { "балкон, лоджия", 1 },
                    { "Балкон или лоджия", 1 },
                    { "балкон", 1 },
                }),
            NormalizationRule.EnumToInt(nameof(Card.LoggiasCount),
                new()
                {
                    { "Балкон и лоджия", 1 },
                    { "балкон, лоджия", 1 },
                    { "лоджия", 1 },
                }),
            NormalizationRule.EnumToInt(nameof(Card.CombinedBathrooms),
                new()
                {
                    { "совмещенный", 1 },
                    { "в доме", 1 },
                    { "в доме, на улице", 1 },
                }),
            NormalizationRule.EnumToInt(nameof(Card.SeparateBathrooms),
                new()
                {
                    { "раздельный", 1 },
                }),
            NormalizationRule.ToEnum(nameof(Card.WindowsView),
                new()
                {
                    { "во двор, на улицу, на солнечную сторону", nameof(WindowViewType.BothSides) },
                    { "во двор, на улицу", nameof(WindowViewType.BothSides) },
                    { "во двор, на солнечную сторону", nameof(WindowViewType.Backyard) },
                    { "на улицу, на солнечную сторону", nameof(WindowViewType.Street) },
                    { "во двор", nameof(WindowViewType.Backyard) },
                    { "на улицу", nameof(WindowViewType.Street) },
                }),
            new NormalizationRule(nameof(Card.Security), NormalizationRuleType.StringToBool) { TrueValues = new() { "Да" } },
            new NormalizationRule(nameof(Card.DistanceToCity), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(Card.Floor), NormalizationRuleType.Floor),
            new NormalizationRule(nameof(Card.IsStudio), NormalizationRuleType.StringToBool) { TrueValues = new() { "студия" } }
    };

    private static string Param(string paramTitle) =>
        $"$.paramsDto.items[?(@.title == '{paramTitle}')].description";

    private static string HouseParam(string paramTitle) =>
        $"$.item.houseParams.data.items[?(@.title == '{paramTitle}')].description";

    private static string AnyParam(string paramTitle) =>
        $"$..items[?(@.title == '{paramTitle}')].description";
}
