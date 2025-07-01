using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides extension methods for configuring catalog-related services and API endpoints within the application.
/// </summary>
public static class CatalogModule
{
    /// <summary>
    /// Adds catalog-related services to the dependency injection container.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IHostApplicationBuilder"/> used to configure the application's services.
    /// </param>
    public static Assembly AddCatalogModule<T>(this T builder) where T : IHostApplicationBuilder
    {
        builder.Services.TryAddTransient<CreateProduct.CommandHandler>();
        builder.Services.TryAddTransient<GetProduct.QueryHandler>();
        builder.Services.TryAddTransient<ListProducts.QueryHandler>();
        builder.Services.TryAddTransient<CreateProductVariant.CommandHandler>();
        return typeof(CatalogModule).Assembly;       
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
                CreateProduct.Command command, 
                CreateProduct.CommandHandler handler, 
                CancellationToken ct)
                => handler.HandleAsync(command, ct)
            )
            .WithSummary("Create Product");

        // GET /catalog/products/{identifier}
        productsApi
            .MapGet("/products/{identifier}", (
                string identifier,
                GetProduct.QueryHandler handler,
                [FromQuery] ProductIdentifierType identifierType = ProductIdentifierType.Slug,
                CancellationToken ct = default)
                => handler.GetProduct(identifier, identifierType, ct)
            )
            .WithSummary("Get Product");

        // GET /catalog/products
        productsApi
            .MapGet("/products", (
                [AsParameters] ListProducts.Query query,
                ListProducts.QueryHandler handler,
                CancellationToken ct)
                => handler.Handle(query, ct)
            )
            .WithSummary("List Products");
        
        productsApi
            .MapPost("/products/{productId:guid}/variants", (
                Guid productId,
                CreateProductVariant.RequestBody request,
                CreateProductVariant.CommandHandler handler,
                CancellationToken ct)
                => handler.Handle(new CreateProductVariant.Command(productId, request.Name, request.Attributes), ct)
            )
            .WithSummary("Create Product Variant");
    }
}
