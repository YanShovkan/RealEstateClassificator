using RealEstateClassificator.Common.Enums;

namespace RealEstateClassificator.Core.Settings;
public record JsonMapping(string ItemListSelector, string AdSelector, Dictionary<string, string> MembersMap);