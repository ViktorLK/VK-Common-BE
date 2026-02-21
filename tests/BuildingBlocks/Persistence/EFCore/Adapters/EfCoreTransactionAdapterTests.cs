using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using VK.Blocks.Persistence.EFCore.Adapters;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Adapters;

public class EfCoreTransactionAdapterTests
{
    private readonly IFixture _fixture;

    public EfCoreTransactionAdapterTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Fact]
    public async Task EfCoreTransactionAdapter_DelegatesToUnderlyingTransaction()
    {
        // Arrange
        var mockTransaction = _fixture.Freeze<Mock<IDbContextTransaction>>();
        var mockId = Guid.NewGuid();
        mockTransaction.Setup(x => x.TransactionId).Returns(mockId);

        var adapter = new EfCoreTransactionAdapter(mockTransaction.Object);

        // Act & Assert - Id
        adapter.TransactionId.Should().Be(mockId);

        // Act & Assert - Commit
        await adapter.CommitAsync();
        mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Act & Assert - Rollback
        await adapter.RollbackAsync();
        mockTransaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Act & Assert - Dispose
        adapter.Dispose();
        mockTransaction.Verify(x => x.Dispose(), Times.Once);

        // Act & Assert - DisposeAsync
        await adapter.DisposeAsync();
        mockTransaction.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
