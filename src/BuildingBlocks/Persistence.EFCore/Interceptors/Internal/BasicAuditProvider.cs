using System;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Interceptors.Internal;

/// <summary>
/// Provides a default implementation of <see cref="IVKAuditProvider"/> that uses system defaults and <see cref="TimeProvider"/>.
/// </summary>
internal sealed class BasicAuditProvider(TimeProvider timeProvider, string systemIdentifier = "System") : IVKAuditProvider
{
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);

    /// <inheritdoc />
    public string CurrentUserId => systemIdentifier;

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
}
