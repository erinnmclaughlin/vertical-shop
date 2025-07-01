using System.Diagnostics.CodeAnalysis;
using Dapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using Npgsql;
using VerticalShop.IntegrationEvents.Products;

namespace VerticalShop.Inventory;

/// <summary>
/// A consumer responsible for handling the <see cref="ProductCreated"/> event
/// and performing actions related to inventory management.
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed class ProductCreatedConsumer(
    NpgsqlDataSource dataSource,
    ILogger<ProductCreatedConsumer> logger
) : IConsumer<ProductCreated>
{
    private readonly NpgsqlDataSource _dataSource = dataSource;
    private readonly ILogger<ProductCreatedConsumer> _logger = logger;

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        _logger.LogInformation("Consuming ProductCreated event from Inventory module. ProductSlug: {ProductSlug}", context.Message.ProductSlug);

        await using var connection = await _dataSource.OpenConnectionAsync(context.CancellationToken);

        await connection.ExecuteAsync(
            """
            insert into inventory.items (product_slug, quantity)
            values (@ProductSlug, 0)
            on conflict (product_slug) do nothing
            """,
            new { context.Message.ProductSlug });
    }
}
