using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Profile.Entities;

public sealed class PwpProfileConfiguration : IEntityTypeConfiguration<PwpProfileEntity>
{
    private static readonly ValueConverter<DateTimeOffset, string> DateTimeOffsetToStringConverter = new(
        v => v.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"),
        v => DateTimeOffset.Parse(v));

    public void Configure(EntityTypeBuilder<PwpProfileEntity> builder)
    {
        builder.ToTable("VK_AI_User_Profile");
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.DisplayName).HasMaxLength(128);
        builder.Property(e => e.PreferredLanguage).HasMaxLength(16);
        builder.Property(e => e.TimeZone).HasMaxLength(64);
        builder.Property(e => e.PreferencesJson).HasMaxLength(4000);
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);
        builder.Property(e => e.CreatedAt).HasConversion(DateTimeOffsetToStringConverter);
        builder.Property(e => e.UpdatedAt).HasConversion(DateTimeOffsetToStringConverter);

        builder.HasIndex(e => new { e.TenantId, e.UserId }).HasDatabaseName("IX_VK_AI_User_Profile_TenantId_UserId");
    }
}
