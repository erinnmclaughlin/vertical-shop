using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Products;
using ContextDrivenDevelopment.Api.Products.Events;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

using Command = ContextDrivenDevelopment.Api.Products.Commands.CreateProduct.Command;
using CommandHandler = ContextDrivenDevelopment.Api.Products.Commands.CreateProduct.CommandHandler;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Products.Commands.CreateProduct;

public sealed class CreateProductHandlerTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var slug = ProductSlug.Parse(Guid.NewGuid().ToString());
        var command = new Command
        {
            Slug = slug,
            Name = "Test Product",
            Attributes = new Dictionary<string, string>
            {
                ["color"] = "yellow",
                ["size"] = "small"
            }
        };

        // Act
        using var scope = api.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert that the response type is correct
        Assert.IsType<Created>(result.Result);
        
        // Assert that the product was persisted to the database
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var getResult = await productRepository.GetBySlugAsync(slug, TestContext.Current.CancellationToken);
        var createdProduct = Assert.IsType<Product>(getResult.Value);
        Assert.Equal(command.Slug, createdProduct.Slug);
        Assert.Equal(command.Name, createdProduct.Name);
        Assert.Equal(command.Attributes, createdProduct.Attributes);
        
        // Assert that the "ProductCreated" event was published to the outbox
        var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();
        var message = await outbox.GetNextMessageOfType<ProductCreated>(TestContext.Current.CancellationToken);
        Assert.NotNull(message);
        Assert.Equivalent(new ProductCreated(slug), message.Message);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_ShouldNotCreateProduct()
    {
        // Arrange
        var slug = ProductSlug.Parse(Guid.NewGuid().ToString());
        var command = new Command
        {
            Slug = slug,
            Name = "", // name is required
            Attributes = new Dictionary<string, string>
            {
                ["color"] = "yellow",
                ["size"] = "small"
            }
        };
        
        // Act
        using var scope = api.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert  
        Assert.IsType<ValidationProblem>(result.Result); 
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var getResult = await productRepository.GetBySlugAsync(slug, TestContext.Current.CancellationToken);
        Assert.IsType<OneOf.Types.NotFound>(getResult.Value);
    }
} 
