using System.Net.Http.Json;
using VerticalShop.Catalog;

namespace VerticalShop.EndToEndTests;

public class ProductsApiTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;

    public ProductsApiTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnSuccessStatusCode()
    {
        // Arrange
        using var client = _fixture.CreateHttpClient("api");
        
        // Act
        var request = new CreateProduct.Command("test-product-1", "Test Product");
        var response = await client.PostAsJsonAsync("catalog/products", request, TestContext.Current.CancellationToken);
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task InventoryShouldHaveProductAfterProductIsCreated()
    {
        // Arrange
        using var client = _fixture.CreateHttpClient("api");
        
        // Act
        var request = new CreateProduct.Command("test-product-2", "Test Product");
        var response = await client.PostAsJsonAsync("catalog/products", request, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Give OutboxProcessor time to do its thing. TODO: Configure time interval to be lower
        await Task.Delay(TimeSpan.FromSeconds(11), TestContext.Current.CancellationToken);

        var quantity = await client.GetStringAsync("inventory/items/test-product-2/quantity", TestContext.Current.CancellationToken);
        Assert.Equal("0", quantity);
    }
}