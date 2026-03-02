using Domain.Entities.Entities;

namespace Services.Repositories.Abstractions.Interfaces;

/// <summary>
/// Интерфейс репозитория подписок пользователей
/// Обеспечивает доступ к данным подписок
/// </summary>
public interface IUserSubscriptionRepository
{
    /// <summary>
    /// Получение подписки по идентификатору
    /// </summary>
    /// <param name="subscriptionId">Идентификатор подписки</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Подписка или null</returns>
    Task<UserSubscription?> GetByIdAsync(int subscriptionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение всех подписок пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список подписок</returns>
    Task<IEnumerable<UserSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение активных подписок пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список активных подписок</returns>
    Task<IEnumerable<UserSubscription>> GetActiveSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение подписки по идентификатору заказа nopCommerce
    /// </summary>
    /// <param name="nopCommerceOrderId">Идентификатор заказа в nopCommerce</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Подписка или null</returns>
    Task<UserSubscription?> GetByNopCommerceOrderIdAsync(string nopCommerceOrderId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Создание новой подписки
    /// </summary>
    /// <param name="subscription">Подписка для создания</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task<UserSubscription> CreateAsync(UserSubscription subscription, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление подписки
    /// </summary>
    /// <param name="subscription">Подписка для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task UpdateAsync(UserSubscription subscription, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверка наличия активной подписки у пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>True если есть активная подписка</returns>
    Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
}
