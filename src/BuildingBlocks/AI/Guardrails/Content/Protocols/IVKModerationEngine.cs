using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Provides content moderation and safety evaluation capabilities.
/// </summary>
public interface IVKModerationEngine
{
    /// <summary>
    /// Checks the given text content against safety and moderation guidelines.
    /// </summary>
    /// <param name="content">The text content to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A moderation result indicating whether the content is flagged and why.</returns>
    Task<VKResult<VKModerationResult>> CheckContentAsync(string content, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a content moderation check.
/// </summary>
public sealed record VKModerationResult
{
    /// <summary>
    /// Gets a value indicating whether the content was flagged as unsafe or inappropriate.
    /// </summary>
    public required bool IsFlagged { get; init; }

    /// <summary>
    /// Gets the reason the content was flagged, if applicable.
    /// </summary>
    public string? FlaggedReason { get; init; }
}
