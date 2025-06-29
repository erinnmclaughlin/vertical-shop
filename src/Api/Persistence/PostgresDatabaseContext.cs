using System.Data.Common;

namespace VerticalShop.Api.Persistence;

/// <inheritdoc />
public sealed class PostgresDatabaseContext : IDatabaseContext
{
    private readonly NpgsqlDataSource _dataSource;
    
    private NpgsqlConnection? _connection;
    
    /// <inheritdoc />
    public DbConnection Connection => _connection ??= _dataSource.OpenConnection();

    /// <inheritdoc />
    public DbTransaction? CurrentTransaction { get; private set; }
    
    /// <summary>
    /// Creates a new <see cref="PostgresDatabaseContext"/> instance.
    /// </summary>
    /// <param name="dataSource">The <see cref="NpgsqlDataSource"/> used to open a connection to the database.</param>
    public PostgresDatabaseContext(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <inheritdoc />
    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        CurrentTransaction = await Connection.BeginTransactionAsync(cancellationToken);
        return CurrentTransaction;
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction is null)
            throw new InvalidOperationException("No transaction is currently active.");
        
        await CurrentTransaction.CommitAsync(cancellationToken);
        CurrentTransaction = null;
    }

    /// <inheritdoc />
    public async Task InsertOutboxMessageAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await Connection.ExecuteAsync(
            "insert into outbox_messages(id, type, payload) values (@id, @type, @payload::jsonb)", 
            new
            {
                id = Guid.CreateVersion7(),
                type = typeof(T).FullName,
                payload = JsonSerializer.Serialize(message)
            },
            CurrentTransaction
        );
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        CurrentTransaction?.Dispose();
        _connection?.Dispose();
        _connection = null;
    }
}
