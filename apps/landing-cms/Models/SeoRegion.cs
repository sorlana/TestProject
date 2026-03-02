using Piranha.Extend;
using Piranha.Extend.Fields;

namespace LandingCms.Models;

/// <summary>
/// Регион для SEO данных
/// </summary>
public class SeoRegion
{
    /// <summary>
    /// Meta Title
    /// </summary>
    [Field(Title = "Meta Title", Description = "Заголовок страницы для поисковых систем")]
    public StringField MetaTitle { get; set; } = new();

    /// <summary>
    /// Meta Description
    /// </summary>
    [Field(Title = "Meta Description", Description = "Описание страницы для поисковых систем")]
    public TextField MetaDescription { get; set; } = new();

    /// <summary>
    /// Meta Keywords
    /// </summary>
    [Field(Title = "Meta Keywords", Description = "Ключевые слова через запятую")]
    public StringField MetaKeywords { get; set; } = new();

    /// <summary>
    /// OG Title
    /// </summary>
    [Field(Title = "OG Title", Description = "Заголовок для социальных сетей")]
    public StringField OgTitle { get; set; } = new();

    /// <summary>
    /// OG Description
    /// </summary>
    [Field(Title = "OG Description", Description = "Описание для социальных сетей")]
    public TextField OgDescription { get; set; } = new();

    /// <summary>
    /// OG Image
    /// </summary>
    [Field(Title = "OG Image", Description = "Изображение для социальных сетей")]
    public ImageField OgImage { get; set; } = new();
}
