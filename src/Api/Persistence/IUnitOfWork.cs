using ContextDrivenDevelopment.Api.Domain.Inventory;
using ContextDrivenDevelopment.Api.Domain.Products;
using ContextDrivenDevelopment.Api.Messaging;

namespace ContextDrivenDevelopment.Api.Persistence;

/// <summary>
/// Defines the contract for a unit of work that manages the lifecycle of a transaction.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Provides access to operations related to message processing and retrieval in the outbox pattern.
    /// </summary>
    IOutbox Outbox { get; }

    /// <summary>
    /// Provides access to operations for managing product entities in a data store.
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Provides access to operations for managing inventory entities in a data store.
    /// </summary>
    IInventoryRepository Inventory { get; }

    /// <summary>
    /// Commits all changes made within the unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the commit operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous commit operation.
    /// </returns>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
