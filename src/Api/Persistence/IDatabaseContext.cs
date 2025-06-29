using System.Data.Common;

namespace ContextDrivenDevelopment.Api.Persistence;

/// <summary>
/// Defines the contract for a unit of work that manages the lifecycle of a transaction.
/// </summary>
public interface IDatabaseContext : IDisposable
{
    /// <summary>
    /// Begins a new transaction for the unit of work, allowing multiple operations to be executed as a single atomic operation.
    /// </summary>
    /// <returns>The created transaction.</returns>
    DbTransaction BeginTransaction();
}
