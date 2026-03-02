namespace Services.Contracts.Results;

/// <summary>
/// Результат отправки кода подтверждения
/// </summary>
public class SendCodeResult
{
    /// <summary>
    /// Флаг успешности операции
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Сообщение о результате
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Список ошибок
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }
    
    /// <summary>
    /// Время истечения кода
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
