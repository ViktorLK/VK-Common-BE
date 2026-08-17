using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Entities;

public sealed class PwpPatternConfiguration : IEntityTypeConfiguration<PwpPatternEntity>
{
    public void Configure(EntityTypeBuilder<PwpPatternEntity> builder)
    {
        builder.ToTable("VK_AI_Tenant_Preset_Pattern");
        builder.HasKey(e => e.Id);
        builder.OwnsOne(e => e.Segment, layout =>
        {
            layout.Property(l => l.Content).HasColumnName("Content").HasMaxLength(4000).IsRequired();
            layout.Property(l => l.Name).HasColumnName("Name").HasMaxLength(128).IsRequired();
            layout.Property(l => l.IsEnabled).HasColumnName("IsEnabled");
            layout.Property(l => l.TargetRole).HasColumnName("TargetRole");
            layout.Property(l => l.AbsoluteDepth).HasColumnName("AbsoluteDepth");
            layout.Property(l => l.RelativeAnchor).HasColumnName("RelativeAnchor");
            layout.Property(l => l.Priority).HasColumnName("Priority");
        });

        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("IX_VK_AI_Tenant_Preset_Pattern_TenantId");
    }
}
