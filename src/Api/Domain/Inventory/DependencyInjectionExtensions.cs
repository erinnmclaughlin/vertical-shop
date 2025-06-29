using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Domain.Inventory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.TryAddScoped<IInventoryRepository, PostgresInventoryRepository>();
        return services;
    }
}