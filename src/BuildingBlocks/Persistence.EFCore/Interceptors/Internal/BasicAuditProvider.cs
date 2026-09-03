using System;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Interceptors.Internal;

/// <summary>
/// Provides a default implementation of <see cref="IVKAuditProvider"/> that uses system defaults and <see cref="TimeProvider"/>.
/// </summary>
internal sealed class BasicAuditProvider(TimeProvider timeProvider, VKUserId? systemIdentifier = null) : IVKAuditProvider
{
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);
    private readonly VKUserId? _systemIdentifier = systemIdentifier ?? VKUserId.System;

    /// <inheritdoc />
    public VKUserId? CurrentUserId => _systemIdentifier;

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
}
