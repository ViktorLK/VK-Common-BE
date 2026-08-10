using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

/// <summary>
/// Defines contract for calculating human-like pause delays and text pacing for narrative segment arrays.
/// </summary>
public interface IVKEgressPacer
{
    /// <summary>
    /// Calculates text chunks and human pause delays for a narrative segment array.
    /// </summary>
    /// <param name="segments">The narrative text segments to pace.</param>
    /// <param name="overrideOptions">Optional egress text options override.</param>
    /// <returns>A result containing the list of paced text chunks.</returns>
    VKResult<IReadOnlyList<VKEgressPacingChunk>> CalculatePacing(IReadOnlyList<string> segments, VKEgressTextOptions? overrideOptions = null);
}
