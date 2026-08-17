using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;

public sealed class PwpKnowledgeConfiguration : IEntityTypeConfiguration<PwpKnowledgeEntity>
{
    private static readonly ValueConverter<DateTimeOffset, string> DateTimeOffsetToStringConverter = new(
        v => v.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"),
        v => DateTimeOffset.Parse(v));

    public void Configure(EntityTypeBuilder<PwpKnowledgeEntity> builder)
    {
        builder.ToTable("VK_AI_Knowledge_Entry");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExpiresAt).HasConversion(DateTimeOffsetToStringConverter);
        builder.Property(e => e.ExclusiveGroup).HasMaxLength(64);
        builder.Property(e => e.Tag).HasMaxLength(64);
        builder.Property(e => e.StateConditions).HasMaxLength(256);
        builder.Property(e => e.ExclusionTag).HasMaxLength(64);
        builder.Property(e => e.DependencyId).HasMaxLength(64);
        builder.Property(e => e.ConflictGroupId).HasMaxLength(64);
        builder.Property(e => e.RevealSecretKey).HasMaxLength(128);
        builder.Property(e => e.TargetPersonaId).HasMaxLength(64);
        builder.Property(e => e.UserSegment).HasMaxLength(128);
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.OwnsOne(e => e.Segment, layout =>
        {
            layout.Property(l => l.Content).HasColumnName("Content").HasMaxLength(4000).IsRequired();
            layout.Property(l => l.Name).HasColumnName("Name").HasMaxLength(128);
            layout.Property(l => l.IsEnabled).HasColumnName("IsEnabled");
            layout.Property(l => l.TargetRole).HasColumnName("TargetRole");
            layout.Property(l => l.AbsoluteDepth).HasColumnName("AbsoluteDepth");
            layout.Property(l => l.RelativeAnchor).HasColumnName("RelativeAnchor");
            layout.Property(l => l.Priority).HasColumnName("Priority");
        });

        builder.HasMany(e => e.Keys)
            .WithOne(k => k.Entry)
            .HasForeignKey(k => k.KnowledgeEntryId)
            .HasConstraintName("FK_VK_AI_Knowledge_Key_VK_AI_Knowledge_Entry_KnowledgeEntryId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TenantId, e.KnowledgeBookId })
            .HasDatabaseName("IX_VK_AI_Knowledge_Entry_Tenant_Book");
    }
}
