using ContextDrivenDevelopment.Api.Products.Commands;
using ContextDrivenDevelopment.Api.Products.Queries;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Products;

public static class DependencyInjectionExtensions
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
    }

    /// <summary>
    /// Configures the product-related API endpoints for the application.
    /// </summary>
    /// <param name="app">
    /// The <see cref="IEndpointRouteBuilder"/> used to define and configure API routes.
    /// </param>
    public static void MapProductApi(this IEndpointRouteBuilder app)
    {
        var productsApi = app.MapGroup("/products").WithTags("Products API");
        
        productsApi.MapPost("/commands/createProduct", (
            CreateProduct.Command command, 
            CreateProduct.CommandHandler handler, 
            CancellationToken ct
        ) => handler.HandleAsync(command, ct));

        productsApi.MapGet("/queries/getById", (
            string id,
            GetProduct.QueryHandler handler
        ) => handler.GetById(ProductId.Parse(id)));
        
        productsApi.MapGet("/queries/getBySlug", (
            string slug,
            GetProduct.QueryHandler handler
        ) => handler.GetBySlug(ProductSlug.Parse(slug)));
    }
}
