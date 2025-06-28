using AutoBogus;
using ContextDrivenDevelopment.Api.Domain.Products.Commands;
using ContextDrivenDevelopment.Api.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace ContextDrivenDevelopment.Api.Tests.Domain.Products.Commands;

public sealed class CreateProductHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateProductAndReturnCreated()
    {
        // Arrange
        var command = AutoFaker.Generate<CreateProduct.Command>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = TestUtils.CreateMockValidatorFor(command, isValid: true);
        var handler = new CreateProduct.CommandHandler(unitOfWork, validator);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await unitOfWork.ReceivedWithAnyArgs().CommitAsync(Arg.Any<CancellationToken>());
        Assert.IsType<Created>(result.Result);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_ShouldReturnValidationProblem()
    {
        // Arrange
        var command = AutoFaker.Generate<CreateProduct.Command>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = TestUtils.CreateMockValidatorFor(command, isValid: false);
        var handler = new CreateProduct.CommandHandler(unitOfWork, validator);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(TestContext.Current.CancellationToken);     
        Assert.IsType<ValidationProblem>(result.Result);  
    }
} 
