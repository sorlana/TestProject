using Piranha.AttributeBuilder;
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace LandingCms.Models;

/// <summary>
/// Модель главной страницы лендинга
/// </summary>
[PageType(Title = "Landing Page", UseBlocks = false)]
[ContentTypeRoute(Title = "Default", Route = "/")]
public class LandingPage : Page<LandingPage>
{
    /// <summary>
    /// Hero секция
    /// </summary>
    [Region(Title = "Hero Section", Description = "Главная секция с заголовком и описанием")]
    public HeroRegion Hero { get; set; } = new();

    /// <summary>
    /// Секция "О программе"
    /// </summary>
    [Region(Title = "About Section", Description = "Описание возможностей платформы")]
    public AboutRegion About { get; set; } = new();

    /// <summary>
    /// Секция "Тарифы"
    /// </summary>
    [Region(Title = "Tariffs Section", Description = "Доступные тарифы")]
    public TariffsRegion Tariffs { get; set; } = new();

    /// <summary>
    /// SEO данные
    /// </summary>
    [Region(Title = "SEO", Description = "Мета-теги и Open Graph")]
    public SeoRegion Seo { get; set; } = new();
}
