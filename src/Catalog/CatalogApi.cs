using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using VerticalShop.Catalog.Features.CreateProduct;
using VerticalShop.Catalog.Features.CreateProductVariant;
using VerticalShop.Catalog.Features.GetProduct;
using VerticalShop.Catalog.Features.ListProducts;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides extension methods for configuring catalog-related services and API endpoints within the application.
/// </summary>
public static class CatalogApi
{
    /// <summary>
    /// Adds the necessary services for the catalog API to the application's dependency injection container.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static Assembly AddCatalogApi(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<ICreateProductDataService, CreateProductNpgsqlDataService>();
        builder.Services.AddTransient<ICreateProductVariantDataService, CreateProductVariantNpgsqlDataService>();
        builder.Services.AddTransient<IGetProductDataService, GetProductNpgsqlDataService>();
        builder.Services.AddTransient<IListProductsDataService, ListProductsNpgsqlDataService>();
        return typeof(CatalogApi).Assembly;
    }

    /// <summary>
    /// Configures the product-related API endpoints for the application.
    /// </summary>
    /// <param name="app">
    /// The <see cref="IEndpointRouteBuilder"/> used to define and configure API routes.
    /// </param>
    public static void MapCatalogApi(this IEndpointRouteBuilder app)
    {
        var productsApi = app
            .MapGroup("/catalog")
            .WithTags("Catalog API");
        
        // POST /catalog/products
        productsApi
            .MapPost("/products", (
                CreateProductRequest command, 
                IMediator mediator, 
                CancellationToken cancellationToken)
                => mediator.Send(command, cancellationToken)
            )
            .WithSummary("Create Product");

        // GET /catalog/products/{identifier}
        productsApi
            .MapGet("/products/{identifier}", (
                [AsParameters] GetProductRequest request,
                IMediator mediator,
                CancellationToken cancellationToken)
                => mediator.Send(request, cancellationToken)
            )
            .WithSummary("Get Product");

        // GET /catalog/products
        productsApi
            .MapGet("/products", (
                [AsParameters] ListProductsRequest request,
                IMediator mediator,
                CancellationToken ct)
                => mediator.Send(request, ct)
            )
            .WithSummary("List Products");

        // POST /catalog/product-variants
        productsApi
            .MapPost("/product-variants", (
                CreateProductVariantRequest request,
                IMediator mediator,
                CancellationToken ct)
                => mediator.Send(request, ct)
            )
            .WithSummary("Create Product Variant");
    }
}
