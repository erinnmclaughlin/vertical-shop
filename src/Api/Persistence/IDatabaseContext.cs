using System.Data.Common;
using ContextDrivenDevelopment.Api.Inventory;
using ContextDrivenDevelopment.Api.Messaging;
using ContextDrivenDevelopment.Api.Products;

namespace ContextDrivenDevelopment.Api.Persistence;

/// <summary>
/// Defines the contract for a unit of work that manages the lifecycle of a transaction.
/// </summary>
public interface IDatabaseContext : IAsyncDisposable
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
    /// Begins a new transaction for the unit of work, allowing multiple operations to be executed as a single atomic operation.
    /// </summary>
    /// <returns>The created transaction.</returns>
    DbTransaction BeginTransaction();
}
