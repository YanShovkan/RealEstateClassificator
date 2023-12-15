namespace RealEstateClassificator.Core.Settings;

public enum NormalizationRuleType
{
    None,
    Regex,
    AddTextBefore,
    StringToBool,
    Floor,
    ToEnum,
    EnumToInt
}
