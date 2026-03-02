using Domain.Entities.Entities;
using Domain.Entities.Enums;

namespace Services.Repositories.Abstractions.Interfaces;

/// <summary>
/// Интерфейс репозитория логов аутентификации
/// Обеспечивает доступ к данным логов операций аутентификации
/// </summary>
public interface IAuthenticationLogRepository
{
    /// <summary>
    /// Создание новой записи лога
    /// </summary>
    /// <param name="log">Лог для создания</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task CreateAsync(AuthenticationLog log, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение логов пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="limit">Максимальное количество записей</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список логов</returns>
    Task<IEnumerable<AuthenticationLog>> GetByUserIdAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение логов по IP адресу
    /// </summary>
    /// <param name="ipAddress">IP адрес</param>
    /// <param name="since">Начало периода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список логов</returns>
    Task<IEnumerable<AuthenticationLog>> GetByIpAddressAsync(string ipAddress, DateTime since, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение количества неудачных попыток входа пользователя за период
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="since">Начало периода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Количество неудачных попыток</returns>
    Task<int> GetFailedLoginCountAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение количества неудачных попыток входа с IP адреса за период
    /// </summary>
    /// <param name="ipAddress">IP адрес</param>
    /// <param name="since">Начало периода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Количество неудачных попыток</returns>
    Task<int> GetFailedLoginCountByIpAsync(string ipAddress, DateTime since, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение логов по типу события
    /// </summary>
    /// <param name="eventType">Тип события</param>
    /// <param name="since">Начало периода</param>
    /// <param name="limit">Максимальное количество записей</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список логов</returns>
    Task<IEnumerable<AuthenticationLog>> GetByEventTypeAsync(AuthenticationEventType eventType, DateTime since, int limit = 100, CancellationToken cancellationToken = default);
}
