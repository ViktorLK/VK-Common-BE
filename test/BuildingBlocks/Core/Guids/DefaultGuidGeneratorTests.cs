using VK.Blocks.Core.Guids.Internal;

namespace VK.Blocks.Core.UnitTests.Guids;

public sealed class DefaultGuidGeneratorTests
{
    private readonly DefaultGuidGenerator _sut = new();

    [Fact]
    public void Create_ShouldReturnNewGuid()
    {
        // Act
        var guid1 = _sut.Create();
        var guid2 = _sut.Create();

        // Assert
        guid1.Should().NotBe(Guid.Empty);
        guid2.Should().NotBe(Guid.Empty);
        guid1.Should().NotBe(guid2);
    }
}
