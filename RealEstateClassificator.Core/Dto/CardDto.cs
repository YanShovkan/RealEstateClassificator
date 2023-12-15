using RealEstateClassificator.Common.Enums;

namespace RealEstateClassificator.Core.Dto;
public record CardDto
{
    /// <summary>
    /// Ссылка на URL объявления.
    /// </summary>
    public string Url { get; init; } = null!;

    /// <summary>
    /// Стоимость.
    /// </summary>
    public string Price { get; set; } = null!;

    /// <summary>
    /// Описание объявления.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Город.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Район.
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// Адрес.
    /// </summary>
    public string Address { get; set; } = null!;

    /// <summary>
    /// Этаж.
    /// </summary>
    public string? Floor { get; set; }

    /// <summary>
    /// Кол-во этажей.
    /// </summary>
    public string? Floors { get; set; }

    /// <summary>
    /// Комнат.
    /// </summary>
    public string? Rooms { get; set; }

    /// <summary>
    /// Площадь квартиры.
    /// </summary>
    public string? TotalArea { get; set; }

    /// <summary>
    /// Жилая площадь.
    /// </summary>
    public string? LivingArea { get; set; }

    /// <summary>
    /// Площадь кухни.
    /// </summary>
    public string? KitchenArea { get; set; }

    /// <summary>
    /// Ремонт.
    /// </summary>
    public string? Renovation { get; set; }

    /// <summary>
    /// Количество совмещенных санузлов.
    /// </summary>
    public string? CombinedBathrooms { get; set; }

    /// <summary>
    /// Количество раздельных санузлов.
    /// </summary>
    public string? SeparateBathrooms { get; set; }

    /// <summary>
    /// Количество балконов.
    /// </summary>
    public string? BalconiesCount { get; set; }
            
    /// <summary>
    /// Расстояние от города.
    /// </summary>
    public string? DistanceToCity { get; set; }

    /// <summary>
    /// Год постройки.
    /// </summary>
    public string? BuiltYear { get; set; }

    /// <summary>
    /// Лифты пассажирские количество.
    /// </summary>
    public string? PassengerLiftsCount { get; set; }

    /// <summary>
    /// Лифты грузовые количество.
    /// </summary>
    public string? CargoLiftsCount { get; set; }

    /// <summary>
    /// Квартира является студией.
    /// </summary>
    public string? IsStudio { get; set; }

    /// <summary>
    /// Свойство для фильтрации.
    /// </summary>
    public string? FilterProperty { get; set; }
}
