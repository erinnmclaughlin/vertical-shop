using Npgsql;
using OneOf;
using VerticalShop.Catalog.ErrorTypes;
using VerticalShop.IntegrationEvents.Products;

namespace VerticalShop.Catalog.Features.CreateProduct;

internal interface ICreateProductDataService
{
    /// <summary>
    /// Creates a new product with the specified identifier, slug, and name.
    /// </summary>
    /// <param name="productSlug">The URL-friendly slug that uniquely identifies the product. Cannot be null or empty.</param>
    /// <param name="productName">The display name of the product. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task<OneOf<Guid, DuplicateSlug>> CreateProduct(string productSlug, string productName, CancellationToken cancellationToken = default);
}

internal sealed class CreateProductNpgsqlDataService : ICreateProductDataService
{
    private readonly INpgsqlDataStore _dataStore;

    public CreateProductNpgsqlDataService(INpgsqlDataStore dataStore) 
    {
        _dataStore = dataStore;
    }
    
    public async Task<OneOf<Guid, DuplicateSlug>> CreateProduct(string productSlug, string productName, CancellationToken cancellationToken = default)
    {
        await _dataStore.BeginTransactionAsync(cancellationToken);

        const string sql = "insert into catalog.products (id, name, slug) values (@Id, @Name, @Slug)";
        var productId = Guid.CreateVersion7();

        try
        {
            await _dataStore.ExecuteAsync(sql, new { Id = productId, Name = productName, Slug = productSlug }, cancellationToken);
        }
        catch (PostgresException ex) when (ex.IsUniqueConstraintViolationOnColumn("slug"))
        {
            return new DuplicateSlug(productSlug);
        }

        await _dataStore.InsertOutboxMessageAsync(new ProductCreated(productId, productSlug, productName), cancellationToken);

        await _dataStore.CommitTransactionAsync(cancellationToken);

        return productId;
    }
}