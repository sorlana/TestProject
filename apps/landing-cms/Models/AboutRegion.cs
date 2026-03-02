using Piranha.Extend;
using Piranha.Extend.Fields;

namespace LandingCms.Models;

/// <summary>
/// Регион секции "О программе"
/// </summary>
public class AboutRegion
{
    /// <summary>
    /// Заголовок
    /// </summary>
    [Field(Title = "Заголовок", Description = "Заголовок секции")]
    public StringField Title { get; set; } = new();

    /// <summary>
    /// Описание
    /// </summary>
    [Field(Title = "Описание", Description = "HTML контент с описанием программы")]
    public HtmlField Description { get; set; } = new();

    /// <summary>
    /// Изображение
    /// </summary>
    [Field(Title = "Изображение", Description = "Изображение для секции")]
    public ImageField Image { get; set; } = new();
}
