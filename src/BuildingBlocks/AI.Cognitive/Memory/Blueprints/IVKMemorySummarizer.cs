using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for summarizing memory context.
/// </summary>
public interface IVKMemorySummarizer
{
    /// <summary>
    /// Summarizes the given content.
    /// </summary>
    /// <param name="content">The content to summarize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary result.</returns>
    Task<VKResult<string>> SummarizeAsync(string content, CancellationToken cancellationToken = default);
}
