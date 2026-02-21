using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Core.Results;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Extensions;

// Define Context and Entity here as they are specific to this test
public class ConcurrencyEntity : IConcurrency
{
    public int Id { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public class ConcurrencyContext : BaseDbContext
{
    public ConcurrencyContext(DbContextOptions options) : base(options) { }

    public DbSet<ConcurrencyEntity> ConcurrencyEntities { get; set; } = null!;
}

public class ConcurrencyTests : IntegrationTestBase<ConcurrencyContext>
{
    [Fact]
    public void ApplyConcurrencyToken_SetsRowVersion()
    {
        // Act
        // Context is already created and EnsureCreatedAsync called by base class
        var configuredEntity = Context.Model.FindEntityType(typeof(ConcurrencyEntity));
        var property = configuredEntity!.FindProperty(nameof(IConcurrency.RowVersion));

        // Assert
        property.Should().NotBeNull();
        property!.IsConcurrencyToken.Should().BeTrue();
    }
}
