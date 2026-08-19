using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the store operations for retrieving custom prompt patterns.
/// </summary>
public interface IVKPatternStore
{
    /// <summary>
    /// Gets pattern entries matching the specified pattern IDs (or all patterns if patternIds is empty).
    /// </summary>
    Task<VKResult<IReadOnlyList<VKPatternEntry>>> GetPatternsAsync(
        IReadOnlyList<VKPatternId> patternIds,
        CancellationToken cancellationToken = default);
}
