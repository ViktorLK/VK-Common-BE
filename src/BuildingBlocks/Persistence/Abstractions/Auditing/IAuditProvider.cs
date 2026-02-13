namespace VK.Blocks.Persistence.Abstractions.Auditing;

public interface IAuditProvider
{
    string CurrentUserId { get; }

    string CurrentUserName { get; }

    DateTimeOffset UtcNow { get; }

    bool IsAuthenticated { get; }
}
