using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Products.Commands.CreateProduct;

using Command = Api.Domain.Products.Commands.CreateProduct.Command;
using CommandHandler = Api.Domain.Products.Commands.CreateProduct.CommandHandler;

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

        // Assert
        Assert.IsType<Created>(result.Result);
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var getResult = await unitOfWork.Products.GetBySlugAsync(slug, TestContext.Current.CancellationToken);
        var createdProduct = Assert.IsType<Product>(getResult.Value);
        Assert.Equal(command.Slug, createdProduct.Slug);
        Assert.Equal(command.Name, createdProduct.Name);
        Assert.Equal(command.Attributes, createdProduct.Attributes);
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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var getResult = await unitOfWork.Products.GetBySlugAsync(slug, TestContext.Current.CancellationToken);
        Assert.IsType<OneOf.Types.NotFound>(getResult.Value);
    }
} 
