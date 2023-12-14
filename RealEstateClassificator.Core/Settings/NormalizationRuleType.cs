namespace RealEstateClassificator.Core.Settings;

public enum NormalizationRuleType
{
    None = 0,
    Regex = 1,
    Replacement = 2,
    ToEnum = 3,
    AddTextBefore = 4,
    ChangeContactPersonAvito = 5,
    RoomCount = 6,
    EnumToInt = 7,
    BoolToInt = 8,
    DeveloperNameYandex = 9,
    Area = 10,
    SellerTypeYandex = 11,
    StringToBool = 12,
    SellerTypeCian = 13,
    CombinedBathroomsCian = 14,
    SellerCian = 15,
    SeparatedBathroomsCian = 16,
    EnumToIntOrInt = 17,
    Floor = 18,
}
