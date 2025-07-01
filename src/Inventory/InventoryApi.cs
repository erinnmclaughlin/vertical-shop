using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace VerticalShop.Inventory;

/// <summary>
/// Provides extension methods to add inventory services and map inventory API endpoints.
/// </summary>
public static class InventoryApi
{
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
                IMediator mediator, 
                CancellationToken cancellationToken) 
                => mediator.Send(
                    new RestockInventoryItem.Command(productSlug, body.Quantity), 
                    cancellationToken)
            )
            .WithSummary("Restock Inventory Item");

        inventoryApi
            .MapGet("/items/{productSlug}/quantity", (
                string productSlug,
                IMediator mediator, 
                CancellationToken cancellationToken) 
                => mediator.Send(
                    new CheckQuantityInStock.Query(productSlug), 
                    cancellationToken)
            )
            .WithSummary("Check Quantity In Stock");
    }
}