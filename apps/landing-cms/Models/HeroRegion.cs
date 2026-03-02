using Piranha.Extend;
using Piranha.Extend.Fields;

namespace LandingCms.Models;

/// <summary>
/// Регион Hero секции
/// </summary>
public class HeroRegion
{
    /// <summary>
    /// Заголовок
    /// </summary>
    [Field(Title = "Заголовок", Description = "Главный заголовок Hero секции")]
    public StringField Title { get; set; } = new();

    /// <summary>
    /// Описание
    /// </summary>
    [Field(Title = "Описание", Description = "Текст описания под заголовком")]
    public TextField Description { get; set; } = new();

    /// <summary>
    /// Фоновое изображение
    /// </summary>
    [Field(Title = "Фоновое изображение", Description = "Изображение для фона Hero секции")]
    public ImageField BackgroundImage { get; set; } = new();

    /// <summary>
    /// Текст кнопки
    /// </summary>
    [Field(Title = "Текст кнопки", Description = "Текст на кнопке призыва к действию")]
    public StringField ButtonText { get; set; } = new();
}
