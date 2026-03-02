using Services.Contracts.Results;

namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса подтверждения телефона
/// Обеспечивает отправку и проверку SMS кодов подтверждения
/// </summary>
public interface IPhoneVerificationService
{
    /// <summary>
    /// Отправка кода подтверждения на телефон
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат отправки кода</returns>
    Task<SendCodeResult> SendVerificationCodeAsync(string phoneNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверка кода подтверждения
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="code">Код подтверждения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат проверки кода</returns>
    Task<VerificationResult> VerifyCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Повторная отправка кода подтверждения
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат отправки кода</returns>
    Task<SendCodeResult> ResendCodeAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
