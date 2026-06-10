using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the contract for splitting text inputs into chunks.
/// Complies with CS.01, CS.03, and AP.01.
/// </summary>
public interface IVKTextSplitter
{
    /// <summary>
    /// Splits the provided text input into a list of chunks.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of text chunks.</returns>
    Task<VKResult<IReadOnlyList<string>>> SplitTextAsync(string text, CancellationToken cancellationToken = default);
}
