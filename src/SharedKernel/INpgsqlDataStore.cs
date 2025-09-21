using Dapper;
using Npgsql;
using System.Text.Json;

namespace VerticalShop;

public interface INpgsqlDataStore : IDisposable
{
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task InsertOutboxMessageAsync<T>(T message, CancellationToken cancellationToken);

    Task<int> ExecuteAsync(string sql, object? param, CancellationToken cancellationToken);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param, CancellationToken cancellationToken);

    public Task<IEnumerable<dynamic>> QueryAsync(string sql, object? param, CancellationToken cancellationToken)
    {
        return QueryAsync<dynamic>(sql, param, cancellationToken);
    }
}

internal sealed class NpgsqlDataStore : INpgsqlDataStore
{
    private readonly NpgsqlDataSource _dataSource;

    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;

    public NpgsqlDataStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public void Dispose()
    {
        if (_transaction is not null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        if (_connection is not null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction is already in progress.");

        _connection ??= await _dataSource.OpenConnectionAsync(cancellationToken);
        _transaction = await _connection.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction in progress.");

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task<int> ExecuteAsync(string sql, object? param, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _connection ??= await _dataSource.OpenConnectionAsync(cancellationToken);
        return await _connection.ExecuteAsync(sql, param, _transaction);
    }

    public async Task InsertOutboxMessageAsync<T>(T message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _connection ??= await _dataSource.OpenConnectionAsync(cancellationToken);
        await _connection.ExecuteAsync(
            "insert into outbox_messages(type, payload) values (@type, @payload::jsonb)",
            new
            {
                type = typeof(T).FullName,
                payload = JsonSerializer.Serialize(message)
            },
            _transaction
        );
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connection ??= await _dataSource.OpenConnectionAsync(cancellationToken);
        return await _connection.QueryAsync<T>(sql, param, _transaction);
    }
}