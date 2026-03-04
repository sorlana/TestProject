using Microsoft.Extensions.Logging;
using Services.Abstractions.Interfaces;

namespace Infrastructure.ExternalServices.Services;

/// <summary>
/// Заглушка для публикации событий (для разработки без RabbitMQ)
/// </summary>
public class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;

    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Публикация события (заглушка - только логирование)
    /// </summary>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        _logger.LogInformation("ЗАГЛУШКА: Событие {EventType} не опубликовано (RabbitMQ отключен)", typeof(TEvent).Name);
        return Task.CompletedTask;
    }
}
