namespace VK.Blocks.Persistence.Abstractions.Auditing;

public sealed class DefaultAuditProvider(string systemIdentifier = "System") : IAuditProvider
{
    public string CurrentUserId => systemIdentifier;

    public string CurrentUserName => systemIdentifier;

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public bool IsAuthenticated => false;
}
