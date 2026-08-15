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
    /// Gets current default or global pattern entries.
    /// </summary>
    Task<VKResult<IEnumerable<VKPatternEntry>>> GetPatternsAsync(CancellationToken cancellationToken = default);
}
