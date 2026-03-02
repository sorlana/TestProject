namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса отправки SMS
/// Обеспечивает отправку SMS сообщений через внешнего провайдера
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Отправка SMS сообщения
    /// </summary>
    /// <param name="phoneNumber">Номер телефона получателя</param>
    /// <param name="message">Текст сообщения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>True если сообщение отправлено успешно, иначе False</returns>
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
