using System.Net;
using System.Net.Http.Json;
using ContextDrivenDevelopment.Api.Domain.Products.Commands;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Products.Commands;

public sealed class CreateProductEndpointTests(ApiApplication api) : IClassFixture<ApiApplication>
{
    [Fact]
    public async Task ApiReturnsSuccessWhenRequestIsValid()
    {
        var command = new CreateProduct.Command
        {
            Slug = "test-product",
            Name = "Test Product"
        };

        var response = await SendRequest(command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ApiReturnsErrorWhenRequestIsInvalid()
    {
        var command = new CreateProduct.Command
        {
            Slug = "",
            Name = "Test Product"
        };
        
        var response = await SendRequest(command);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendRequest(CreateProduct.Command command)
    { 
        using var httpClient = api.CreateClient();
        return await httpClient.PostAsJsonAsync("products/commands/createProduct", command, TestContext.Current.CancellationToken);
    }
}
