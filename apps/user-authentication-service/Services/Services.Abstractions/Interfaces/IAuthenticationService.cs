using Services.Contracts.DTOs;
using Services.Contracts.Results;

namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса аутентификации
/// Обеспечивает регистрацию, вход и выход пользователей
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с токенами</returns>
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Вход пользователя по логину и паролю
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с токенами</returns>
    Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Вход пользователя через Google OAuth
    /// </summary>
    /// <param name="request">Данные Google аутентификации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с токенами</returns>
    Task<AuthenticationResult> LoginWithGoogleAsync(GoogleAuthRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление JWT токена через refresh токен
    /// </summary>
    /// <param name="refreshToken">Refresh токен</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат аутентификации с новыми токенами</returns>
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Выход пользователя из системы
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
