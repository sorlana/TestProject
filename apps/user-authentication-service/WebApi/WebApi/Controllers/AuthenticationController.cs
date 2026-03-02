using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.Interfaces;
using Services.Contracts.DTOs;
using Services.Contracts.Results;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers;

/// <summary>
/// Контроллер аутентификации
/// Обеспечивает регистрацию, вход и выход пользователей
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с токенами</returns>
    /// <response code="200">Регистрация успешна</response>
    /// <response code="400">Ошибка валидации данных</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var registerRequest = new RegisterRequest
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName
        };

        var result = await _authenticationService.RegisterAsync(registerRequest, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Пользователь {UserName} успешно зарегистрирован", request.UserName);
        return Ok(result);
    }

    /// <summary>
    /// Вход в систему по логину и паролю
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с токенами</returns>
    /// <response code="200">Вход выполнен успешно</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="401">Неверные учетные данные</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var loginRequest = new LoginRequest
        {
            UserName = request.UserName,
            Password = request.Password,
            RememberMe = request.RememberMe
        };

        var result = await _authenticationService.LoginAsync(loginRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Неудачная попытка входа для пользователя {UserName}", request.UserName);
            return Unauthorized(new { errors = result.Errors });
        }

        _logger.LogInformation("Пользователь {UserName} успешно вошел в систему", request.UserName);
        return Ok(result);
    }

    /// <summary>
    /// Вход через Google OAuth
    /// </summary>
    /// <param name="request">Google ID токен</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с токенами</returns>
    /// <response code="200">Вход выполнен успешно</response>
    /// <response code="400">Ошибка валидации токена</response>
    /// <response code="401">Неверный Google токен</response>
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginWithGoogle(
        [FromBody] GoogleAuthRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var googleAuthRequest = new GoogleAuthRequest
        {
            IdToken = request.IdToken
        };

        var result = await _authenticationService.LoginWithGoogleAsync(googleAuthRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Неудачная попытка входа через Google");
            return Unauthorized(new { errors = result.Errors });
        }

        _logger.LogInformation("Пользователь успешно вошел через Google");
        return Ok(result);
    }

    /// <summary>
    /// Обновление JWT токена через refresh токен
    /// </summary>
    /// <param name="request">Refresh токен</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с новыми токенами</returns>
    /// <response code="200">Токены обновлены успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Неверный или истекший refresh токен</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequestModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Неудачная попытка обновления токена");
            return Unauthorized(new { errors = result.Errors });
        }

        return Ok(result);
    }

    /// <summary>
    /// Выход из системы
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="200">Выход выполнен успешно</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        // Получаем UserId из claims текущего пользователя
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub");
        
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Пользователь не авторизован" });
        }

        await _authenticationService.LogoutAsync(userId, cancellationToken);

        _logger.LogInformation("Пользователь {UserId} вышел из системы", userId);
        return Ok(new { message = "Выход выполнен успешно" });
    }
}

/// <summary>
/// Модель запроса регистрации с валидацией
/// </summary>
public class RegisterRequestModel
{
    /// <summary>
    /// Логин пользователя
    /// </summary>
    [Required(ErrorMessage = "Логин обязателен")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен содержать от 3 до 50 символов")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email адрес
    /// </summary>
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Номер телефона
    /// </summary>
    [Required(ErrorMessage = "Номер телефона обязателен")]
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Имя (необязательное поле)
    /// </summary>
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия (необязательное поле)
    /// </summary>
    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
    public string? LastName { get; set; }

    /// <summary>
    /// Отчество (необязательное поле)
    /// </summary>
    [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
    public string? MiddleName { get; set; }
}

/// <summary>
/// Модель запроса входа с валидацией
/// </summary>
public class LoginRequestModel
{
    /// <summary>
    /// Логин пользователя
    /// </summary>
    [Required(ErrorMessage = "Логин обязателен")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Флаг "Запомнить меня"
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// Модель запроса Google аутентификации с валидацией
/// </summary>
public class GoogleAuthRequestModel
{
    /// <summary>
    /// Google ID токен
    /// </summary>
    [Required(ErrorMessage = "Google ID токен обязателен")]
    public string IdToken { get; set; } = string.Empty;
}

/// <summary>
/// Модель запроса обновления токена с валидацией
/// </summary>
public class RefreshTokenRequestModel
{
    /// <summary>
    /// Refresh токен
    /// </summary>
    [Required(ErrorMessage = "Refresh токен обязателен")]
    public string RefreshToken { get; set; } = string.Empty;
}
