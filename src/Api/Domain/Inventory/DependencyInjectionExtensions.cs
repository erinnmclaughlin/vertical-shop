using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Domain.Inventory;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds inventory-related services to the service collection.
    /// </summary>
    /// <param name="services">The collection of service descriptors to which inventory services will be added.</param>
    /// <returns>An updated service collection with inventory services registered.</returns>
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.TryAddScoped<IInventoryRepository, PostgresInventoryRepository>();
        return services;
    }
}