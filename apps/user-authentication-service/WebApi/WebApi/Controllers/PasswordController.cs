using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.Interfaces;

namespace WebApi.Controllers;

/// <summary>
/// Контроллер работы с паролями
/// Обеспечивает генерацию надежных паролей
/// </summary>
[ApiController]
[Route("api/password")]
[Produces("application/json")]
public class PasswordController : ControllerBase
{
    private readonly IPasswordService _passwordService;
    private readonly ILogger<PasswordController> _logger;

    public PasswordController(
        IPasswordService passwordService,
        ILogger<PasswordController> logger)
    {
        _passwordService = passwordService;
        _logger = logger;
    }

    /// <summary>
    /// Генерация надежного пароля
    /// </summary>
    /// <returns>Сгенерированный пароль</returns>
    /// <response code="200">Пароль сгенерирован успешно</response>
    [HttpGet("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GeneratePassword()
    {
        var password = _passwordService.GenerateSecurePassword();
        
        _logger.LogInformation("Сгенерирован надежный пароль");
        
        return Ok(new 
        { 
            password = password,
            message = "Надежный пароль успешно сгенерирован"
        });
    }
}
