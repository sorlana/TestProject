using Domain.Entities.Entities;

namespace Services.Repositories.Abstractions.Interfaces;

/// <summary>
/// Интерфейс репозитория refresh токенов
/// Обеспечивает доступ к данным токенов обновления
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Получение refresh токена по значению
    /// </summary>
    /// <param name="token">Значение токена</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Refresh токен или null</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение всех активных токенов пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список активных токенов</returns>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Создание нового refresh токена
    /// </summary>
    /// <param name="refreshToken">Токен для создания</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление refresh токена
    /// </summary>
    /// <param name="refreshToken">Токен для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отзыв refresh токена
    /// </summary>
    /// <param name="token">Значение токена</param>
    /// <param name="revokedByIp">IP адрес, с которого отозван токен</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task RevokeAsync(string token, string? revokedByIp, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отзыв всех активных токенов пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="revokedByIp">IP адрес, с которого отозваны токены</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task RevokeAllUserTokensAsync(Guid userId, string? revokedByIp, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаление истекших токенов
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
