using System.Diagnostics.CodeAnalysis;
using MassTransit;
using Microsoft.Extensions.Logging;
using VerticalShop.IntegrationEvents.Products;

namespace VerticalShop.Inventory.EventConsumers;

/// <summary>
/// A consumer responsible for handling the <see cref="ProductCreated"/> event
/// and performing actions related to inventory management.
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed class ProductCreatedConsumer(
    IInventoryRepository inventory,
    ILogger<ProductCreatedConsumer> logger
) : IConsumer<ProductCreated>
{
    private readonly IInventoryRepository _inventory = inventory;
    private readonly ILogger<ProductCreatedConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        _logger.LogInformation("Consuming ProductCreated event from Inventory module. ProductSlug: {ProductSlug}", context.Message.ProductSlug);
        var item = InventoryItem.CreateNew(context.Message.ProductSlug);
        await _inventory.UpsertAsync(item, context.CancellationToken);
    }
}
