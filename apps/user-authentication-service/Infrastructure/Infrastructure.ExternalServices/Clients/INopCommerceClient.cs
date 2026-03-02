using Services.Contracts.DTOs;

namespace Infrastructure.ExternalServices.Clients;

/// <summary>
/// Интерфейс клиента для интеграции с внешним nopCommerce API
/// </summary>
public interface INopCommerceClient
{
    /// <summary>
    /// Получение доступных планов подписок
    /// </summary>
    Task<IEnumerable<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение подписок пользователя
    /// </summary>
    Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Создание новой подписки
    /// </summary>
    Task<CreateSubscriptionResponse> CreateSubscriptionAsync(Guid userId, int planId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление статуса подписки
    /// </summary>
    Task<bool> UpdateSubscriptionStatusAsync(int subscriptionId, string status, CancellationToken cancellationToken = default);
}

/// <summary>
/// Ответ на создание подписки от nopCommerce
/// </summary>
public class CreateSubscriptionResponse
{
    /// <summary>
    /// Идентификатор заказа в nopCommerce
    /// </summary>
    public string OrderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Идентификатор подписки
    /// </summary>
    public int SubscriptionId { get; set; }
    
    /// <summary>
    /// Дата начала подписки
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Дата окончания подписки
    /// </summary>
    public DateTime EndDate { get; set; }
}
