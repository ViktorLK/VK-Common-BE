using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Security.Internal;

// [AP.01] sealed
internal sealed class ConfigurationAICredentialResolver : IVKAICredentialResolver
{
    private readonly IConfiguration _configuration;

    public ConfigurationAICredentialResolver(IConfiguration configuration)
    {
        _configuration = VKGuard.NotNull(configuration);
    }

    public ValueTask<VKResult<VKSensitiveString>> ResolveAsync(
        VKAIScopeKey scopeKey,
        string credentialRef,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = scopeKey;

        if (string.IsNullOrWhiteSpace(credentialRef))
        {
            return ValueTask.FromResult(VKResult.Failure<VKSensitiveString>(VKAISynapseErrors.UnauthorizedConnection));
        }

        string? secret = _configuration[credentialRef] ?? _configuration[$"AI:Credentials:{credentialRef}"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            return ValueTask.FromResult(VKResult.Failure<VKSensitiveString>(VKAISynapseErrors.UnauthorizedConnection));
        }

        return ValueTask.FromResult(VKResult.Success(new VKSensitiveString(secret)));
    }
}
