namespace Services.Contracts.Events;

/// <summary>
/// Событие регистрации нового пользователя
/// </summary>
public class UserRegisteredEvent
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
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время регистрации
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Метод регистрации (Password, Google)
    /// </summary>
    public string RegistrationMethod { get; set; } = string.Empty;
}
