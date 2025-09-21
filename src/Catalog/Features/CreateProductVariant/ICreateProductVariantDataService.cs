namespace VerticalShop.Catalog.Features.CreateProductVariant;

internal interface ICreateProductVariantDataService
{
    Task CreateProductVariant(Guid productId, string name, IReadOnlyDictionary<string, string>? attributes, CancellationToken cancellationToken = default);
}

internal sealed class CreateProductVariantNpgsqlDataService : ICreateProductVariantDataService
{
    private readonly INpgsqlDataStore _dataStore;

    public CreateProductVariantNpgsqlDataService(INpgsqlDataStore dataStore)
    {
        _dataStore = dataStore;
    }
    
    public async Task CreateProductVariant(Guid productId, string name, IReadOnlyDictionary<string, string>? attributes, CancellationToken cancellationToken = default)
    {
        await _dataStore.BeginTransactionAsync(cancellationToken);

        const string sql = "insert into catalog.product_variants (id, product_id, name, attributes) values (@Id, @ProductId, @Name, @Attributes)";

        await _dataStore.ExecuteAsync(sql, new { Id = Guid.CreateVersion7(), ProductId = productId, Name = name, Attributes = attributes }, cancellationToken);

        await _dataStore.CommitTransactionAsync(cancellationToken);
    }
}
