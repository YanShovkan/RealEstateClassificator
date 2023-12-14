using RealEstateClassificator.Common.Enums;

namespace RealEstateClassificator.Dal.Entities;

/// <summary>
/// Карточка объявления.
/// </summary>
public class Card
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Ссылка на URL объявления.
    /// </summary>
    public string Url { get; init; } = null!;

    /// <summary>
    /// Медиа файлы.
    /// </summary>
    public List<string> MediaFiles { get; } = new();

    /// <summary>
    /// Стоимость.
    /// </summary>
    public long Price { get; set; } 

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
    public string Address { get; set; } = new(string.Empty);

    /// <summary>
    /// Этаж.
    /// </summary>
    public int? Floor { get; set; }

    /// <summary>
    /// Кол-во этажей.
    /// </summary>
    public int? Floors { get; set; }

    /// <summary>
    /// Комнат.
    /// </summary>
    public int? Rooms { get; set; }

    /// <summary>
    /// Площадь квартиры.
    /// </summary>
    public double? TotalArea { get; set; }

    /// <summary>
    /// Жилая площадь.
    /// </summary>
    public double? LivingArea { get; set; }

    /// <summary>
    /// Площадь кухни.
    /// </summary>
    public double? KitchenArea { get; set; }

    /// <summary>
    /// Высота потолков.
    /// </summary>
    public double? CeilingHeight { get; set; }

    /// <summary>
    /// Ремонт.
    /// </summary>
    public RenovationType? Renovation { get; set; }

    /// <summary>
    /// Количество совмещенных санузлов.
    /// </summary>
    public int? CombinedBathrooms { get; set; }

    /// <summary>
    /// Количество раздельных санузлов.
    /// </summary>
    public int? SeparateBathrooms { get; set; }

    /// <summary>
    /// Количество балконов.
    /// </summary>
    public int? BalconiesCount { get; set; }

    /// <summary>
    /// Количество лоджий.
    /// </summary>
    public int? LoggiasCount { get; set; }

    /// <summary>
    /// Вид из окон.
    /// </summary>
    public WindowViewType? WindowsView { get; set; }

    /// <summary>
    /// Охрана.
    /// </summary>
    public bool? Security { get; set; }

    /// <summary>
    /// Расстояние от города.
    /// </summary>
    public double? DistanceToCity { get; set; }

    /// <summary>
    /// Материал стен.
    /// </summary>
    public WallMaterialType? ConstructionType { get; set; }

    /// <summary>
    /// Год постройки.
    /// </summary>
    public int? BuiltYear { get; set; }

    /// <summary>
    /// Лифты пассажирские количество.
    /// </summary>
    public int? PassengerLiftsCount { get; set; }

    /// <summary>
    /// Лифты грузовые количество.
    /// </summary>
    public int? CargoLiftsCount { get; set; }

    /// <summary>
    /// Квартира является студией.
    /// </summary>
    public bool? IsStudio { get; set; }

    /// <summary>
    /// Свойство для фильтрации.
    /// </summary>
    public string? FilterProperty { get; set; }

    /// <summary>
    /// Класс объявления.
    /// </summary>
    public int? ClassOfCard { get; set; }
}
