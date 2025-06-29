using ContextDrivenDevelopment.Api.Inventory.Commands;
using ContextDrivenDevelopment.Api.Inventory.Queries;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Inventory;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds inventory-related services to the specified <see cref="WebApplicationBuilder"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to which the inventory services will be added.</param>
    public static void AddInventoryServices(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<IInventoryRepository, PostgresInventoryRepository>();
        builder.Services.TryAddTransient<ReceiveInventory.CommandHandler>();
        builder.Services.TryAddTransient<CheckQuantityInStock.QueryHandler>();
    }

    /// <summary>
    /// Maps the inventory API endpoints to the specified endpoint route builder.
    /// </summary>
    /// <param name="builder">The endpoint route builder to which the inventory API endpoints will be mapped.</param>
    public static void MapInventoryApi(this IEndpointRouteBuilder builder)
    {
        var inventoryApi = builder.MapGroup("/inventory").WithTags("Inventory API");
        
        inventoryApi.MapPost("/commands/receiveInventory", (
            ReceiveInventory.Command command,
            ReceiveInventory.CommandHandler handler,
            CancellationToken cancellationToken
        ) => handler.HandleAsync(command, cancellationToken));

        inventoryApi.MapGet("/queries/checkQuantityInStock", (
            string productSlug,
            CheckQuantityInStock.QueryHandler handler,
            CancellationToken cancellationToken
        ) => handler.Handle(productSlug, cancellationToken));
    }
}