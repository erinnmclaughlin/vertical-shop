using AutoBogus;
using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Products.Commands.CreateProduct;

public sealed class CreateProductHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = AutoFaker.Generate<Api.Domain.Products.Commands.CreateProduct.Command>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = TestUtils.CreateMockValidatorFor(command, isValid: true);
        var handler = new Api.Domain.Products.Commands.CreateProduct.CommandHandler(unitOfWork, validator);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        var expectedProduct = Arg.Is<Product>(x => x.Name == command.Name && x.Slug == command.Slug);
        await unitOfWork.Products.Received().CreateAsync(expectedProduct, Arg.Any<CancellationToken>());
        await unitOfWork.ReceivedWithAnyArgs().CommitAsync(Arg.Any<CancellationToken>());
        Assert.IsType<Created>(result.Result);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_ShouldNotCreateProduct()
    {
        // Arrange
        var command = AutoFaker.Generate<Api.Domain.Products.Commands.CreateProduct.Command>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = TestUtils.CreateMockValidatorFor(command, isValid: false);
        var handler = new Api.Domain.Products.Commands.CreateProduct.CommandHandler(unitOfWork, validator);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(TestContext.Current.CancellationToken);     
        Assert.IsType<ValidationProblem>(result.Result);  
    }
} 
