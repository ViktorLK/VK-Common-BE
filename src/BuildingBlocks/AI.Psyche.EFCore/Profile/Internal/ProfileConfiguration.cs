using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Blocks.AI.Psyche.EFCore.Profile.Internal;

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<VKPsycheProfileEntity>
{
    public void Configure(EntityTypeBuilder<VKPsycheProfileEntity> builder)
    {
        builder.ToTable("VK_AI_Psyche_Profile");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DisplayName).HasMaxLength(128);
        builder.Property(e => e.PreferredLanguage).HasMaxLength(32);
        builder.Property(e => e.TimeZone).HasMaxLength(64);
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("IX_VK_AI_Psyche_Profile_TenantId");
    }
}
