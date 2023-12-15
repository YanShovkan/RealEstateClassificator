using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Core.Settings;
using RealEstateClassificator.Dal.Entities;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RealEstateClassificator.Core.Services;
public class CardNormalizator : ICardNormalizator
{
     private const string RegexDouble = "[^0-9.,]";


    private readonly IReadOnlyCollection<NormalizationRule> _rules;

    public CardNormalizator(IReadOnlyCollection<NormalizationRule> rules)
    {
        _rules = rules;
    }

    /// <inheritdoc/>
    public void Normalize(Card card)
    {
        foreach (var propertyInfo in typeof(Card).GetProperties())
        {
            var rule = _rules.FirstOrDefault(_ => _.AttributeName == propertyInfo.Name);

            if (rule == null)
            {
                continue;
            }

            switch (rule.RuleType)
            {
                case NormalizationRuleType.AddTextBefore:
                    propertyInfo.SetValue(card, rule.PatternValue + propertyInfo.GetValue(card)!.ToString());
                    break;

                default:
                    break;
            }
        }

        Normalize(card);

        card.SetCoordinates(_deduplicationOptionSnapshot.H3IndexResolution);
    }

    /// <inheritdoc/>
    public void Normalize(AdAttributes adAttributes, bool fromFeed = false)
    {
        foreach (var (name, attribute) in adAttributes.GetAttributeDictionary())
        {
            try
            {
                var rule = _rules.FirstOrDefault(_ => _.AttributeName == name);

                if (attribute is null)
                {
                    continue;
                }

                var attributeType = attribute.ValueType;

                switch (rule?.RuleType)
                {
                    case NormalizationRuleType.Regex:
                        attribute.TryNormalize(ChangeType(Regex.Replace(attribute.Value, rule.PatternValue ?? string.Empty, string.Empty), attributeType));
                        break;

                    case NormalizationRuleType.Replacement:
                    case null:
                        attribute.TryNormalize(ChangeType(attribute.Value, attributeType));
                        break;

                    case NormalizationRuleType.ToEnum:
                        NormalizeToEnum(rule, attribute, attributeType);
                        break;

                    case NormalizationRuleType.AddTextBefore:
                        if (attributeType.IsArray)
                        {
                            var stringArray = (string[])ChangeType(attribute.Value, attributeType)!;
                            attribute.TryNormalize(stringArray
                                .Where(s => !string.IsNullOrEmpty(s))
                                .Select(s => rule.PatternValue + s)
                                .ToArray());
                        }
                        else
                        {
                            attribute.TryNormalize(ChangeType(rule.PatternValue + attribute.Value, attributeType));
                        }

                        break;

                    case NormalizationRuleType.ChangeContactPersonAvito:
                        attribute.TryNormalize(ChangeType(attribute.Value, attributeType));

                        if (string.Equals(adAttributes.SellerType?.Value, "Частное лицо", StringComparison.InvariantCultureIgnoreCase))
                        {
                            attribute.TryNormalize("Пользователь");
                        }

                        break;

                    case NormalizationRuleType.RoomCount:
                        attribute.TryNormalize(int.TryParse(attribute.Value, out var value) ? value : 0);
                        break;

                    case NormalizationRuleType.EnumToInt:
                        var intValue = rule.IntMapping!.FirstOrDefault(d => attribute.Value.Equals(d.Key, StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrEmpty(intValue.Key))
                        {
                            attribute.TryNormalize(intValue.Value);
                        }

                        break;

                    case NormalizationRuleType.EnumToIntOrInt:
                        var intValueMapping = rule.IntMapping!.FirstOrDefault(d => attribute.Value.Equals(d.Key, StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrEmpty(intValueMapping.Key))
                        {
                            attribute.TryNormalize(intValueMapping.Value);
                        }
                        else
                        {
                            attribute.TryNormalize(ChangeType(attribute.Value, attributeType));
                        }

                        break;

                    case NormalizationRuleType.BoolToInt:
                        attribute.TryNormalize(string.Equals(attribute.Value, "true", StringComparison.OrdinalIgnoreCase) ? 1 : 0);
                        break;

                    case NormalizationRuleType.DeveloperNameYandex:
                        attribute.TryNormalize(ChangeType(string.Empty, attributeType));
                        if (string.Equals(adAttributes.SellerType?.Value, "DEVELOPER", StringComparison.InvariantCultureIgnoreCase))
                        {
                            attribute.TryNormalize(ChangeType(attribute.Value, attributeType));
                        }

                        break;

                    case NormalizationRuleType.Area:
                        var digitsString = Regex.Replace(attribute.Value, RegexDouble, string.Empty);
                        var normalizedDigits = digitsString.TrimEnd().TrimEnd(',').TrimEnd('.').NormalizeDigit();
                        var numberValue = decimal.Parse(normalizedDigits, NumberStyles.Any, CultureInfo.InvariantCulture);
                        attribute.TryNormalize((double)(numberValue * GetAreaUnitCoefficient(attribute.Value)));

                        break;

                    

                    case NormalizationRuleType.StringToBool:
                        if (rule.TrueValues?.Contains(attribute.Value) ?? false)
                        {
                            attribute.TryNormalize(true);
                        }
                        else
                        {
                            attribute.TryNormalize(false);
                        }

                        break;

                   

                    case NormalizationRuleType.Floor:
                        if (attribute.Value.Equals("цокольный"))
                        {
                            attribute.TryNormalize(-1);
                        }
                        else
                        {
                            var floorNumber = attribute.Value.Split(' ')[0];
                            attribute.TryNormalize(int.TryParse(floorNumber, out var normalizedFloorNumber) ? normalizedFloorNumber : 0);
                        }

                        break;

                    default:
                        attribute.TryNormalize(ChangeType(attribute.Value, attributeType));
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    private static int GetAreaUnitCoefficient(string areaValue)
    {
        var hectare = new[] { "hectare", "гектар" };
        var sotka = new[] { "are", "sotka", "сот.", "сотка", "соток" };

        if (hectare.Any(v => areaValue.Contains(v)))
        {
            return 10000;
        }

        if (sotka.Any(v => areaValue.Contains(v)))
        {
            return 100;
        }

        return 1;
    }

    private static void NormalizeToEnum(NormalizationRule rule, IAdAttribute attribute, Type attributeType)
    {
        bool IsMatch(KeyValuePair<string, string> kvp) =>
            new Regex($"(\\s|^){Regex.Escape(kvp.Key)}(\\s|$)", RegexOptions.IgnoreCase)
                .IsMatch(attribute.Value);

        var enumStringValue = rule.EnumMapping!.FirstOrDefault(predicate: IsMatch);
        if (string.IsNullOrEmpty(enumStringValue.Key))
        {
            attribute.TryNormalize(Enum.ToObject(attributeType, default));
        }
        else
        {
            attribute.TryNormalize(Enum.Parse(attributeType, enumStringValue.Value, true));
        }
    }

    private static object? ChangeType(string value, Type attributeType)
    {
        if (attributeType == typeof(double) || attributeType == typeof(double?))
        {
            if (attributeType == typeof(double?) && string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return double.Parse(value.NormalizeDigit(), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        if (attributeType == typeof(int) || attributeType == typeof(int?))
        {
            if (attributeType == typeof(int?) && string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return int.Parse(value.NormalizeDigit(), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        if (attributeType == typeof(string[]))
        {
            return value.Split(' ').ToArray();
        }

        if (attributeType == typeof(bool) || attributeType == typeof(bool?))
        {
            return bool.TryParse(value, out var result) && result;
        }

        if (attributeType.IsEnum)
        {
            return Enum.TryParse(attributeType, value, out object? enumValue) ? enumValue! : 0;
        }

        return Convert.ChangeType(value, attributeType);
    }
}
