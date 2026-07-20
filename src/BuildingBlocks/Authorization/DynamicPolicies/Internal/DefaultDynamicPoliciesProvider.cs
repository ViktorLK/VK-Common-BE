using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Default implementation of <see cref="IVKDynamicPoliciesProvider"/> that reads from user claims, resource properties, and environmental providers.
/// </summary>
internal sealed class DefaultDynamicPoliciesProvider(
    TimeProvider timeProvider,
    IVKIpAddressProvider? ipAddressProvider = null) : IVKDynamicPoliciesProvider
{
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);

    /// <inheritdoc />
    public ValueTask<VKResult<string?>> GetAttributeValueAsync(
        ClaimsPrincipal user,
        string attributeName,
        CancellationToken cancellationToken = default)
    {
        return GetAttributeValueAsync(user, attributeName, null, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<VKResult<string?>> GetAttributeValueAsync(
        ClaimsPrincipal user,
        string attributeName,
        object? resource,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(user);
        VKGuard.NotNullOrWhiteSpace(attributeName);

        if (attributeName.StartsWith("User.", StringComparison.OrdinalIgnoreCase))
        {
            var claimType = attributeName.Substring(5);
            var value = user.FindFirst(claimType)?.Value;
            return ValueTask.FromResult(value is not null
                ? VKResult.Success<string?>(value)
                : VKResult.Failure<string?>(VKAuthorizationErrors.AttributeNotFound));
        }

        if (attributeName.StartsWith("Resource.", StringComparison.OrdinalIgnoreCase))
        {
            if (resource is null)
            {
                return ValueTask.FromResult(VKResult.Failure<string?>(VKAuthorizationErrors.AttributeNotFound));
            }

            var propertyName = attributeName.Substring(9);
            try
            {
                var prop = resource.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    var val = prop.GetValue(resource)?.ToString();
                    return ValueTask.FromResult(VKResult.Success<string?>(val));
                }

                var field = resource.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field is not null)
                {
                    var val = field.GetValue(resource)?.ToString();
                    return ValueTask.FromResult(VKResult.Success<string?>(val));
                }
            }
            catch
            {
                // Fall through to failure
            }

            return ValueTask.FromResult(VKResult.Failure<string?>(VKAuthorizationErrors.AttributeNotFound));
        }

        if (string.Equals(attributeName, "Env.Time", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult(VKResult.Success<string?>(_timeProvider.GetUtcNow().ToString("o")));
        }

        if (string.Equals(attributeName, "Env.IpAddress", StringComparison.OrdinalIgnoreCase))
        {
            if (ipAddressProvider is null)
            {
                return ValueTask.FromResult(VKResult.Failure<string?>(VKAuthorizationErrors.AttributeNotFound));
            }
            var ip = ipAddressProvider.GetRemoteIpAddress();
            return ValueTask.FromResult(ip is not null
                ? VKResult.Success<string?>(ip.ToString())
                : VKResult.Failure<string?>(VKAuthorizationErrors.AttributeNotFound));
        }

        // Fallback: Check as raw user claim
        var rawClaimValue = user.FindFirst(attributeName)?.Value;
        return rawClaimValue is null
            ? ValueTask.FromResult(VKResult.Failure<string?>(VKAuthorizationErrors.AttributeNotFound))
            : ValueTask.FromResult(VKResult.Success<string?>(rawClaimValue));
    }
}
