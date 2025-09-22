using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OneOf;
using VerticalShop.Catalog.Models;

namespace VerticalShop.Catalog.Features.GetProduct;

internal sealed class GetProductRequestHandler : IRequestHandler<GetProductRequest, Results<Ok<ProductDetail>, NotFound>>
{
    private readonly IGetProductDataService _dataSource;

    public GetProductRequestHandler(IGetProductDataService dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Results<Ok<ProductDetail>, NotFound>> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataSource.GetProductAsync(ParseIdentifier(request), cancellationToken);

        return result.Match<Results<Ok<ProductDetail>, NotFound>>(
            product => TypedResults.Ok(product),
            _ => TypedResults.NotFound()
        );
    }

    private static OneOf<Guid, string> ParseIdentifier(GetProductRequest request)
    {
        if (request.IdentifierType is null)
        {
            if (Guid.TryParse(request.Identifier, out var guid))
            {
                return guid;
            }
            else
            {
                return request.Identifier;
            }
        }
        else if (request.IdentifierType is ProductIdentifierType.Id)
        {
            return Guid.Parse(request.Identifier);
        }
        else
        {
            return request.Identifier;
        }
    }
}