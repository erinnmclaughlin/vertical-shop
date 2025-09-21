using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using System.Text.Json;

namespace VerticalShop.Catalog.Features.CreateProductVariant;

internal sealed class CreateProductVariantRequestHandler : IRequestHandler<CreateProductVariantRequest, Results<Created, ValidationProblem>>
{
    private readonly INpgsqlDataStore _dataStore;
    private readonly IValidator<CreateProductVariantRequest> _validator;

    public CreateProductVariantRequestHandler(INpgsqlDataStore dataStore, IValidator<CreateProductVariantRequest> validator)
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

        await _dataStore.ExecuteAsync(
            """
            INSERT INTO catalog.product_variants (product_id, name, attributes) 
            VALUES (@ProductId, @Name, @Attributes::jsonb)
            """,
            new
            {
                request.ProductId,
                request.Name,
                Attributes = JsonSerializer.Serialize(request.Attributes)
            },
            cancellationToken
        );

        return TypedResults.Created();
    }
}
