using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.Core.Results;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Constants;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution.Resolvers;

/// <summary>
/// Resolves the tenant by extracting the subdomain from the request host
/// and looking it up via <see cref="ITenantStore"/>.
/// The domain template is configurable via <see cref="TenantResolutionOptions.DomainTemplate"/>.
/// </summary>
public sealed class DomainTenantResolver(
    IOptions<TenantResolutionOptions> options,
    ITenantStore tenantStore) : ITenantResolver
{
    #region Fields

    private readonly string _domainTemplate = options.Value.DomainTemplate;
    private readonly ITenantStore _tenantStore = tenantStore;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int Order => 300;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<Result<string>> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var host = context.Request.Host.Host;

        if (string.IsNullOrWhiteSpace(host))
        {
            return Result.Failure<string>(MultiTenancyErrors.TenantNotFound);
        }

        var tenantSegment = ExtractTenantSegment(host);

        if (string.IsNullOrWhiteSpace(tenantSegment))
        {
            return Result.Failure<string>(MultiTenancyErrors.TenantNotFound);
        }

        var tenantInfo = await _tenantStore.GetByDomainAsync(tenantSegment, cancellationToken);

        if (tenantInfo is not null)
        {
            return Result.Success(tenantInfo.Id);
        }

        return Result.Failure<string>(MultiTenancyErrors.TenantNotFound);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Extracts the tenant segment from the host using the configured domain template.
    /// For example, with template "{tenant}.yourdomain.com" and host "acme.yourdomain.com",
    /// returns "acme".
    /// </summary>
    private string? ExtractTenantSegment(string host)
    {
        // Template format: {tenant}.yourdomain.com
        // Extract the suffix after {tenant} placeholder
        const string placeholder = MultiTenancyConstants.Config.TenantPlaceholder;
        var placeholderIndex = _domainTemplate.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase);

        if (placeholderIndex < 0)
        {
            return null;
        }

        var suffix = _domainTemplate[(placeholderIndex + placeholder.Length)..];

        if (!host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var tenantSegment = host[..^suffix.Length];

        // If there's a prefix before {tenant}, remove it
        if (placeholderIndex > 0)
        {
            var prefix = _domainTemplate[..placeholderIndex];
            if (tenantSegment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                tenantSegment = tenantSegment[prefix.Length..];
            }
        }

        return string.IsNullOrWhiteSpace(tenantSegment) ? null : tenantSegment;
    }

    #endregion
}
