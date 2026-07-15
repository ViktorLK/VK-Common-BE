using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Connection.Internal;

/// <summary>
/// Default DbContext implementation for the Cosmos provider.
/// </summary>
internal sealed class VKCosmosDbContext : DbContext
{
    private readonly VKPersistenceEFCoreCosmosOptions _options;

    public VKCosmosDbContext(
        DbContextOptions<VKCosmosDbContext> dbContextOptions,
        VKPersistenceEFCoreCosmosOptions options)
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
