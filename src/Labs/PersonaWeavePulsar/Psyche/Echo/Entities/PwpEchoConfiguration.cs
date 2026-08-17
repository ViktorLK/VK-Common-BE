using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;

public sealed class PwpEchoConfiguration : IEntityTypeConfiguration<PwpEchoEntity>
{
    private static readonly ValueConverter<DateTimeOffset, string> DateTimeOffsetToStringConverter = new(
        v => v.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"),
        v => DateTimeOffset.Parse(v));

    public void Configure(EntityTypeBuilder<PwpEchoEntity> builder)
    {
        builder.ToTable("VK_AI_Chat_Message");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Content).HasMaxLength(8000).IsRequired();
        builder.Property(e => e.CreatedAt).HasConversion(DateTimeOffsetToStringConverter);

        builder.HasIndex(e => new { e.SessionId, e.CreatedAt }).HasDatabaseName("IX_VK_AI_Chat_Message_SessionId_CreatedAt");
    }
}
