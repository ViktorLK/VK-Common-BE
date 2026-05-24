using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI Privacy Filter (PII detection and masking).
/// </summary>
public interface IVKPrivacyFilter
{
    /// <summary>
    /// Scans the text for PII and returns a masked version.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the masked text and identified PII entities.</returns>
    Task<VKResult<VKPrivacyMaskingResult>> MaskAsync(string text, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a privacy masking operation.
/// </summary>
public record VKPrivacyMaskingResult(string MaskedText, IReadOnlyList<VKPIIEntity> Entities);

/// <summary>
/// Represents an identified PII entity.
/// </summary>
public record VKPIIEntity(string EntityType, string OriginalValue, int StartIndex, int EndIndex);
