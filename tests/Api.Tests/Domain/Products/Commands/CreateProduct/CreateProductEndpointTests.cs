using System.Net;
using System.Net.Http.Json;

namespace VerticalShop.Api.Tests.Domain.Products.Commands.CreateProduct;

using Command = Api.Products.CreateProduct.Command;

public sealed class CreateProductEndpointTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task ApiReturnsSuccessWhenRequestIsValid()
    {
        var response = await SendRequest(new Command
        {
            Slug = "test-product",
            Name = "Test Product"
        });
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ApiReturnsErrorWhenRequestIsInvalid()
    {
        var response = await SendRequest(new Command
        {
            Slug = "",
            Name = "Test Product"
        });
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendRequest(Command command)
    { 
        using var httpClient = api.CreateClient();
        return await httpClient.PostAsJsonAsync("products", command, TestContext.Current.CancellationToken);
    }
}
