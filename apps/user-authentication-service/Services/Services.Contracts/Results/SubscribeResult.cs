using Services.Contracts.DTOs;

namespace Services.Contracts.Results;

/// <summary>
/// Результат оформления подписки
/// </summary>
public class SubscribeResult
{
    /// <summary>
    /// Флаг успешности операции
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Информация о созданной подписке
    /// </summary>
    public UserSubscriptionDto? Subscription { get; set; }
    
    /// <summary>
    /// Список ошибок
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }
    
    /// <summary>
    /// Сообщение о результате
    /// </summary>
    public string? Message { get; set; }
}
