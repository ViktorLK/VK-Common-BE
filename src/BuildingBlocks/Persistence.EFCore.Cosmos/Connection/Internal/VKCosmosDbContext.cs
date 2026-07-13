using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Cosmos;

namespace VK.Blocks.Persistence.Cosmos.Connection.Internal;

/// <summary>
/// Default DbContext implementation for the Cosmos provider.
/// </summary>
internal sealed class VKCosmosDbContext : DbContext
{
    private readonly VKPersistenceCosmosOptions _options;

    public VKCosmosDbContext(
        DbContextOptions<VKCosmosDbContext> dbContextOptions,
        VKPersistenceCosmosOptions options)
        : base(dbContextOptions)
    {
        _options = VKGuard.NotNull(options); // [AP.01]
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // [AP.01]

        if (!string.IsNullOrWhiteSpace(_options.DatabaseName))
        {
            modelBuilder.HasDefaultContainer(_options.DatabaseName);
        }
    }
}

