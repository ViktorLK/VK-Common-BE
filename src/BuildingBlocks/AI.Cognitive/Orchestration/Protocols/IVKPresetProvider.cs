using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Provides static task-specific presets (bottom notes or AuthorsNote) to inject into the cognitive prompt tapestry.
/// </summary>
public interface IVKPresetProvider
{
    /// <summary>
    /// Retrieves a preset's instructions.
    /// </summary>
    /// <param name="presetId">The preset identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the preset string, or failure.</returns>
    Task<VKResult<string>> GetPresetAsync(string presetId, CancellationToken cancellationToken = default); // [CS.03]
}
