

using VK.Blocks.Persistence.Abstractions.Auditing;

namespace VK.Blocks.Persistence.EFCore.Auditing;

/// <summary>
/// Provides a default implementation of <see cref="IAuditProvider"/> that uses system defaults.
/// </summary>
public sealed class DefaultAuditProvider(string systemIdentifier = "System") : IAuditProvider
{
    #region Properties

    public string CurrentUserId => systemIdentifier;

    public string CurrentUserName => systemIdentifier;

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public bool IsAuthenticated => false;

    #endregion
}
