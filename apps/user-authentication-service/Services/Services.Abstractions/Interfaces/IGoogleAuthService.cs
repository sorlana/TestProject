using Domain.Entities.Entities;
using Services.Contracts.DTOs;

namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса аутентификации через Google OAuth
/// Обеспечивает валидацию Google токенов и создание/связывание пользователей
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Валидация Google ID токена и получение информации о пользователе
    /// </summary>
    /// <param name="idToken">Google ID токен</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о пользователе из Google</returns>
    Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение существующего пользователя или создание нового из данных Google
    /// </summary>
    /// <param name="googleInfo">Информация о пользователе из Google</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Пользователь системы</returns>
    Task<User> GetOrCreateUserFromGoogleAsync(GoogleUserInfo googleInfo, CancellationToken cancellationToken = default);
}
