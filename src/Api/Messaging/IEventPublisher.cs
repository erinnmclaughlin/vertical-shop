namespace ContextDrivenDevelopment.Api.Messaging;

public interface IEventPublisher<in T>
{
    Task PublishAsync(T @event);
}

public sealed class EventPublisher<T> : IEventPublisher<T>
{
    private readonly IEnumerable<IEventConsumer<T>> _consumers;

    public EventPublisher(IEnumerable<IEventConsumer<T>> consumers)
    {
        _consumers = consumers;
    }
    
    public async Task PublishAsync(T @event)
    {
        foreach (var consumer in _consumers)
        {
            await consumer.ConsumeAsync(@event);
        }
    }
}
