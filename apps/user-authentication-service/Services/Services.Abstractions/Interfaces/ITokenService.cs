using Domain.Entities.Entities;

namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса работы с токенами
/// Обеспечивает генерацию, валидацию и отзыв JWT и refresh токенов
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Генерация JWT токена для пользователя
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <param name="roles">Роли пользователя</param>
    /// <returns>JWT токен</returns>
    string GenerateJwtToken(User user, IList<string> roles);
    
    /// <summary>
    /// Генерация refresh токена
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="rememberMe">Флаг "Запомнить меня" для увеличения времени жизни</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Refresh токен</returns>
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, bool rememberMe, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Валидация refresh токена
    /// </summary>
    /// <param name="token">Значение токена</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>True если токен валиден, иначе False</returns>
    Task<bool> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отзыв refresh токена
    /// </summary>
    /// <param name="token">Значение токена</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отзыв всех refresh токенов пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
