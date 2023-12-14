namespace RealEstateClassificator.Core.Settings;

public record NormalizationRule
{
    public string AttributeName { get; set; }

    public NormalizationRule(string attributeName, NormalizationRuleType ruleType, string? patternValue = null)
    {
        AttributeName = attributeName;
        RuleType = ruleType;
        PatternValue = patternValue;
    }

    public NormalizationRuleType RuleType { get; set; }

    public string? PatternValue { get; set; }

    public List<(string, string)>? ReplacementValues { get; set; }

    /// <summary>
    /// Словарь: часть значения в классифайде - Enum.
    /// </summary>
    public Dictionary<string, string>? EnumMapping { get; set; }

    public Dictionary<string, int>? IntMapping { get; set; }

    public List<string>? TrueValues { get; set; }

    public static NormalizationRule ToEnum(string attributeName, Dictionary<string, string> enumMapping) =>
        new(attributeName, NormalizationRuleType.ToEnum)
        {
            EnumMapping = enumMapping
        };

    public static NormalizationRule EnumToInt(string attributeName, Dictionary<string, int> intMapping) =>
    new(attributeName, NormalizationRuleType.EnumToInt)
    {
        IntMapping = intMapping
    };

    public static NormalizationRule EnumToIntOrInt(string attributeName, Dictionary<string, int> intMapping) =>
    new(attributeName, NormalizationRuleType.EnumToIntOrInt)
    {
        IntMapping = intMapping
    };
}
