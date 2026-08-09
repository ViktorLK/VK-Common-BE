using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressText.Internal;

internal sealed class DefaultEgressTextFormatter : IVKEgressTextFormatter
{
    private readonly VKEgressTextOptions _options;

    public DefaultEgressTextFormatter(IOptionsSnapshot<VKEgressTextOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value);
    }

    public Task<VKResult<string>> FormatOutputAsync(string text, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(text);
        cancellationToken.ThrowIfCancellationRequested();

        var processed = text;

        if (_options.TrimWhitespace)
        {
            processed = processed.Trim();
        }

        return Task.FromResult(VKResult.Success(processed));
    }
}
