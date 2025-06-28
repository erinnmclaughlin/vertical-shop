using AutoBogus;
using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Products.Commands.CreateProduct;

using Command = Api.Domain.Products.Commands.CreateProduct.Command;
using CommandHandler = Api.Domain.Products.Commands.CreateProduct.CommandHandler;
using CommandValidator = Api.Domain.Products.Commands.CreateProduct.CommandValidator;

public sealed class CreateProductHandlerTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = new Command
        {
            Slug = "test-product",
            Name = "Test Product",
            Attributes = new Dictionary<string, string>
            {
                ["color"] = "yellow",
                ["size"] = "small"
            }
        };

        //Act
        using var scope = api.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var handler = new CommandHandler(unitOfWork, new CommandValidator(unitOfWork));
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);
        Assert.IsType<Created>(result.Result);

        // Assert
        var getResult = await unitOfWork.Products.GetBySlugAsync(command.Slug, TestContext.Current.CancellationToken);
        var createdProduct = Assert.IsType<Product>(getResult.Value);
        Assert.Equal(command.Slug, createdProduct.Slug);
        Assert.Equal(command.Name, createdProduct.Name);
        Assert.Equal(command.Attributes, createdProduct.Attributes);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_ShouldNotCreateProduct()
    {
        // Arrange
        var command = AutoFaker.Generate<Command>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = TestUtils.CreateMockValidatorFor(command, isValid: false);
        var handler = new CommandHandler(unitOfWork, validator);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(TestContext.Current.CancellationToken);     
        Assert.IsType<ValidationProblem>(result.Result);  
    }
} 
