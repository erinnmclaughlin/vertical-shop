using ContextDrivenDevelopment.Api.Domain.Inventory.EventConsumers;
using ContextDrivenDevelopment.Api.Domain.Products.Events;
using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Persistence;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Domain.Inventory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.TryAddScoped(sp => sp.GetRequiredService<IUnitOfWork>().Inventory);
        services.TryAddTransient<IEventConsumer<ProductCreated>, ProductCreatedConsumer>();
        return services;
    }
}