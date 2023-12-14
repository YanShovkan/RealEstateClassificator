namespace RealEstateClassificator.Common.Enums;

/// <summary>
/// Тип ремонта.
/// </summary>
public enum RenovationType
{
    Undefined = 0,

    /// <summary>
    /// косметический.
    /// </summary>
    Cosmetic = 1,

    /// <summary>
    /// дизайнерский.
    /// </summary>
    Design = 2,

    /// <summary>
    /// евроремонт.
    /// </summary>
    Euro = 3,

    /// <summary>
    /// чистовая.
    /// </summary>
    Finishing = 4,

    /// <summary>
    /// Без отделки.
    /// </summary>
    None = 5,

    /// <summary>
    /// Офисная отделка.
    /// </summary>
    Office = 6,

    /// <summary>
    /// требуется ремонт.
    /// </summary>
    Required = 7,

    /// <summary>
    /// черновая.
    /// </summary>
    Rough = 8,

    /// <summary>
    /// обычный.
    /// </summary>
    Standard = 9,
}