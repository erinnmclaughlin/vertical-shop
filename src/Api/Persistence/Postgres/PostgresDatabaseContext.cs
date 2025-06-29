using System.Data.Common;
using ContextDrivenDevelopment.Api.Inventory;
using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Products;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres;

/// <inheritdoc />
public sealed class PostgresDatabaseContext : IDatabaseContext
{
    private readonly NpgsqlDataSource _dataSource;
    
    private NpgsqlConnection? _connection;
    private PostgresInventoryRepository? _inventory;
    private PostgresOutbox? _outbox;
    private PostgresProductRepository? _products;
    
    private NpgsqlConnection Connection => _connection ??= _dataSource.OpenConnection();

    /// <inheritdoc />
    public IOutbox Outbox => _outbox ??= new PostgresOutbox(Connection);
    
    /// <inheritdoc />
    public IInventoryRepository Inventory => _inventory ??= new PostgresInventoryRepository(Connection);
    
    /// <inheritdoc />
    public IProductRepository Products => _products ??= new PostgresProductRepository(Connection);
    
    /// <summary>
    /// Creates a new <see cref="PostgresDatabaseContext"/> instance.
    /// </summary>
    /// <param name="dataSource">The <see cref="NpgsqlDataSource"/> used to open a connection to the database.</param>
    public PostgresDatabaseContext(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <inheritdoc />
    public DbTransaction BeginTransaction() => Connection.BeginTransaction();
    
    /// <inheritdoc />
    public void Dispose()
    {
        _connection?.Dispose();
        _connection = null;
    }
}
