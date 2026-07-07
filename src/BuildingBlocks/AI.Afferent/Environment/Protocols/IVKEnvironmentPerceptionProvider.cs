using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Represents the captured state of the user's environment.
/// </summary>
public record VKEnvironmentState(string ActiveWindowTitle, string ScreenOcrText, string ClipboardContent);

/// <summary>
/// Defines the contract for an Environment Perception Provider.
/// </summary>
public interface IVKEnvironmentPerceptionProvider
{
    /// <summary>
    /// Captures the current environment state (e.g., screen text, active window).
    /// </summary>
    Task<VKResult<VKEnvironmentState>> GetEnvironmentStateAsync(CancellationToken cancellationToken = default);
}
