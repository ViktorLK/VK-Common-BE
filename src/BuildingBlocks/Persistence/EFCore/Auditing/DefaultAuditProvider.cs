

using VK.Blocks.Persistence.Abstractions.Auditing;

namespace VK.Blocks.Persistence.EFCore.Auditing;

/// <summary>
/// Provides a default implementation of <see cref="IAuditProvider"/> that uses system defaults.
/// </summary>
public sealed class DefaultAuditProvider(string systemIdentifier = "System") : IAuditProvider
{
    #region Properties

    /// <inheritdoc />
    public string CurrentUserId => systemIdentifier;

    /// <inheritdoc />
    public string CurrentUserName => systemIdentifier;

    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public bool IsAuthenticated => false;

    #endregion
}
