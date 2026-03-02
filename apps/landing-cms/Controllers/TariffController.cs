using Microsoft.AspNetCore.Mvc;

namespace LandingCms.Controllers;

/// <summary>
/// Контроллер тарифов
/// </summary>
public class TariffController : Controller
{
    private readonly ILogger<TariffController> _logger;

    public TariffController(ILogger<TariffController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Обработка покупки тарифа
    /// Перенаправляет на фронтенд-микросервис для обработки авторизации и оплаты
    /// </summary>
    [HttpGet("tariff/purchase/{tariffId}")]
    public IActionResult Purchase([FromRoute] string tariffId)
    {
        _logger.LogInformation("Редирект на страницу оплаты для тарифа {TariffId}", tariffId);

        // Простой редирект на фронтенд-микросервис
        // Фронтенд сам решит: показать авторизацию или страницу оплаты
        return Redirect($"/app/payment?tariffId={tariffId}");
    }
}
