using Samolet.Dp.RealEstateClassifiedParser.Core.Entities;
using Samolet.Dp.RealEstateClassifiedParser.ValueObjects.Enums;
using Samolet.Dp.RealEstateClassifiedParser.ValueObjects.ValueObjects;

namespace Samolet.Dp.RealEstateClassifiedParser.Common.ClassifiedSettings.DefaultSettings;
public static class AvitoParsingDefaultSettings
{
    private const string RegexDouble = "[^0-9.,]";

    public static readonly Guid Id = Guid.Parse("b5bd35ee-778f-4438-b7d9-158d66da5aaf");

    /// <summary>
    /// Gets paramsDto value by title.
    /// </summary>
    /// <param name="paramTitle">Item title.</param>
    /// <returns>Item value.</returns>
    private static string Param(string paramTitle) =>
        $"$.paramsDto.items[?(@.title == '{paramTitle}')].description";

    /// <summary>
    /// Gets houseParam value by title.
    /// </summary>
    /// <param name="paramTitle">Item title.</param>
    /// <returns>Item value.</returns>
    private static string HouseParam(string paramTitle) =>
        $"$.item.houseParams.data.items[?(@.title == '{paramTitle}')].description";

    private static string AnyParam(string paramTitle) =>
        $"$..items[?(@.title == '{paramTitle}')].description";

    public static readonly AvitoParsingSettings AvitoParsingSettings = new()
    {
        JsonMapping = new(
            "$..data.catalog.items",
            "$..buyerItem",
            new()
            {
                // card
                { nameof(Card.FilterProperty), "$.id" },
                { nameof(Card.Url), "$.urlPath" },
                { nameof(Card.AdClassifiedId), "$.id" },
                { nameof(Card.DateTimeCreatedClassified), "$.sortTimeStamp" },
                { nameof(Card.MediaFiles), "$.images[*]['864x648']" },
                { nameof(Card.Coordinates), "$.coords['lat','lng']" },
                { nameof(Card.RealEstateType), "$..categorySlug" },
                { nameof(AdAttributes.DateTimeUpdatedClassified), "$.sortTimeStamp" },
                { nameof(AdAttributes.Address), "$.item.address" },
                { nameof(AdAttributes.Description), "$.description" },
                { nameof(AdAttributes.Price), "$.priceDetailed.value" },
                { nameof(AdAttributes.ConstructionType), "$.ga[?(@.itemID)]['house_type','garage_type','material_sten']" },
                { nameof(AdAttributes.Rooms), "$.paramsDto.items[?(@.title =~ /^(Количество комнат)|(Комнат в квартире)$/)].description" },
                { nameof(AdAttributes.IsStudio), "$.paramsDto.items[?(@.title =~ /^(Количество комнат)|(Комнат в квартире)$/)].description" },
                { nameof(AdAttributes.Renovation), "$.paramsDto.items[?(@.title =~ /Ремонт|Отделка/)].description" },
                { nameof(AdAttributes.CeilingHeight), Param("Высота потолков") },
                { nameof(AdAttributes.CombinedBathrooms), Param("Санузел") },
                { nameof(AdAttributes.SeparateBathrooms), Param("Санузел") },
                { nameof(AdAttributes.PassengerLiftsCount), HouseParam("Пассажирский лифт") },
                { nameof(AdAttributes.CargoLiftsCount), HouseParam("Грузовой лифт") },

                // ad
                { nameof(Card.AdType), "$.ga..offer_type" },
                { nameof(AdAttributes.City), "$..location.name" },
                { nameof(AdAttributes.MetroArea), "$..geo['references', 'geoReferences'][0].content" },
                { nameof(AdAttributes.Area), "$.ga[?(@.categorySlug != 'zemelnye_uchastki')]..['area', 'house_area']" },
                { nameof(AdAttributes.LivingArea), "$.ga[?(@.area)].area_live" },
                { nameof(AdAttributes.KitchenArea), "$.ga[?(@.area)].area_kitchen" },
                { nameof(AdAttributes.LotArea), "$.paramsDto.items[?(@.title =~ /Площадь( участка)?$/)].description" },
                { nameof(AdAttributes.HouseType), "$.ga[?(@.categorySlug=='doma_dachi_kottedzhi')].type" },
                { nameof(AdAttributes.GarageType), "$.ga[?(@.categorySlug=='garazhi_i_mashinomesta')].type" },
                { nameof(AdAttributes.CommercialType), "$.ga[?(@.categorySlug=='kommercheskaya_nedvizhimost')].type" },
                { nameof(AdAttributes.LotType), "$..['ga', 'paramsDto']..[?(@.categorySlug == 'zemelnye_uchastki' || @.title == 'Категория земель')]['purpose', 'description']" },
                { nameof(AdAttributes.BalconiesCount), Param("Балкон или лоджия") },
                { nameof(AdAttributes.LoggiasCount), Param("Балкон или лоджия") },
                { nameof(AdAttributes.Floor), Param("Этаж") },
                { nameof(AdAttributes.Floors), "$.ga..floors_count" },
                { nameof(AdAttributes.Seller), "$.seller.name" },
                { nameof(AdAttributes.ContactPerson), "$.seller.manager" },
                { nameof(AdAttributes.SellerType), "$.seller.labels.nominative" },
                { nameof(AdAttributes.BuiltYear), AnyParam("Год постройки") },
                { nameof(AdAttributes.WindowsView), Param("Окна") },
                { nameof(AdAttributes.Security), Param("Охрана") },
                { nameof(AdAttributes.DistanceToCity), Param("Расстояние до центра города") },
                { nameof(AdAttributes.BuildingClass), HouseParam("Класс здания") }
            }),
        NextPageUrlJPath = "$..data.catalog.pager.next",
        TotalAdsCount = "$..data.mainCount",
        ClosedAdAttributeJPath = "$..buyerItem.closedItem",
        StartUrl = "https://www.avito.ru/{Region}/{RealEstateType}?s=104",
        RegionCodes = new()
                {
                    { "Москва", "moskva" },
                    { "Санкт - Петербург", "sankt-peterburg" },
                    { "Республика Адыгея", "adygeya" },
                    { "Республика Алтай", "respublika_altay" },
                    { "Алтайский край", "altayskiy_kray" },
                    { "Амурская область", "amurskaya_oblast" },
                    { "Архангельская область", "arhangelskaya_oblast" },
                    { "Астраханская область", "astrahanskaya_oblast" },
                    { "Республика Башкортостан", "bashkortostan" },
                    { "Белгородская область", "belgorodskaya_oblast" },
                    { "Брянская область", "bryanskaya_oblast" },
                    { "Республика Бурятия", "buryatiya" },
                    { "Владимирская область", "vladimirskaya_oblast" },
                    { "Волгоградская область", "volgogradskaya_oblast" },
                    { "Вологодская область", "vologodskaya_oblast" },
                    { "Воронежская область", "voronezhskaya_oblast" },
                    { "Республика Дагестан", "dagestan" },
                    { "Еврейская автономная область", "evreyskaya_ao" },
                    { "Ивановская область", "ivanovskaya_oblast" },
                    { "Республика Ингушетия", "ingushetiya" },
                    { "Иркутская область", "irkutskaya_oblast" },
                    { "Кабардино - Балкарская Республика", "kabardino-balkariya" },
                    { "Калининградская область", "kaliningradskaya_oblast" },
                    { "Республика Калмыкия", "kalmykiya" },
                    { "Калужская область", "kaluzhskaya_oblast" },
                    { "Камчатский край", "kamchatskiy_kray" },
                    { "Карачаево - Черкесская Республика", "karachaevo-cherkesiya" },
                    { "Республика Карелия", "kareliya" },
                    { "Кемеровская область", "kemerovskaya_oblast" },
                    { "Кировская область", "kirovskaya_oblast" },
                    { "Республика Коми", "komi" },
                    { "Костромская область", "kostromskaya_oblast" },
                    { "Краснодарский край", "krasnodarskiy_kray" },
                    { "Красноярский край", "krasnoyarskiy_kray" },
                    { "Курганская область", "kurganskaya_oblast" },
                    { "Курская область", "kurskaya_oblast" },
                    { "Ленинградская область", "leningradskaya_oblast" },
                    { "Липецкая область", "lipetskaya_oblast" },
                    { "Магаданская область", "magadanskaya_oblast" },
                    { "Республика Марий Эл", "mariy_el" },
                    { "Республика Мордовия", "mordoviya" },
                    { "Московская область", "moskovskaya_oblast" },
                    { "Мурманская область", "murmanskaya_oblast" },
                    { "Ненецкий автономный округ", "nenetskiy_ao" },
                    { "Нижегородская область", "nizhegorodskaya_oblast" },
                    { "Новгородская область", "novgorodskaya_oblast" },
                    { "Новосибирская область", "novosibirskaya_oblast" },
                    { "Омская область", "omskaya_oblast" },
                    { "Оренбургская область", "orenburgskaya_oblast" },
                    { "Орловская область", "orlovskaya_oblast" },
                    { "Пензенская область", "penzenskaya_oblast" },
                    { "Пермский край", "permskiy_kray" },
                    { "Приморский край", "primorskiy_kray" },
                    { "Псковская область", "pskovskaya_oblast" },
                    { "Ростовская область", "rostovskaya_oblast" },
                    { "Рязанская область", "ryazanskaya_oblast" },
                    { "Самарская область", "samarskaya_oblast" },
                    { "Саратовская область", "saratovskaya_oblast" },
                    { "Республика Саха(Якутия)", "saha_yakutiya" },
                    { "Сахалинская область", "sahalinskaya_oblast" },
                    { "Свердловская область", "sverdlovskaya_oblast" },
                    { "Республика Северная Осетия - Алания", "severnaya_osetiya" },
                    { "Смоленская область", "smolenskaya_oblast" },
                    { "Ставропольский край", "stavropolskiy_kray" },
                    { "Тамбовская область", "tambovskaya_oblast" },
                    { "Республика Татарстан", "tatarstan" },
                    { "Тверская область", "tverskaya_oblast" },
                    { "Томская область", "tomskaya_oblast" },
                    { "Тульская область", "tulskaya_oblast" },
                    { "Республика Тыва", "tyva" },
                    { "Тюменская область", "tyumenskaya_oblast" },
                    { "Удмуртская Республика", "udmurtiya" },
                    { "Ульяновская область", "ulyanovskaya_oblast" },
                    { "Хабаровский край", "habarovskiy_kray" },
                    { "Республика Хакасия", "hakasiya" },
                    { "Ханты - Мансийский автономный округ", "hanty-mansiyskiy_ao" },
                    { "Челябинская область", "chelyabinskaya_oblast" },
                    { "Чеченская Республика", "chechenskaya_respublika" },
                    { "Чувашская Республика", "chuvashiya" },
                    { "Чукотский автономный округ", "chukotskiy_ao" },
                    { "Ямало - Ненецкий автономный округ", "yamalo-nenetskiy_ao" },
                    { "Ярославская область", "yaroslavskaya_oblast" },
                    { "Республика Крым", "respublika_krym" },
                    { "Севастополь", "sevastopol" },
                    { "Забайкальский край", "zabaykalskiy_kray" },
                },
        RealEstateTypeCodes = new()
                {
                    { RealEstateType.Apartment, "kvartiry/prodam/vtorichka" },
                    { RealEstateType.Room, "komnaty/prodam" },
                    { RealEstateType.Garage, "garazhi_i_mashinomesta/prodam" },
                    { RealEstateType.CountryEstate, "doma_dachi_kottedzhi/prodam" },
                    { RealEstateType.Land, "zemelnye_uchastki/prodam" },
                    { RealEstateType.CommercialRealEstate, "kommercheskaya_nedvizhimost/prodam" },
                },
        NormalizationRules = new()
        {
            new NormalizationRule(nameof(Card.Url), NormalizationRuleType.AddTextBefore, "https://www.avito.ru"),
            new NormalizationRule(nameof(AdAttributes.Area), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.LivingArea), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.KitchenArea), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.LotArea), NormalizationRuleType.Area),
            new NormalizationRule(nameof(AdAttributes.CeilingHeight), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.PassengerLiftsCount), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.Rooms), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.CargoLiftsCount), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.ContactPerson), NormalizationRuleType.ChangeContactPersonAvito, RegexDouble),
            NormalizationRule.ToEnum(nameof(AdAttributes.ConstructionType),
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
            NormalizationRule.ToEnum(nameof(AdAttributes.Renovation),
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
            NormalizationRule.EnumToInt(nameof(AdAttributes.BalconiesCount),
                new()
                {
                    { "Балкон и лоджия", 1 },
                    { "балкон, лоджия", 1 },
                    { "Балкон или лоджия", 1 },
                    { "балкон", 1 },
                }),
            NormalizationRule.EnumToInt(nameof(AdAttributes.LoggiasCount),
                new()
                {
                    { "Балкон и лоджия", 1 },
                    { "балкон, лоджия", 1 },
                    { "лоджия", 1 },
                }),
            NormalizationRule.ToEnum(nameof(AdAttributes.SellerType),
                new()
                {
                    { "Агентство", nameof(SellerType.Agency) },
                    { "Риелтор", nameof(SellerType.Agent) },
                    { "Компания", nameof(SellerType.Company) },
                    { "Частное лицо", nameof(SellerType.Individual) },
                }),
            NormalizationRule.EnumToInt(nameof(AdAttributes.CombinedBathrooms),
                new()
                {
                    { "совмещенный", 1 },
                    { "в доме", 1 },
                    { "в доме, на улице", 1 },
                }),
            NormalizationRule.EnumToInt(nameof(AdAttributes.SeparateBathrooms),
                new()
                {
                    { "раздельный", 1 },
                }),
            NormalizationRule.ToEnum(nameof(AdAttributes.WindowsView),
                new()
                {
                    { "во двор, на улицу, на солнечную сторону", nameof(WindowViewType.BothSides) },
                    { "во двор, на улицу", nameof(WindowViewType.BothSides) },
                    { "во двор, на солнечную сторону", nameof(WindowViewType.Backyard) },
                    { "на улицу, на солнечную сторону", nameof(WindowViewType.Street) },
                    { "во двор", nameof(WindowViewType.Backyard) },
                    { "на улицу", nameof(WindowViewType.Street) },
                }),
            NormalizationRule.ToEnum(nameof(AdAttributes.HouseType),
                new()
                {
                    { "Дом", nameof(RealEstateSubType.House) },
                    { "Дача", nameof(RealEstateSubType.Dacha) },
                    { "Коттедж", nameof(RealEstateSubType.Cottage) },
                    { "Таунхаус", nameof(RealEstateSubType.Townhouse) }
                }),
            NormalizationRule.ToEnum(nameof(AdAttributes.LotType),
                new()
                {
                    { "Сельхозназначения (СНТ, ДНП)", nameof(RealEstateSubType.Farming) },
                    { "Поселений (ИЖС)", nameof(RealEstateSubType.PrivateHousingProjects) },
                    { "Промназначения", nameof(RealEstateSubType.IndustrialUse) },
                    { "индивидуальное жилищное строительство (ИЖС)", nameof(RealEstateSubType.PrivateHousingProjects) },
                    { "садовое некоммерческое товарищество (СНТ)", nameof(RealEstateSubType.GardenNoncommercialPartnership) },
                    { "Личное подсобное хозяйство (ЛПХ)", nameof(RealEstateSubType.PrivateFarming) },
                    { "дачное некоммерческое партнёрство (ДНП)", nameof(RealEstateSubType.DachaNoncommercialPartnership) },
                    { "фермерское хозяйство", nameof(RealEstateSubType.Farming) },
                }),
            NormalizationRule.ToEnum(nameof(AdAttributes.GarageType),
                new()
                {
                    { "Гараж", nameof(RealEstateSubType.Garage) },
                    { "Машиноместо", nameof(RealEstateSubType.ParkingSpace) },
                }),
            NormalizationRule.ToEnum(nameof(AdAttributes.CommercialType),
                new()
                {
                    { "Офисное помещение", nameof(RealEstateSubType.Office) },
                    { "Помещение свободного назначения", nameof(RealEstateSubType.FreeSpace) },
                    { "Торговое помещение", nameof(RealEstateSubType.Trading) },
                    { "Производственное помещение", nameof(RealEstateSubType.Industrial) },
                    { "Складское помещение", nameof(RealEstateSubType.Storage) },
                    { "Помещение общественного питания", nameof(RealEstateSubType.Catering) },
                    { "Гостиница", nameof(RealEstateSubType.Hotel) },
                    { "Автосервис", nameof(RealEstateSubType.AutoRepairShop) },
                    { "Здание", nameof(RealEstateSubType.CommercialBuilding) },
                }),
            new NormalizationRule(nameof(AdAttributes.Security), NormalizationRuleType.StringToBool) { TrueValues = new() { "Да" } },
            new NormalizationRule(nameof(AdAttributes.DistanceToCity), NormalizationRuleType.Regex, RegexDouble),
            new NormalizationRule(nameof(AdAttributes.Floor), NormalizationRuleType.Floor),
            new NormalizationRule(nameof(AdAttributes.IsStudio), NormalizationRuleType.StringToBool) { TrueValues = new() { "студия" } }
        },
    };
}
