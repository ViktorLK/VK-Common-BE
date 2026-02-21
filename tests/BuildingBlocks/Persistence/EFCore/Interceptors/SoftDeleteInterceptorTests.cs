using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Persistence.EFCore.Services;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Interceptors;

public class SoftDeleteInterceptorTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEntityLifecycleProcessor> _processorMock;
    private readonly SoftDeleteInterceptor _sut;

    public SoftDeleteInterceptorTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _processorMock = _fixture.Freeze<Mock<IEntityLifecycleProcessor>>();
        _sut = new SoftDeleteInterceptor(_processorMock.Object);
    }

    [Fact]
    public void SavingChanges_ValidContext_CallsProcessSoftDelete()
    {
        // Arrange
        var contextMock = new Mock<DbContext>();
        var eventData = new DbContextEventData(
            null!,
            null!,
            contextMock.Object);

        // Act
        _sut.SavingChanges(eventData, new InterceptionResult<int>());

        // Assert
        _processorMock.Verify(x => x.ProcessSoftDelete(contextMock.Object), Times.Once);
    }

    [Fact]
    public void SavingChanges_NullContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventData = new DbContextEventData(
            null!,
            null!,
            null);

        // Act
        Action act = () => _sut.SavingChanges(eventData, new InterceptionResult<int>());

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Context is null*");
    }

    [Fact]
    public async Task SavingChangesAsync_ValidContext_CallsProcessSoftDelete()
    {
        // Arrange
        var contextMock = new Mock<DbContext>();
        var eventData = new DbContextEventData(
            null!,
            null!,
            contextMock.Object);

        // Act
        await _sut.SavingChangesAsync(eventData, new InterceptionResult<int>());

        // Assert
        _processorMock.Verify(x => x.ProcessSoftDelete(contextMock.Object), Times.Once);
    }

    [Fact]
    public async Task SavingChangesAsync_NullContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventData = new DbContextEventData(
            null!,
            null!,
            null);

        // Act
        Func<Task> act = async () => await _sut.SavingChangesAsync(eventData, new InterceptionResult<int>());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("*Context is null*");
    }
}
