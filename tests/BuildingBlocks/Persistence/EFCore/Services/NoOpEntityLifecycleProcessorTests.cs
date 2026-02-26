using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VK.Blocks.Persistence.EFCore.Repositories;
using VK.Blocks.Persistence.EFCore.Services;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Services;

/// <summary>
/// Unit tests for <see cref="NoOpEntityLifecycleProcessor"/>.
/// </summary>
public class NoOpEntityLifecycleProcessorTests
{
    private readonly NoOpEntityLifecycleProcessor _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoOpEntityLifecycleProcessorTests"/> class.
    /// </summary>
    public NoOpEntityLifecycleProcessorTests()
    {
        _sut = new NoOpEntityLifecycleProcessor();
    }

    /// <summary>
    /// A test entity for verification.
    /// </summary>
    public class TestEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int Id { get; set; }
    }
#if NET8_0
    /// <summary>
    /// Verifies that all methods in <see cref="NoOpEntityLifecycleProcessor"/> can be called without errors, as they are no-ops.
    /// </summary>
    [Fact]
    public void AllMethods_CanBeCalledWithoutError()
    {
        // Arrange
        var contextMock = new Mock<DbContext>();
        var setterMock = new Mock<EfCorePropertySetter<TestEntity>>();

        // Act & Assert
        // Rationale: Ensure that calling any of the processor methods does not result in an exception.
        Action act1 = () => _sut.ProcessAuditing(contextMock.Object);
        Action act2 = () => _sut.ProcessSoftDelete(contextMock.Object);
        Action act3 = () => _sut.ProcessBulkUpdate(setterMock.Object);
        Action act4 = () => _sut.ProcessBulkSoftDelete(setterMock.Object);

        act1.Should().NotThrow();
        act2.Should().NotThrow();
        act3.Should().NotThrow();
        act4.Should().NotThrow();
    }
#endif
}
