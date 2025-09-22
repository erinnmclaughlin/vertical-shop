using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using VerticalShop.Catalog.Features.CreateProductVariant;

namespace VerticalShop.Catalog.UnitTests.Features.CreateProductVariant;

public sealed class CreateProductVariantHandlerTests
{
    private readonly ICreateProductVariantDataService _dataService = Substitute.For<ICreateProductVariantDataService>();
    private readonly IValidator<CreateProductVariantRequest> _validator = Substitute.For<IValidator<CreateProductVariantRequest>>();
    
    [Fact]
    public async Task Handle_ReturnsValidationProblem_WhenValidationFails()
    {
        var request = new CreateProductVariantRequest
        {
            ProductId = Guid.NewGuid(),
            Name = "Some Variant"
        };

        _validator
            .ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var handler = new CreateProductVariantRequestHandler(_dataService, _validator);
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        var validationProblem = Assert.IsType<ValidationProblem>(result.Result);
        Assert.True(validationProblem.ProblemDetails.Errors.TryGetValue("Name", out var errors));
        Assert.Contains("Name is required", errors);
    }

    [Fact]
    public async Task Handle_ReturnsCreated_WhenSuccessful()
    {
        var request = new CreateProductVariantRequest
        {
            ProductId = Guid.NewGuid(),
            Name = "Some Variant"
        };

        var handler = new CreateProductVariantRequestHandler(_dataService, _validator);
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.IsType<Created>(result.Result);
    }
}