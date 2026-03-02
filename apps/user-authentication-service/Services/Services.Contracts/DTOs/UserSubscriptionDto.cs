namespace Services.Contracts.DTOs;

/// <summary>
/// DTO с информацией о подписке пользователя
/// </summary>
public class UserSubscriptionDto
{
    /// <summary>
    /// Идентификатор подписки
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }
    
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
    /// Флаг активности подписки
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Флаг автоматического продления
    /// </summary>
    public bool AutoRenew { get; set; }
    
    /// <summary>
    /// Дата создания подписки
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата отмены подписки
    /// </summary>
    public DateTime? CancelledAt { get; set; }
}
