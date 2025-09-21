using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VerticalShop.Catalog.Features.CreateProductVariant;

internal sealed class CreateProductVariantRequestHandler : IRequestHandler<CreateProductVariantRequest, Results<Created, ValidationProblem>>
{
    private readonly ICreateProductVariantDataService _dataStore;
    private readonly IValidator<CreateProductVariantRequest> _validator;

    public CreateProductVariantRequestHandler(ICreateProductVariantDataService dataStore, IValidator<CreateProductVariantRequest> validator)
    {
        _dataStore = dataStore;
        _validator = validator;
    }

    public async Task<Results<Created, ValidationProblem>> Handle(CreateProductVariantRequest request, CancellationToken cancellationToken)
    {
        if (await _validator.ValidateAsync(request, cancellationToken) is { IsValid: false } error)
        {
            return TypedResults.ValidationProblem(error.ToDictionary());
        }

        await _dataStore.CreateProductVariant(request.ProductId, request.Name, request.Attributes, cancellationToken);
        return TypedResults.Created();
    }
}
