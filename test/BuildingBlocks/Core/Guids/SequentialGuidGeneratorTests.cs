using VK.Blocks.Core.Guids.Internal;
using VK.Blocks.Core.UnitTests.Utilities;

namespace VK.Blocks.Core.UnitTests.Guids;

public sealed class SequentialGuidGeneratorTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly SequentialGuidGenerator _sut;

    public SequentialGuidGeneratorTests()
    {
        _sut = new SequentialGuidGenerator(_timeProvider);
    }

    [Fact]
    public void Create_GeneratesNonEmptyGuid()
    {
        // Act
        var guid = _sut.Create();

        // Assert
        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_GeneratesUniqueGuids()
    {
        // Act
        var guid1 = _sut.Create();
        var guid2 = _sut.Create();

        // Assert
        guid1.Should().NotBe(guid2);
    }

    [Fact]
    public void Create_IsSequentialInLastSixBytes()
    {
        // Arrange
        var baseTime = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(baseTime);

        // Act
        var guid1 = _sut.Create();

        // Move time forward 1 second
        _timeProvider.SetUtcNow(baseTime.AddSeconds(1));
        var guid2 = _sut.Create();

        // Assert
        var bytes1 = guid1.ToByteArray();
        var bytes2 = guid2.ToByteArray();

        // Last 6 bytes should represent the timestamp (Big-Endian in Comb GUID)
        // Extract last 6 bytes (from index 10 to 15)
        var span1 = bytes1.AsSpan(10);
        var span2 = bytes2.AsSpan(10);

        // Convert the last 6 bytes to a long (Big-Endian interpretation)
        long GetVal(ReadOnlySpan<byte> s)
        {
            long val = 0;
            for (int i = 0; i < 6; i++)
                val = (val << 8) | s[i];
            return val;
        }

        var val1 = GetVal(span1);
        var val2 = GetVal(span2);

        val2.Should().BeGreaterThan(val1, "GUIDs should be sequential in the last 6 bytes over time");
    }

    [Fact]
    public void Create_AtSameTime_ProducesDifferentGuids()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(now);

        // Act
        var guid1 = _sut.Create();
        var guid2 = _sut.Create();

        // Assert
        guid1.Should().NotBe(guid2);
        // But the last 6 bytes should be IDENTICAL because they share the same timestamp (ms resolution)
        var bytes1 = guid1.ToByteArray();
        var bytes2 = guid2.ToByteArray();
        bytes1.AsSpan(10).SequenceEqual(bytes2.AsSpan(10)).Should().BeTrue();
    }
}
