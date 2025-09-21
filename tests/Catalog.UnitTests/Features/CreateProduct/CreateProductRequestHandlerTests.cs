using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using OneOf.Types;
using VerticalShop.Catalog.ErrorTypes;
using VerticalShop.Catalog.Features.CreateProduct;

namespace VerticalShop.Catalog.UnitTests.Features.CreateProduct;

public sealed class CreateProductRequestHandlerTests
{
    private readonly ICreateProductDataService _dataService = Substitute.For<ICreateProductDataService>();
    private readonly IValidator<CreateProductRequest> _validator = Substitute.For<IValidator<CreateProductRequest>>();

    [Fact]
    public async Task Handle_ReturnsValidationProblem_WhenValidationFails()
    {
        var request = new CreateProductRequest { Slug = "apple", Name = "Apple" };

        _validator
            .ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var handler = new CreateProductRequestHandler(_dataService, _validator);
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        var validationProblem = Assert.IsType<ValidationProblem>(result.Result);
        Assert.True(validationProblem.ProblemDetails.Errors.TryGetValue("Name", out var errors));
        Assert.Contains("Name is required", errors);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenDataStoreReturnsDuplicateSlugError()
    {
        var request = new CreateProductRequest { Slug = "apple", Name = "Apple" };

        _dataService
            .CreateProduct(Arg.Any<Guid>(), request.Slug, request.Name, Arg.Any<CancellationToken>())
            .Returns(new DuplicateSlug(request.Slug));

        var handler = new CreateProductRequestHandler(_dataService, _validator);
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.IsType<Conflict>(result.Result);
    }

    [Fact]
    public async Task Handle_ReturnsCreated_WhenDataStoreReturnsSuccess()
    {
        var request = new CreateProductRequest { Slug = "apple", Name = "Apple" };

        _dataService
            .CreateProduct(Arg.Any<Guid>(), request.Slug, request.Name, Arg.Any<CancellationToken>())
            .Returns(new Success());

        var handler = new CreateProductRequestHandler(_dataService, _validator);
        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.IsType<Created>(result.Result);
    }
}
