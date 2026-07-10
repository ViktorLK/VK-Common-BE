using System;

namespace VK.Blocks.Persistence.Auditing.Internal;

/// <summary>
/// A no-op implementation of <see cref="IVKAuditProvider"/> used when auditing is disabled.
/// </summary>
internal sealed class NoOpAuditProvider(TimeProvider timeProvider) : IVKAuditProvider
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public string CurrentUserId => string.Empty;

    /// <inheritdoc />
    // [CS.06]
    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
}
