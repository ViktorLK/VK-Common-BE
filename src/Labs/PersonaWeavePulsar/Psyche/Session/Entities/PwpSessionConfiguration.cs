using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Session.Entities;

public sealed class PwpSessionConfiguration : IEntityTypeConfiguration<PwpSessionEntity>
{
    private static readonly ValueConverter<DateTimeOffset, string> DateTimeOffsetToStringConverter = new(
        v => v.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"),
        v => DateTimeOffset.Parse(v));

    public void Configure(EntityTypeBuilder<PwpSessionEntity> builder)
    {
        builder.ToTable("VK_AI_Chat_Session");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ForkPointRef).HasMaxLength(128);
        builder.Property(e => e.CustomModelId).HasMaxLength(128);
        builder.Property(e => e.CustomApiKey).HasMaxLength(1024);
        builder.Property(e => e.CustomServiceType).HasMaxLength(64);
        builder.Property(e => e.CustomEndpoint).HasMaxLength(512);
        builder.Property(e => e.KnowledgeStateJson).HasMaxLength(4000);
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.Property(e => e.CreatedAt).HasConversion(DateTimeOffsetToStringConverter);
        builder.Property(e => e.UpdatedAt).HasConversion(DateTimeOffsetToStringConverter);
        builder.Property(e => e.LastActivityAt).HasConversion(DateTimeOffsetToStringConverter);

        builder.HasMany(e => e.Messages)
            .WithOne()
            .HasForeignKey(m => m.SessionId)
            .HasConstraintName("FK_VK_AI_Chat_Message_VK_AI_Chat_Session_SessionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TenantId, e.UserId, e.CreatedAt })
            .HasDatabaseName("IX_VK_AI_Chat_Session_TenantId_UserId_CreatedAt");
        builder.HasIndex(e => new { e.TenantId, e.PersonaId })
            .HasDatabaseName("IX_VK_AI_Chat_Session_TenantId_PersonaId");
    }
}
