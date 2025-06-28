namespace ContextDrivenDevelopment.Api.Messaging;

public interface IEventConsumer<in T>
{
    Task ConsumeAsync(T @event);
}