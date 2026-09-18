using Microsoft.EntityFrameworkCore;
using VK.Blocks.AI.Psyche.EFCore;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.EFCore;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Fixtures;

/// <summary>
/// Integration DbContext configured with Psyche EFCore entities, model contributors,
/// and SQLite compatibility conventions. Supports dynamic tenant resolution.
/// Follows AP.01 (sealed class default) and CS.08.
/// </summary>
public sealed class PsycheIntegrationDbContext : VKBaseDbContext
{
    private static readonly VKBlocksAIPsycheEFCoreModelContributor s_contributor = new();
    private static readonly VKMultiTenantQueryFilterContributor s_tenantFilter = new();

    public PsycheIntegrationDbContext(
        DbContextOptions<PsycheIntegrationDbContext> options,
        IVKTenantProvider? tenantProvider = null)
        : base(
            options,
            tenantProvider: tenantProvider,
            creatingContributors: [s_contributor],
            conventionContributors: [s_contributor],
            filterContributors: [s_tenantFilter])
    {
    }

    protected override void ConfigureConventionsCustom(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventionsCustom(configurationBuilder);
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter>();
        configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter>();
    }

    protected override void OnModelCreatingCustom(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingCustom(modelBuilder);
        modelBuilder.Entity<VKPsycheSessionEntity>()
            .Property(e => e.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever();
    }

    public DbSet<VKPsycheEchoEntity> Echoes => Set<VKPsycheEchoEntity>();
    public DbSet<VKPsycheDirectiveEntity> Directives => Set<VKPsycheDirectiveEntity>();
    public DbSet<VKPsycheKnowledgeEntity> Knowledges => Set<VKPsycheKnowledgeEntity>();
    public DbSet<VKPsycheKnowledgeKeyEntity> KnowledgeKeys => Set<VKPsycheKnowledgeKeyEntity>();
    public DbSet<VKPsychePatternEntity> Patterns => Set<VKPsychePatternEntity>();
    public DbSet<VKPsychePersonaEntity> Personas => Set<VKPsychePersonaEntity>();
    public DbSet<VKPsycheProfileEntity> Profiles => Set<VKPsycheProfileEntity>();
    public DbSet<VKPsycheSessionEntity> Sessions => Set<VKPsycheSessionEntity>();
}
