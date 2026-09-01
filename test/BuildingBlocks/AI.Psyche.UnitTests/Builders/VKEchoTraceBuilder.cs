using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKEchoTrace"/> objects in unit tests.
/// </summary>
public sealed class VKEchoTraceBuilder : VKTestDataBuilder<VKEchoTrace>
{
    private VKEchoId _id = new(Guid.NewGuid());
    private VKSessionId _sessionId = new(Guid.NewGuid());
    private VKChatRole _role = VKChatRole.User;
    private string _content = "Default test message";
    private int _tokenCount;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

    public VKEchoTraceBuilder WithId(VKEchoId id)
    {
        _id = id;
        return this;
    }

    public VKEchoTraceBuilder WithSessionId(VKSessionId sessionId)
    {
        _sessionId = sessionId;
        return this;
    }

    public VKEchoTraceBuilder WithRole(VKChatRole role)
    {
        _role = role;
        return this;
    }

    public VKEchoTraceBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public VKEchoTraceBuilder WithTokenCount(int tokenCount)
    {
        _tokenCount = tokenCount;
        return this;
    }

    public VKEchoTraceBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    protected override VKEchoTrace CreateDefault()
    {
        return new VKEchoTrace
        {
            Id = _id,
            SessionId = _sessionId,
            Role = _role,
            Content = _content,
            TokenCount = _tokenCount,
            CreatedAt = _createdAt
        };
    }
}
