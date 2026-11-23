using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Embeddings.Internal;

/// <summary>
/// Intercepts embedding generation values and masks PII before invoking the underlying generator.
/// Adheres to ARCH-12, CS.01, CS.03, and AP.01.
/// </summary>
internal sealed class PrivacyEmbeddingGenerator(
    IEmbeddingGenerator<string, Embedding<float>> innerGenerator,
    IVKPrivacyFilter privacyFilter)
    : DelegatingEmbeddingGenerator<string, Embedding<float>>(innerGenerator)
{
    private readonly IVKPrivacyFilter _privacyFilter = VKGuard.NotNull(privacyFilter);

    public override async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(values);

        var sanitized = new List<string>();
        foreach (var text in values)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(text))
            {
                sanitized.Add(text);
                continue;
            }

            var maskRes = await _privacyFilter.MaskAsync(text, cancellationToken).ConfigureAwait(false);
            if (maskRes.IsSuccess && maskRes.Value is not null)
            {
                sanitized.Add(maskRes.Value.MaskedText);
            }
            else
            {
                sanitized.Add(text);
            }
        }

        return await base.GenerateAsync(sanitized, options, cancellationToken).ConfigureAwait(false);
    }
}

