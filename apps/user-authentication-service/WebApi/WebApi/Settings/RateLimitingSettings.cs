namespace WebApi.Settings;

/// <summary>
/// Настройки для защиты от брутфорса и rate limiting
/// </summary>
public class RateLimitingSettings
{
    /// <summary>
    /// Максимальное количество неудачных попыток входа на пользователя
    /// </summary>
    public int FailedLoginAttemptsPerUser { get; set; } = 5;

    /// <summary>
    /// Временное окно для подсчета неудачных попыток входа (в минутах)
    /// </summary>
    public int FailedLoginWindowMinutes { get; set; } = 15;

    /// <summary>
    /// Время блокировки пользователя после превышения лимита (в минутах)
    /// </summary>
    public int UserLockoutMinutes { get; set; } = 15;

    /// <summary>
    /// Максимальное количество неудачных попыток входа с одного IP адреса
    /// </summary>
    public int FailedLoginAttemptsPerIp { get; set; } = 20;

    /// <summary>
    /// Время блокировки IP адреса после превышения лимита (в часах)
    /// </summary>
    public int IpLockoutHours { get; set; } = 1;

    /// <summary>
    /// Максимальное количество отправок SMS кодов в час на один номер
    /// </summary>
    public int PhoneVerificationMaxAttemptsPerHour { get; set; } = 3;
}
