using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Domain.Products.Persistence;
using Npgsql;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres;

/// <inheritdoc />
public sealed class PostgresUnitOfWork : IUnitOfWork
{
    // Injected Services
    private readonly NpgsqlDataSource _dataSource;
    
    // Backing fields
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;
    
    // Internal Properties
    internal NpgsqlConnection Connection => _connection ??= _dataSource.OpenConnection();
    internal NpgsqlTransaction Transaction => _transaction ??= Connection.BeginTransaction();
    
    // Public Properties
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
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Transaction.CommitAsync(cancellationToken);
        await DisposeTransactionAsync();
    }
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
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
    
    private async ValueTask DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}