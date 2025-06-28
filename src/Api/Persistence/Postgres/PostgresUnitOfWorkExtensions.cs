using Dapper;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres;

public static class PostgresUnitOfWorkExtensions
{
    /// <summary>
    /// Executes a SQL command asynchronously within the specified unit of work.
    /// </summary>
    /// <param name="unitOfWork">The unit of work that provides the database connection and transaction context.</param>
    /// <param name="sql">The SQL command to be executed.</param>
    /// <param name="parameters">The parameters to be used in the SQL command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the result being the number of rows affected by the execution.</returns>
    public static Task<int> ExecuteAsync(
        this PostgresUnitOfWork unitOfWork,
        string sql,
        object? parameters,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            transaction: unitOfWork.Transaction,
            cancellationToken: cancellationToken
        );

        return unitOfWork.Connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Executes a SQL query asynchronously and returns a single entity of type <typeparamref name="T"/>, or the default value if no rows are returned.
    /// </summary>
    /// <param name="unitOfWork">The unit of work that provides the database connection and transaction context.</param>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <param name="parameters">The parameters to be used in the SQL query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <typeparam name="T">The type of the entity to be returned.</typeparam>
    /// <returns>A task representing the asynchronous operation, with the result being an entity of type <typeparamref name="T"/> or <see langword="null"/> if no rows are returned.</returns>
    public static Task<T?> QuerySingleOrDefaultAsync<T>(
        this PostgresUnitOfWork unitOfWork,
        string sql,
        object? parameters,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            transaction: unitOfWork.Transaction,
            cancellationToken: cancellationToken
        );

        return unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(command);
    }

    /// <summary>
    /// Executes a SQL query that returns a single result asynchronously within the specified unit of work.
    /// </summary>
    /// <param name="unitOfWork">The unit of work that provides the database connection and transaction context.</param>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <param name="parameters">The parameters to be used in the SQL query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <typeparam name="T">The type of the result being queried.</typeparam>
    /// <returns>A task representing the asynchronous operation, with the result being the single item returned by the SQL query.</returns>
    public static Task<T> QuerySingleAsync<T>(
        this PostgresUnitOfWork unitOfWork,
        string sql,
        object? parameters,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            transaction: unitOfWork.Transaction,
            cancellationToken: cancellationToken
        );
        
        return unitOfWork.Connection.QuerySingleAsync<T>(command);
    }

    /// <summary>
    /// Executes a SQL query asynchronously using the specified unit of work and retrieves a list of dynamic objects as the result.
    /// </summary>
    /// <param name="unitOfWork">The unit of work that provides the database connection and transaction context.</param>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <param name="parameters">The parameters to be used in the SQL query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the result being a list of dynamic objects returned by the query.</returns>
    public static Task<List<dynamic>> QueryListAsync(
        this PostgresUnitOfWork unitOfWork,
        string sql,
        object? parameters,
        CancellationToken cancellationToken = default)
        => unitOfWork.QueryListAsync<dynamic>(sql, parameters, cancellationToken);
    
    /// <summary>
    /// Executes a SQL query asynchronously and retrieves the results as a list of strongly-typed objects.
    /// </summary>
    /// <param name="unitOfWork">The unit of work that provides the database connection and transaction context.</param>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <param name="parameters">The parameters to be used in the SQL query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <typeparam name="T">The type of the objects to be retrieved from the query results.</typeparam>
    /// <returns>A task representing the asynchronous operation, with the result being a list of objects of the specified type.</returns>
    public static async Task<List<T>> QueryListAsync<T>(
        this PostgresUnitOfWork unitOfWork,
        string sql,
        object? parameters,
        CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            transaction: unitOfWork.Transaction,
            cancellationToken: cancellationToken
        );

        var results = await unitOfWork.Connection.QueryAsync<T>(command);
        return results.ToList();
    }
}
