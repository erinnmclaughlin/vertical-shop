using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VerticalShop.Catalog.Migrations;

namespace VerticalShop.Catalog;

/// <summary>
/// Provides extension methods for configuring catalog-related services and API endpoints within the application.
/// </summary>
public static class CatalogApi
{
    public static void MigrateCatalogSchema(this IEndpointRouteBuilder app)
    {
        var configuration = app.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var services = new ServiceCollection()
            .AddScoped<IVersionTableMetaData, CatalogVersionTableMetaData>()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres();
                rb.WithGlobalConnectionString(configuration.GetConnectionString("vertical-shop-db"));
                rb.ScanIn(typeof(CatalogApi).Assembly).For.All();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        using var scope = services.BuildServiceProvider().CreateScope();
        scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
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
                IMediator mediator, 
                CancellationToken cancellationToken)
                => mediator.Send(command, cancellationToken)
            )
            .WithSummary("Create Product");

        // GET /catalog/products/{identifier}
        productsApi
            .MapGet("/products/{identifier}", (
                string identifier,
                IMediator mediator,
                [FromQuery] ProductIdentifierType identifierType,
                CancellationToken cancellationToken)
                => mediator.Send(new GetProduct.Query(identifier, identifierType), cancellationToken)
            )
            .WithSummary("Get Product");

        // GET /catalog/products
        productsApi
            .MapGet("/products", (
                [AsParameters] ListProducts.Query query,
                IMediator mediator,
                CancellationToken ct)
                => mediator.Send(query, ct)
            )
            .WithSummary("List Products");
        
        productsApi
            .MapPost("/products/{productId:guid}/variants", (
                Guid productId,
                CreateProductVariant.RequestBody request,
                IMediator mediator,
                CancellationToken ct)
                => mediator.Send(request.ToCommand(productId), ct)
            )
            .WithSummary("Create Product Variant");
    }
}
