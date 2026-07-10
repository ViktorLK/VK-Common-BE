using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines a pipeline hook that executes around the SaveChanges operation.
/// Implementations can dispatch domain events, publish outbox messages, etc.
/// </summary>
public interface IVKSaveChangesPipeline
{
    /// <summary>
    /// Executes before SaveChanges is called.
    /// Use this to collect domain events from tracked entities.
    /// </summary>
    // [CS.03]
    Task BeforeSaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes after SaveChanges succeeds.
    /// Use this to dispatch domain events or commit outbox messages.
    /// </summary>
    // [CS.03]
    Task AfterSaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes when SaveChanges fails.
    /// Use this to handle compensating actions or logging.
    /// </summary>
    // [CS.03]
    Task OnSaveFailedAsync(Exception exception, CancellationToken cancellationToken = default);
}
