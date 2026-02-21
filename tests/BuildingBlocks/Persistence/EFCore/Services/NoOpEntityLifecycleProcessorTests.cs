using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VK.Blocks.Persistence.EFCore.Repositories;
using VK.Blocks.Persistence.EFCore.Services;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Services;

public class NoOpEntityLifecycleProcessorTests
{
    private readonly NoOpEntityLifecycleProcessor _sut;

    public NoOpEntityLifecycleProcessorTests()
    {
        _sut = new NoOpEntityLifecycleProcessor();
    }

    public class TestEntity { public int Id { get; set; } }

    [Fact]
    public void AllMethods_CanBeCalledWithoutError()
    {
        // Arrange
        var contextMock = new Mock<DbContext>();
        var setterMock = new Mock<EfCorePropertySetter<TestEntity>>();

        // Act & Assert
        Action act1 = () => _sut.ProcessAuditing(contextMock.Object);
        Action act2 = () => _sut.ProcessSoftDelete(contextMock.Object);
        Action act3 = () => _sut.ProcessBulkUpdate(setterMock.Object);
        Action act4 = () => _sut.ProcessBulkSoftDelete(setterMock.Object);

        act1.Should().NotThrow();
        act2.Should().NotThrow();
        act3.Should().NotThrow();
        act4.Should().NotThrow();
    }
}
