using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Federation.Protocols;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Federation.Internal;

/// <summary>
/// A thread-safe, in-memory implementation of <see cref="IVKAccountLinkService"/> for local fallbacks.
/// </summary>
internal sealed class InMemoryAccountLinkService(IOptions<VKFederationOptions> options) : IVKAccountLinkService
{
    private readonly VKFederationOptions _options = VKGuard.NotNull(options).Value;
    private readonly ConcurrentDictionary<(string, string), VKAccountLinkInfo> _externalMappings = new();

    /// <inheritdoc />
    public ValueTask<VKResult<string>> FindLinkedUserIdAsync(string loginProvider, string providerKey, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(loginProvider);
        VKGuard.NotNullOrWhiteSpace(providerKey);

        var key = (loginProvider.ToLowerInvariant(), providerKey);
        if (_externalMappings.TryGetValue(key, out var info))
        {
            return ValueTask.FromResult(VKResult.Success(info.UserId));
        }

        return ValueTask.FromResult(VKResult.Failure<string>(new VKError(
            "Federation.AccountNotLinked",
            "The external identity is not linked to any local account.",
            VKErrorType.NotFound)));
    }

    /// <inheritdoc />
    public ValueTask<VKResult> LinkAccountAsync(
        string userId,
        string loginProvider,
        string providerKey,
        string providerDisplayName,
        CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(userId);
        VKGuard.NotNullOrWhiteSpace(loginProvider);
        VKGuard.NotNullOrWhiteSpace(providerKey);

        string providerLower = loginProvider.ToLowerInvariant();
        var key = (providerLower, providerKey);

        if (!_options.AllowMultipleLinksPerExternalAccount)
        {
            if (_externalMappings.TryGetValue(key, out var existing))
            {
                if (existing.UserId != userId)
                {
                    return ValueTask.FromResult(VKResult.Failure(new VKError(
                        "Federation.AccountAlreadyLinked",
                        "This external account is already linked to another local user.",
                        VKErrorType.Conflict)));
                }
                return ValueTask.FromResult(VKResult.Success()); // Idempotent mapping success
            }
        }

        var linkInfo = new VKAccountLinkInfo
        {
            UserId = userId,
            LoginProvider = loginProvider,
            ProviderKey = providerKey,
            ProviderDisplayName = providerDisplayName,
            LinkedAt = DateTimeOffset.UtcNow
        };

        _externalMappings.TryAdd(key, linkInfo);
        return ValueTask.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public ValueTask<VKResult> UnlinkAccountAsync(string userId, string loginProvider, string providerKey, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(userId);
        VKGuard.NotNullOrWhiteSpace(loginProvider);
        VKGuard.NotNullOrWhiteSpace(providerKey);

        var key = (loginProvider.ToLowerInvariant(), providerKey);
        if (_externalMappings.TryGetValue(key, out var existing))
        {
            if (existing.UserId == userId)
            {
                _externalMappings.TryRemove(key, out _);
                return ValueTask.FromResult(VKResult.Success());
            }
        }

        return ValueTask.FromResult(VKResult.Failure(new VKError(
            "Federation.LinkNotFound",
            "Specified account linking mapping was not found.",
            VKErrorType.NotFound)));
    }

    /// <inheritdoc />
    public ValueTask<VKResult<IReadOnlyList<VKAccountLinkInfo>>> GetLinkedAccountsAsync(string userId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(userId);

        IReadOnlyList<VKAccountLinkInfo> list = _externalMappings.Values
            .Where(x => x.UserId == userId)
            .ToList();

        return ValueTask.FromResult(VKResult.Success(list));
    }
}
