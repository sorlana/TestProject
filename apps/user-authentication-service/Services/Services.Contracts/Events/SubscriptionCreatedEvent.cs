namespace Services.Contracts.Events;

/// <summary>
/// Событие создания новой подписки
/// </summary>
public class SubscriptionCreatedEvent
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Идентификатор подписки
    /// </summary>
    public int SubscriptionId { get; set; }

    /// <summary>
    /// Идентификатор плана подписки
    /// </summary>
    public int SubscriptionPlanId { get; set; }

    /// <summary>
    /// Название плана
    /// </summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала подписки
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания подписки
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ID заказа в nopCommerce
    /// </summary>
    public string? NopCommerceOrderId { get; set; }
}
