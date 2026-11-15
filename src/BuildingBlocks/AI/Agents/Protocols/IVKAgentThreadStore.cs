using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Port abstraction for persisting and retrieving agent execution thread/session state.
/// Adheres to AP.07 (non-intrusive capability; host/app owns persistence lifecycle) and CS.01 (VKResult return).
/// </summary>
public interface IVKAgentThreadStore
{
    /// <summary>
    /// Retrieves stored state variables for a given thread/session identifier.
    /// </summary>
    /// <param name="threadId">The unique identifier of the agent thread/session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary of stored variables, or null if thread is not found.</returns>
    Task<VKResult<IReadOnlyDictionary<string, object>?>> GetThreadStateAsync(
        string threadId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates the state variables for a given thread/session identifier.
    /// </summary>
    /// <param name="threadId">The unique identifier of the agent thread/session.</param>
    /// <param name="state">The state variables to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> SaveThreadStateAsync(
        string threadId,
        IReadOnlyDictionary<string, object> state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the state associated with the given thread identifier.
    /// </summary>
    /// <param name="threadId">The unique identifier of the agent thread/session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> DeleteThreadStateAsync(
        string threadId,
        CancellationToken cancellationToken = default);
}
