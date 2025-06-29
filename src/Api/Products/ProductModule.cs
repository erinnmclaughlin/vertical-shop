using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VerticalShop.Api.Products;

/// <summary>
/// Provides extension methods for configuring product-related services and API endpoints
/// within the application.
/// </summary>
public static class ProductModule
{
    /// <summary>
    /// Adds product-related services to the dependency injection container.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="WebApplicationBuilder"/> used to configure the application's services.
    /// </param>
    public static void AddProductServices(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<IProductRepository, PostgresProductRepository>();
        builder.Services.TryAddTransient<CreateProduct.CommandHandler>();
        builder.Services.TryAddTransient<GetProduct.QueryHandler>();
        builder.Services.TryAddTransient<ListProducts.QueryHandler>();
    }

    /// <summary>
    /// Configures the product-related API endpoints for the application.
    /// </summary>
    /// <param name="app">
    /// The <see cref="IEndpointRouteBuilder"/> used to define and configure API routes.
    /// </param>
    public static void MapProductApi(this IEndpointRouteBuilder app)
    {
        var productsApi = app
            .MapGroup("/products")
            .WithTags("Products API");
        
        productsApi
            .MapPost("/", (
                CreateProduct.Command command, 
                CreateProduct.CommandHandler handler, 
                CancellationToken ct)
                => handler.HandleAsync(command, ct)
            )
            .WithSummary("Create Product");

        productsApi
            .MapGet("/{identifier}", (
                string identifier,
                GetProduct.QueryHandler handler,
                [AllowedValues("id", "slug")] // TODO: Figure out how to get OpenAPI to pick up on this
                string identifierType = "id",
                CancellationToken ct = default)
                => identifierType is "slug" 
                    ? handler.GetBySlug(ProductSlug.Parse(identifier), ct) 
                    : handler.GetById(ProductId.Parse(identifier), ct)
            )
            .WithSummary("Get Product");

        productsApi
            .MapGet("/", (
                [AsParameters] ListProducts.Query query,
                ListProducts.QueryHandler handler,
                CancellationToken ct)
                => handler.Handle(query, ct)
            )
            .WithSummary("List Products");
    }
}
