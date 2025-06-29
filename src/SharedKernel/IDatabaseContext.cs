using System.Data.Common;

namespace VerticalShop;

/// <summary>
/// Defines the contract for a unit of work that manages the lifecycle of a transaction.
/// </summary>
public interface IDatabaseContext : IDisposable
{
    /// <summary>
    /// Gets the database connection associated with the current context.
    /// </summary>
    /// <remarks>
    /// This property provides an instance of <see cref="System.Data.Common.DbConnection"/>
    /// that represents the connection to the underlying database.
    /// </remarks>
    DbConnection Connection { get; }

    /// <summary>
    /// Gets the current database transaction associated with the database context.
    /// </summary>
    /// <remarks>
    /// This property provides an instance of <see cref="System.Data.Common.DbTransaction"/> that represents
    /// the active transaction for the current unit of work, or null if no transaction is currently active.
    /// </remarks>
    DbTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Begins a new transaction for the unit of work, allowing multiple operations to be executed as a single atomic operation.
    /// </summary>
    /// <returns>The created transaction.</returns>
    Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction, finalizing all operations executed within the transaction scope.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the commit operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new outbox message into the database.
    /// </summary>
    /// <typeparam name="T">The type of the message being inserted.</typeparam>
    /// <param name="message">The message to be inserted into the outbox.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InsertOutboxMessageAsync<T>(T message, CancellationToken cancellationToken = default);
}
