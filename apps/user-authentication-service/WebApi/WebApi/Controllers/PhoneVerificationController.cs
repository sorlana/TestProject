using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers;

/// <summary>
/// Контроллер подтверждения телефона
/// Обеспечивает отправку и проверку SMS кодов подтверждения
/// </summary>
[ApiController]
[Route("api/phone")]
[Produces("application/json")]
public class PhoneVerificationController : ControllerBase
{
    private readonly IPhoneVerificationService _phoneVerificationService;
    private readonly ILogger<PhoneVerificationController> _logger;

    public PhoneVerificationController(
        IPhoneVerificationService phoneVerificationService,
        ILogger<PhoneVerificationController> logger)
    {
        _phoneVerificationService = phoneVerificationService;
        _logger = logger;
    }

    /// <summary>
    /// Отправка кода подтверждения на телефон
    /// </summary>
    /// <param name="request">Номер телефона</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат отправки кода</returns>
    /// <response code="200">Код отправлен успешно</response>
    /// <response code="400">Ошибка валидации или превышен лимит отправок</response>
    [HttpPost("send-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendCode(
        [FromBody] SendCodeRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _phoneVerificationService.SendVerificationCodeAsync(
            request.PhoneNumber, 
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Не удалось отправить код на номер {PhoneNumber}", request.PhoneNumber);
            return BadRequest(new { errors = result.Errors, message = result.Message });
        }

        _logger.LogInformation("Код подтверждения отправлен на номер {PhoneNumber}", request.PhoneNumber);
        return Ok(new 
        { 
            message = result.Message,
            expiresAt = result.ExpiresAt
        });
    }

    /// <summary>
    /// Проверка кода подтверждения
    /// </summary>
    /// <param name="request">Номер телефона и код подтверждения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат проверки кода</returns>
    /// <response code="200">Код подтвержден успешно</response>
    /// <response code="400">Неверный или истекший код</response>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyCode(
        [FromBody] VerifyCodeRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _phoneVerificationService.VerifyCodeAsync(
            request.PhoneNumber,
            request.Code,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Неудачная попытка проверки кода для номера {PhoneNumber}", request.PhoneNumber);
            return BadRequest(new { errors = result.Errors, message = result.Message });
        }

        _logger.LogInformation("Телефон {PhoneNumber} успешно подтвержден", request.PhoneNumber);
        return Ok(new 
        { 
            message = result.Message,
            isVerified = result.IsVerified
        });
    }

    /// <summary>
    /// Повторная отправка кода подтверждения
    /// </summary>
    /// <param name="request">Номер телефона</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат отправки кода</returns>
    /// <response code="200">Код отправлен успешно</response>
    /// <response code="400">Ошибка валидации или превышен лимит отправок</response>
    [HttpPost("resend-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendCode(
        [FromBody] SendCodeRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _phoneVerificationService.ResendCodeAsync(
            request.PhoneNumber,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Не удалось повторно отправить код на номер {PhoneNumber}", request.PhoneNumber);
            return BadRequest(new { errors = result.Errors, message = result.Message });
        }

        _logger.LogInformation("Код подтверждения повторно отправлен на номер {PhoneNumber}", request.PhoneNumber);
        return Ok(new 
        { 
            message = result.Message,
            expiresAt = result.ExpiresAt
        });
    }
}

/// <summary>
/// Модель запроса отправки кода с валидацией
/// </summary>
public class SendCodeRequestModel
{
    /// <summary>
    /// Номер телефона
    /// </summary>
    [Required(ErrorMessage = "Номер телефона обязателен")]
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// Модель запроса проверки кода с валидацией
/// </summary>
public class VerifyCodeRequestModel
{
    /// <summary>
    /// Номер телефона
    /// </summary>
    [Required(ErrorMessage = "Номер телефона обязателен")]
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Код подтверждения
    /// </summary>
    [Required(ErrorMessage = "Код подтверждения обязателен")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен содержать 6 цифр")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Код должен содержать только цифры")]
    public string Code { get; set; } = string.Empty;
}
