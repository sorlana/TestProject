namespace Services.Contracts.Events;

/// <summary>
/// Событие успешного входа пользователя в систему
/// </summary>
public class UserLoggedInEvent
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
    /// Дата и время входа
    /// </summary>
    public DateTime LoggedInAt { get; set; }

    /// <summary>
    /// IP адрес
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Метод входа (Password, Google)
    /// </summary>
    public string LoginMethod { get; set; } = string.Empty;
}
