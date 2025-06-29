using System.Data.Common;
using ContextDrivenDevelopment.Api.Domain.Inventory;
using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Messaging;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres;

/// <inheritdoc />
public sealed class PostgresUnitOfWork : IUnitOfWork
{
    // Injected Services
    private readonly NpgsqlDataSource _dataSource;
    
    private NpgsqlConnection? _connection;
    internal NpgsqlConnection Connection => _connection ??= _dataSource.OpenConnection();
    
    /// <inheritdoc />
    public IOutbox Outbox { get; }
    
    /// <inheritdoc />
    public IInventoryRepository Inventory => _inventory ??= new PostgresInventoryRepository(this);
    private PostgresInventoryRepository? _inventory;
    
    /// <inheritdoc />
    public IProductRepository Products => _products ??= new PostgresProductRepository(this);
    private PostgresProductRepository? _products;
    
    /// <summary>
    /// Creates a new <see cref="PostgresUnitOfWork"/> instance.
    /// </summary>
    /// <param name="dataSource">The <see cref="NpgsqlDataSource"/> used to open a connection to the database.</param>
    public PostgresUnitOfWork(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        Outbox = new PostgresOutbox(this);
    }

    /// <inheritdoc />
    public DbTransaction BeginTransaction() => Connection.BeginTransaction();
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeConnectionAsync();
    }

    private async ValueTask DisposeConnectionAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
