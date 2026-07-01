using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Interface for PII data masking (redaction) and reconstruction (unmasking).
/// </summary>
public interface IVKAIPIIMasker
{
    /// <summary>
    /// Masks PII data from the text, replacing sensitive information with placeholders.
    /// </summary>
    ValueTask<VKResult<string>> MaskAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores masked placeholders back to their original values in the output text.
    /// </summary>
    ValueTask<VKResult<string>> UnmaskAsync(
        string maskedText,
        CancellationToken cancellationToken = default);
}
