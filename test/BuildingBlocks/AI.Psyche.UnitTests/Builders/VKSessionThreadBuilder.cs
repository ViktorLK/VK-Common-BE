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
    private VKEchoId? _forkPointEchoId;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private int _baseTurnOffset;
    private bool _isSandbox;

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

    public VKSessionThreadBuilder WithForkSource(VKSessionId forkSourceSessionId, VKEchoId forkPointEchoId)
    {
        _forkSourceSessionId = forkSourceSessionId;
        _forkPointEchoId = forkPointEchoId;
        return this;
    }

    public VKSessionThreadBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public VKSessionThreadBuilder WithBaseTurnOffset(int baseTurnOffset)
    {
        _baseTurnOffset = baseTurnOffset;
        return this;
    }

    public VKSessionThreadBuilder WithIsSandbox(bool isSandbox = true)
    {
        _isSandbox = isSandbox;
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
            forkPointEchoId: _forkPointEchoId,
            baseTurnOffset: _baseTurnOffset,
            isSandbox: _isSandbox).Value);
    }
}
