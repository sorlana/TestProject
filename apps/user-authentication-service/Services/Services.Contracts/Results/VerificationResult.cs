namespace Services.Contracts.Results;

/// <summary>
/// Результат проверки кода подтверждения
/// </summary>
public class VerificationResult
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
    /// Флаг подтверждения телефона
    /// </summary>
    public bool IsVerified { get; set; }
}
