using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressText.Internal;

internal sealed class DefaultTextSplitter : IVKTextSplitter
{
    private readonly VKIngressTextOptions _options;

    public DefaultTextSplitter(IOptionsSnapshot<VKIngressTextOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value);
    }

    public Task<VKResult<IReadOnlyList<string>>> SplitTextAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (text is null)
        {
            return Task.FromResult(VKResult.Success<IReadOnlyList<string>>([]));
        }

        if (text.Length > _options.MaxInputLength)
        {
            return Task.FromResult(VKResult.Failure<IReadOnlyList<string>>(IngressTextErrors.InputTooLong));
        }

        var processedText = text;
        if (_options.EnableUnicodeNormalization)
        {
            try
            {
                var normForm = Enum.TryParse<NormalizationForm>(_options.NormalizationForm, out var form)
                    ? form
                    : NormalizationForm.FormC;
                processedText = processedText.Normalize(normForm);
            }
            catch (Exception)
            {
                return Task.FromResult(VKResult.Failure<IReadOnlyList<string>>(IngressTextErrors.NormalizationFailed));
            }
        }

        if (_options.EnableWhitespaceTrimming)
        {
            processedText = processedText.Trim();
        }

        var chunks = new List<string>();
        var paragraphs = processedText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                chunks.Add(trimmed);
            }
        }

        IReadOnlyList<string> result = chunks;
        return Task.FromResult(VKResult.Success(result));
    }
}
