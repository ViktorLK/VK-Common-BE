using System.Collections.Generic;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Fixtures;

/// <summary>
/// Test implementation of <see cref="IVKTenantProvider"/> and <see cref="IVKTenantContext"/> for integration testing.
/// Follows AP.01 (sealed class default).
/// </summary>
public sealed class TestTenantProvider : IVKTenantProvider, IVKTenantContext
{
    public VKTenantId? CurrentTenantId { get; set; }

    public TestTenantProvider(VKTenantId? initialTenantId = null)
    {
        CurrentTenantId = initialTenantId;
    }

    public VKTenantId? GetCurrentTenantId() => CurrentTenantId;

    // IVKTenantContext implementation
    public VKTenantId TenantId => CurrentTenantId ?? VKTenantId.Empty;
    public bool IsResolved => CurrentTenantId.HasValue && !CurrentTenantId.Value.IsEmpty;
    public string TenantName => "IntegrationTestTenant";
    public string? Domain => null;
    public bool IsActive => true;
    public VKSensitiveString? ConnectionString => null;
    public string? Schema => null;
    public IReadOnlyDictionary<string, string> Metadata => new Dictionary<string, string>();
}
