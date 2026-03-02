namespace Services.Abstractions.Interfaces;

/// <summary>
/// Интерфейс для публикации событий в RabbitMQ
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Публикует событие в RabbitMQ
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <param name="event">Событие для публикации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}
