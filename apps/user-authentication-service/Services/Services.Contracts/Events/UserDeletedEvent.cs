namespace Services.Contracts.Events;

/// <summary>
/// Событие удаления аккаунта пользователя
/// </summary>
public class UserDeletedEvent
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время удаления
    /// </summary>
    public DateTime DeletedAt { get; set; }

    /// <summary>
    /// Причина удаления
    /// </summary>
    public string? Reason { get; set; }
}
