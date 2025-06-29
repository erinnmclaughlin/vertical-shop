using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace VerticalShop.Inventory;

/// <summary>
/// Provides extension methods to add inventory services and map inventory API endpoints.
/// </summary>
public static class InventoryModule
{
    /// <summary>
    /// Adds inventory-related services to the specified <see cref="IHostApplicationBuilder"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to which the inventory services will be added.</param>
    public static Assembly AddInventoryModule<T>(this T builder) where T : IHostApplicationBuilder
    {
        builder.Services.TryAddScoped<IInventoryRepository, InventoryRepository>();
        builder.Services.TryAddTransient<RestockInventoryItem.CommandHandler>();
        builder.Services.TryAddTransient<CheckQuantityInStock.QueryHandler>();
        return typeof(InventoryModule).Assembly;
    }

    /// <summary>
    /// Maps the inventory API endpoints to the specified endpoint route builder.
    /// </summary>
    /// <param name="builder">The endpoint route builder to which the inventory API endpoints will be mapped.</param>
    public static void MapInventoryApi(this IEndpointRouteBuilder builder)
    {
        var inventoryApi = builder
            .MapGroup("/inventory")
            .WithTags("Inventory API");
        
        inventoryApi
            .MapPost("/items/{productSlug}/restock", (
                string productSlug,
                RestockInventoryItem.RequestBody body,
                RestockInventoryItem.CommandHandler handler, 
                CancellationToken cancellationToken) 
                => handler.HandleAsync(
                    new RestockInventoryItem.Command(productSlug, body.Quantity), 
                    cancellationToken)
            )
            .WithSummary("Restock Inventory Item");

        inventoryApi
            .MapGet("/items/{productSlug}/quantity", (
                string productSlug,
                CheckQuantityInStock.QueryHandler handler,
                CancellationToken cancellationToken) 
                => handler.Handle(productSlug, cancellationToken)
            )
            .WithSummary("Check Quantity In Stock");
    }
}