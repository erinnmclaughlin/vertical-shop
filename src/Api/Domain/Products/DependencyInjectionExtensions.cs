using ContextDrivenDevelopment.Api.Domain.Products.Commands;
using ContextDrivenDevelopment.Api.Domain.Products.Queries;
using ContextDrivenDevelopment.Api.Persistence;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContextDrivenDevelopment.Api.Domain.Products;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds product-related services to the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// An instance of <see cref="IServiceCollection"/> to register services with.
    /// </param>
    /// <returns>
    /// The same instance of the <paramref name="services"/> parameter for chaining.
    /// </returns>
    public static IServiceCollection AddProductServices(this IServiceCollection services)
    {
        services.TryAddTransient(sp => sp.GetRequiredService<IUnitOfWork>().Products);
        
        services.TryAddTransient<CreateProduct.CommandHandler>();
        services.TryAddTransient<GetProduct.QueryHandler>();
        
        return services;
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
            Guid id,
            GetProduct.QueryHandler handler
        ) => handler.GetById(id));
        
        productsApi.MapGet("/queries/getBySlug", (
            string slug,
            GetProduct.QueryHandler handler
        ) => handler.GetBySlug(slug));
    }
}
