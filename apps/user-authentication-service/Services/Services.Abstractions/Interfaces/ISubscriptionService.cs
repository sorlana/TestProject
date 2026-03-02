using Services.Contracts.DTOs;
using Services.Contracts.Results;

namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс сервиса управления подписками
/// Обеспечивает работу с подписками пользователей через интеграцию с nopCommerce
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Получение подписок пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список подписок пользователя</returns>
    Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение доступных планов подписок
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список доступных планов</returns>
    Task<IEnumerable<SubscriptionPlanDto>> GetAvailablePlansAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Оформление подписки
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="planId">Идентификатор плана подписки</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат оформления подписки</returns>
    Task<SubscribeResult> SubscribeAsync(Guid userId, int planId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отмена подписки
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="subscriptionId">Идентификатор подписки</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат отмены подписки</returns>
    Task<CancelSubscriptionResult> CancelSubscriptionAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверка наличия активной подписки у пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>True если есть активная подписка, иначе False</returns>
    Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
}
