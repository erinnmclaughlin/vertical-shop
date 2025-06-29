using System.Data.Common;

namespace VerticalShop.Api.Persistence;

/// <inheritdoc />
public sealed class PostgresDatabaseContext : IDatabaseContext
{
    private readonly NpgsqlDataSource _dataSource;
    
    private NpgsqlConnection? _connection;
    private NpgsqlConnection Connection => _connection ??= _dataSource.OpenConnection();

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
