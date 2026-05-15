using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for solidifying high-frequency patterns into persona traits.
/// </summary>
public interface IVKPersonaPatternSolidifier
{
    /// <summary>
    /// Analyzes content to extract and solidify patterns into the persona's trait map.
    /// </summary>
    /// <param name="content">The content to analyze (e.g. a summary of recent memories).</param>
    /// <param name="currentPersona">The current persona card.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated persona card with solidified traits.</returns>
    Task<VKResult<VKPersonaCard>> SolidifyAsync(
        string content,
        VKPersonaCard currentPersona,
        CancellationToken cancellationToken = default);
}
