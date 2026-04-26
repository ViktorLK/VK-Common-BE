using System;

namespace VK.Blocks.Authentication.UnitTests.Common;

/// <summary>
/// A simple implementation of <see cref="TimeProvider"/> for testing purposes.
/// </summary>
public sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public FakeTimeProvider() : this(DateTimeOffset.UtcNow)
    {
    }

    public FakeTimeProvider(DateTimeOffset initialTime)
    {
        _now = initialTime;
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public void SetUtcNow(DateTimeOffset now) => _now = now;

    public void Advance(TimeSpan span) => _now = _now.Add(span);
}
