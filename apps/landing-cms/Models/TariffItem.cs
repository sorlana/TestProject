using Piranha.Extend;
using Piranha.Extend.Fields;

namespace LandingCms.Models;

/// <summary>
/// Элемент тарифа
/// </summary>
public class TariffItem
{
    /// <summary>
    /// ID тарифа
    /// </summary>
    [Field(Title = "ID тарифа", Description = "Уникальный идентификатор тарифа")]
    public StringField TariffId { get; set; } = new();

    /// <summary>
    /// Название
    /// </summary>
    [Field(Title = "Название", Description = "Название тарифа")]
    public StringField Name { get; set; } = new();

    /// <summary>
    /// Описание
    /// </summary>
    [Field(Title = "Описание", Description = "Краткое описание тарифа")]
    public TextField Description { get; set; } = new();

    /// <summary>
    /// Цена
    /// </summary>
    [Field(Title = "Цена", Description = "Стоимость тарифа в рублях")]
    public NumberField Price { get; set; } = new();

    /// <summary>
    /// Особенности
    /// </summary>
    [Field(Title = "Особенности", Description = "Список особенностей тарифа (по одной на строку)")]
    public TextField Features { get; set; } = new();
}
