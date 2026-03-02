namespace Services.Contracts.Results;

/// <summary>
/// Результат удаления аккаунта
/// </summary>
public class DeleteAccountResult
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
}
