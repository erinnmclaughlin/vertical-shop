using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog.Features.CreateProduct;

internal sealed class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, Results<Created, ValidationProblem, Conflict>>
{
    private readonly ICreateProductDataService _dataService;
    private readonly IValidator<CreateProductRequest> _validator;

    public CreateProductRequestHandler(ICreateProductDataService dataService, IValidator<CreateProductRequest> validator)
    {
        _dataService = dataService;
        _validator = validator;
    }

    public async Task<Results<Created, ValidationProblem, Conflict>> Handle(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (await _validator.ValidateAsync(request, cancellationToken) is { IsValid: false } error)
        {
            return TypedResults.ValidationProblem(error.ToDictionary());
        }

        var result = await _dataService.CreateProduct(request.Slug, request.Name, cancellationToken);

        return result.Match<Results<Created, ValidationProblem, Conflict>>(
            _ => TypedResults.Created(),
            _ => TypedResults.Conflict()
        );
    }
}
