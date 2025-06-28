using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Messaging;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(IEventPublisher<>), typeof(EventPublisher<>));
        return services;
    }
}
