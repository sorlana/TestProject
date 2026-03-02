using Domain.Entities.Entities;

namespace Services.Repositories.Abstractions.Interfaces;

/// <summary>
/// Интерфейс репозитория кодов подтверждения телефона
/// Обеспечивает доступ к данным кодов верификации
/// </summary>
public interface IPhoneVerificationCodeRepository
{
    /// <summary>
    /// Получение активного кода для номера телефона
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Код подтверждения или null</returns>
    Task<PhoneVerificationCode?> GetActiveCodeAsync(string phoneNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Создание нового кода подтверждения
    /// </summary>
    /// <param name="code">Код для создания</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task<PhoneVerificationCode> CreateAsync(PhoneVerificationCode code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление кода подтверждения
    /// </summary>
    /// <param name="code">Код для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task UpdateAsync(PhoneVerificationCode code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Пометка кода как использованного
    /// </summary>
    /// <param name="codeId">Идентификатор кода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task MarkAsUsedAsync(int codeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение количества отправленных кодов за период
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="since">Начало периода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Количество отправленных кодов</returns>
    Task<int> GetSentCountSinceAsync(string phoneNumber, DateTime since, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаление истекших кодов
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task DeleteExpiredCodesAsync(CancellationToken cancellationToken = default);
}
