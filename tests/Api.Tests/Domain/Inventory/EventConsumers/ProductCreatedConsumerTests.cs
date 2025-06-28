using ContextDrivenDevelopment.Api.Domain.Inventory;
using ContextDrivenDevelopment.Api.Domain.Products;
using Microsoft.Extensions.DependencyInjection;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Inventory.EventConsumers;

public sealed class ProductCreatedConsumerTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task WhenProductIsCreated_ItIsAddedToTheInventory()
    {
        using var scope = api.Services.CreateScope();
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var inventoryRepository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
        
        var productSlug = ProductSlug.Parse("test-product");
        var product = new Product { Slug = productSlug, Name = "Test Product" };
        await productRepository.CreateAsync(product, TestContext.Current.CancellationToken);
    }
}