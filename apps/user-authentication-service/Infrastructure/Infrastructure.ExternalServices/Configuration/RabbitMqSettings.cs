namespace Infrastructure.ExternalServices.Configuration;

/// <summary>
/// Настройки подключения к RabbitMQ
/// </summary>
public class RabbitMqSettings
{
    /// <summary>
    /// Имя секции конфигурации
    /// </summary>
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// Хост RabbitMQ
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Порт RabbitMQ
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Виртуальный хост
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Имя exchange для событий
    /// </summary>
    public string ExchangeName { get; set; } = "user-authentication-events";

    /// <summary>
    /// Тип exchange (direct, topic, fanout, headers)
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Таймаут подключения в секундах
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Количество попыток переподключения
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
