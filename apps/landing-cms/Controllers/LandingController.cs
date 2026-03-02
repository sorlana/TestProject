using Microsoft.AspNetCore.Mvc;
using Piranha;

namespace LandingCms.Controllers;

/// <summary>
/// Контроллер главной страницы лендинга
/// </summary>
public class LandingController : Controller
{
    private readonly IApi _api;
    private readonly ILogger<LandingController> _logger;

    public LandingController(IApi api, ILogger<LandingController> logger)
    {
        _api = api;
        _logger = logger;
    }

    /// <summary>
    /// Отображение главной страницы
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Получение данных страницы из Piranha CMS
            var page = await _api.Pages.GetBySlugAsync<Models.LandingPage>("/");

            if (page == null)
            {
                _logger.LogWarning("Главная страница не найдена в CMS");
                return NotFound();
            }

            return View(page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке страницы из CMS");
            return StatusCode(500, "Ошибка загрузки страницы");
        }
    }
}
