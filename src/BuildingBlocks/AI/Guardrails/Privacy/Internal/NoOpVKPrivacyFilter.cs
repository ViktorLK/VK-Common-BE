using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Privacy.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKPrivacyFilter"/>.
/// Returns the original text unmasked.
/// </summary>
internal sealed class NoOpVKPrivacyFilter : IVKPrivacyFilter
{
    // [SG Hook]
    public Task<VKResult<VKPrivacyMaskingResult>> MaskAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        var result = new VKPrivacyMaskingResult(
            MaskedText: text,
            Entities: new List<VKPIIEntity>());

        return Task.FromResult(VKResult.Success(result));
    }
}
