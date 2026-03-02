namespace Domain.Entities.Entities;

/// <summary>
/// Доменная сущность подписки пользователя
/// Хранит информацию о подписках на платные функции системы
/// </summary>
public class UserSubscription
{
    /// <summary>
    /// Уникальный идентификатор подписки
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя, которому принадлежит подписка
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Идентификатор плана подписки
    /// </summary>
    public int SubscriptionPlanId { get; set; }
    
    /// <summary>
    /// Название плана подписки
    /// </summary>
    public string PlanName { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата начала действия подписки
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Дата окончания действия подписки
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Флаг активности подписки
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Флаг автоматического продления подписки
    /// </summary>
    public bool AutoRenew { get; set; }
    
    /// <summary>
    /// Дата и время создания подписки
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата и время отмены подписки
    /// </summary>
    public DateTime? CancelledAt { get; set; }
    
    /// <summary>
    /// Идентификатор заказа в nopCommerce для синхронизации
    /// </summary>
    public string? NopCommerceOrderId { get; set; }
    
    /// <summary>
    /// Навигационное свойство к пользователю
    /// </summary>
    public User User { get; set; } = null!;
}
