using VK.Blocks.Core;
using VK.Blocks.Testing.Builders;

namespace VK.Blocks.AI.Psyche.UnitTests.Builders;

/// <summary>
/// Builder for constructing <see cref="VKSessionThread"/> objects in unit tests.
/// </summary>
public sealed class VKSessionThreadBuilder : VKTestDataBuilder<VKSessionThread>
{
    private VKSessionId _id = new(Guid.NewGuid());
    private VKSessionMode _mode = VKSessionMode.Isolated;
    private VKSessionId? _parentSessionId;
    private VKSessionId? _forkSourceSessionId;
    private string? _forkPointRef;
    private VKSessionKnowledgeState? _knowledgeState;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

    public VKSessionThreadBuilder WithId(VKSessionId id)
    {
        _id = id;
        return this;
    }

    public VKSessionThreadBuilder WithMode(VKSessionMode mode)
    {
        _mode = mode;
        return this;
    }

    public VKSessionThreadBuilder WithParentSessionId(VKSessionId? parentSessionId)
    {
        _parentSessionId = parentSessionId;
        return this;
    }

    public VKSessionThreadBuilder WithForkSource(VKSessionId forkSourceSessionId, string forkPointRef)
    {
        _forkSourceSessionId = forkSourceSessionId;
        _forkPointRef = forkPointRef;
        return this;
    }

    public VKSessionThreadBuilder WithKnowledgeState(VKSessionKnowledgeState knowledgeState)
    {
        _knowledgeState = knowledgeState;
        return this;
    }

    public VKSessionThreadBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    protected override VKSessionThread CreateDefault()
    {
        return VKGuard.NotNull(VKSessionThread.Create(
            id: _id,
            now: _createdAt,
            mode: _mode,
            parentSessionId: _parentSessionId,
            forkSourceSessionId: _forkSourceSessionId,
            forkPointRef: _forkPointRef,
            knowledgeState: _knowledgeState).Value);
    }
}
