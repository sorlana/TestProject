using Piranha.Extend;
using Piranha.Extend.Fields;

namespace LandingCms.Models;

/// <summary>
/// Регион секции "Тарифы"
/// </summary>
public class TariffsRegion
{
    /// <summary>
    /// Заголовок
    /// </summary>
    [Field(Title = "Заголовок", Description = "Заголовок секции тарифов")]
    public StringField Title { get; set; } = new();

    /// <summary>
    /// ID тарифа "Общий"
    /// </summary>
    [Field(Title = "ID тарифа", Description = "Уникальный идентификатор тарифа")]
    public StringField GeneralTariffId { get; set; } = new();

    /// <summary>
    /// Название тарифа "Общий"
    /// </summary>
    [Field(Title = "Название тарифа", Description = "Название тарифа")]
    public StringField GeneralTariffName { get; set; } = new();

    /// <summary>
    /// Описание тарифа "Общий"
    /// </summary>
    [Field(Title = "Описание тарифа", Description = "Краткое описание тарифа")]
    public TextField GeneralTariffDescription { get; set; } = new();

    /// <summary>
    /// Цена тарифа "Общий"
    /// </summary>
    [Field(Title = "Цена тарифа", Description = "Стоимость тарифа в рублях")]
    public NumberField GeneralTariffPrice { get; set; } = new();

    /// <summary>
    /// Особенности тарифа "Общий"
    /// </summary>
    [Field(Title = "Особенности тарифа", Description = "Список особенностей тарифа (по одной на строку)")]
    public TextField GeneralTariffFeatures { get; set; } = new();
}
