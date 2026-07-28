using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Receives external events and evaluates them against active event-triggered prospective reminders.
/// </summary>
public interface IVKReminderEventReceiver
{
    /// <summary>
    /// Handles an external event and triggers any matching reminders.
    /// </summary>
    Task<VKResult> OnEventReceivedAsync(string eventName, string? payload = null, CancellationToken cancellationToken = default);
}
