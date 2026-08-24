using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Blocks.AI.Psyche.EFCore.Pattern.Internal;

internal sealed class PatternConfiguration : IEntityTypeConfiguration<VKPsychePatternEntity>
{
    public void Configure(EntityTypeBuilder<VKPsychePatternEntity> builder)
    {
        builder.ToTable("VK_AI_Psyche_Pattern");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(128);
        builder.Property(e => e.Role).HasConversion<byte>().IsRequired();
        builder.Property(e => e.RelativeDepth).HasConversion<byte?>();
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("IX_VK_AI_Psyche_Pattern_TenantId");
    }
}
