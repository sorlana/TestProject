namespace Services.Contracts.DTOs;

/// <summary>
/// DTO с информацией о плане подписки
/// </summary>
public class SubscriptionPlanDto
{
    /// <summary>
    /// Идентификатор плана
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Название плана
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Описание плана
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Цена плана
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public string Currency { get; set; } = "RUB";
    
    /// <summary>
    /// Длительность подписки в днях
    /// </summary>
    public int DurationDays { get; set; }
    
    /// <summary>
    /// Флаг доступности плана
    /// </summary>
    public bool IsAvailable { get; set; }
}
