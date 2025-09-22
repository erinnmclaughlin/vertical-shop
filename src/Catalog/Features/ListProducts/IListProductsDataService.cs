namespace VerticalShop.Catalog.Features.ListProducts;

internal interface IListProductsDataService
{
    Task<List<ProductReference>> ListProductsAsync(int offset, int limit, CancellationToken cancellationToken);
}

internal sealed class ListProductsNpgsqlDataService : IListProductsDataService
{
    private readonly INpgsqlDataStore _dataStore;

    public ListProductsNpgsqlDataService(INpgsqlDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public async Task<List<ProductReference>> ListProductsAsync(int offset, int limit, CancellationToken cancellationToken)
    {
        const string sql = """
            select 
                p.id as "Id", 
                p.name as "Name",
                p.slug as "Slug"
            from catalog.products p
            offset @Offset
            limit @Limit
            """;

        var products = await _dataStore.QueryAsync<ProductReference>(sql, new { Offset = offset, Limit = limit }, cancellationToken);
        return [.. products];
    }
}