using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.Interfaces;
using Services.Contracts.DTOs;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers;

/// <summary>
/// Контроллер управления профилем пользователя
/// Обеспечивает просмотр, обновление и удаление профиля
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        IUserProfileService userProfileService,
        ILogger<UserProfileController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    /// <summary>
    /// Получение профиля текущего пользователя
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Данные профиля пользователя</returns>
    /// <response code="200">Профиль получен успешно</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Профиль не найден</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        var profile = await _userProfileService.GetProfileAsync(userId.Value, cancellationToken);

        if (profile == null)
        {
            return NotFound(new { message = "Профиль не найден" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Обновление профиля текущего пользователя
    /// </summary>
    /// <param name="request">Данные для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат обновления профиля</returns>
    /// <response code="200">Профиль обновлен успешно</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        var updateRequest = new UpdateProfileRequest
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName
        };

        var result = await _userProfileService.UpdateProfileAsync(userId.Value, updateRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Не удалось обновить профиль пользователя {UserId}", userId.Value);
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Профиль пользователя {UserId} успешно обновлен", userId.Value);
        return Ok(new
        {
            message = "Профиль успешно обновлен",
            profile = result.Profile,
            requiresEmailVerification = result.RequiresEmailVerification,
            requiresPhoneVerification = result.RequiresPhoneVerification
        });
    }

    /// <summary>
    /// Смена пароля текущего пользователя
    /// </summary>
    /// <param name="request">Данные для смены пароля</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат смены пароля</returns>
    /// <response code="200">Пароль изменен успешно</response>
    /// <response code="400">Ошибка валидации или неверный текущий пароль</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
            ConfirmNewPassword = request.ConfirmNewPassword
        };

        var result = await _userProfileService.ChangePasswordAsync(userId.Value, changePasswordRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Не удалось изменить пароль пользователя {UserId}", userId.Value);
            return BadRequest(new { errors = result.Errors, message = result.Message });
        }

        _logger.LogInformation("Пароль пользователя {UserId} успешно изменен", userId.Value);
        return Ok(new { message = result.Message ?? "Пароль успешно изменен" });
    }

    /// <summary>
    /// Удаление аккаунта текущего пользователя (мягкое удаление)
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат удаления аккаунта</returns>
    /// <response code="200">Аккаунт удален успешно</response>
    /// <response code="400">Ошибка при удалении</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        var result = await _userProfileService.DeleteAccountAsync(userId.Value, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Не удалось удалить аккаунт пользователя {UserId}", userId.Value);
            return BadRequest(new { errors = result.Errors, message = result.Message });
        }

        _logger.LogInformation("Аккаунт пользователя {UserId} успешно удален", userId.Value);
        return Ok(new { message = result.Message ?? "Аккаунт успешно удален" });
    }

    /// <summary>
    /// Получение идентификатора текущего пользователя из claims
    /// </summary>
    /// <returns>Идентификатор пользователя или null</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}

/// <summary>
/// Модель запроса обновления профиля с валидацией
/// </summary>
public class UpdateProfileRequestModel
{
    /// <summary>
    /// Email адрес
    /// </summary>
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string? Email { get; set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
    public string? LastName { get; set; }

    /// <summary>
    /// Отчество пользователя
    /// </summary>
    [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
    public string? MiddleName { get; set; }
}

/// <summary>
/// Модель запроса смены пароля с валидацией
/// </summary>
public class ChangePasswordRequestModel
{
    /// <summary>
    /// Текущий пароль
    /// </summary>
    [Required(ErrorMessage = "Текущий пароль обязателен")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    [Required(ErrorMessage = "Новый пароль обязателен")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение нового пароля
    /// </summary>
    [Required(ErrorMessage = "Подтверждение нового пароля обязательно")]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
