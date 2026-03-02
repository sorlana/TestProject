using Microsoft.AspNetCore.Mvc;
using Piranha;
using LandingCms.Models;

namespace LandingCms.Controllers;

/// <summary>
/// Контроллер для обработки страниц Piranha CMS
/// </summary>
public class CmsController : Controller
{
    private readonly IApi _api;
    private readonly ILogger<CmsController> _logger;

    public CmsController(IApi api, ILogger<CmsController> logger)
    {
        _api = api;
        _logger = logger;
    }

    /// <summary>
    /// Обработка страниц CMS
    /// </summary>
    public async Task<IActionResult> Page(string? slug = null)
    {
        try
        {
            // Получаем slug из пути
            var path = string.IsNullOrEmpty(slug) ? "/" : $"/{slug}";
            
            // Загружаем страницу через Piranha API
            var page = await _api.Pages.GetBySlugAsync<LandingPage>(path);

            if (page == null)
            {
                _logger.LogWarning("Страница не найдена: {Path}", path);
                return NotFound();
            }

            // Возвращаем представление
            return View("~/Views/Landing/Index.cshtml", page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке страницы: {Slug}", slug);
            return StatusCode(500, "Ошибка загрузки страницы");
        }
    }
}
