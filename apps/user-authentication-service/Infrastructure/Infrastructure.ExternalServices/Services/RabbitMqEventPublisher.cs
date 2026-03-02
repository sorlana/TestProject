using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Services.Abstractions.Interfaces;
using Infrastructure.ExternalServices.Configuration;

namespace Infrastructure.ExternalServices.Services;

/// <summary>
/// Реализация publisher'а событий в RabbitMQ
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqEventPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        InitializeConnection();
    }

    /// <summary>
    /// Инициализирует подключение к RabbitMQ
    /// </summary>
    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(_settings.ConnectionTimeout),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Объявляем exchange
            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: _settings.ExchangeType,
                durable: true,
                autoDelete: false);

            _logger.LogInformation(
                "Успешно подключено к RabbitMQ: {Host}:{Port}, Exchange: {Exchange}",
                _settings.Host,
                _settings.Port,
                _settings.ExchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подключении к RabbitMQ");
            throw;
        }
    }

    /// <summary>
    /// Публикует событие в RabbitMQ
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : class
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMqEventPublisher));
        }

        var eventType = typeof(TEvent).Name;
        var routingKey = GetRoutingKey(eventType);

        try
        {
            // Сериализуем событие в JSON
            var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var body = Encoding.UTF8.GetBytes(message);

            // Проверяем подключение
            EnsureConnection();

            lock (_lock)
            {
                var properties = _channel!.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.Type = eventType;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                // Публикуем сообщение
                _channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogInformation(
                "Событие {EventType} успешно опубликовано с routing key: {RoutingKey}",
                eventType,
                routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка при публикации события {EventType} с routing key: {RoutingKey}",
                eventType,
                routingKey);
            throw;
        }
    }

    /// <summary>
    /// Получает routing key для типа события
    /// </summary>
    private static string GetRoutingKey(string eventType)
    {
        // Преобразуем имя типа в routing key формата: user.registered, user.loggedin и т.д.
        return eventType switch
        {
            "UserRegisteredEvent" => "user.registered",
            "UserLoggedInEvent" => "user.loggedin",
            "UserDeletedEvent" => "user.deleted",
            "SubscriptionCreatedEvent" => "subscription.created",
            _ => eventType.ToLowerInvariant()
        };
    }

    /// <summary>
    /// Проверяет и восстанавливает подключение при необходимости
    /// </summary>
    private void EnsureConnection()
    {
        if (_channel != null && _channel.IsOpen && _connection != null && _connection.IsOpen)
        {
            return;
        }

        _logger.LogWarning("Подключение к RabbitMQ потеряно, переподключение...");

        lock (_lock)
        {
            // Закрываем старые подключения
            try
            {
                _channel?.Close();
                _channel?.Dispose();
            }
            catch { }

            try
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            catch { }

            // Переподключаемся
            InitializeConnection();
        }
    }

    /// <summary>
    /// Освобождает ресурсы
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("Подключение к RabbitMQ закрыто");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при закрытии подключения к RabbitMQ");
        }
        finally
        {
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
