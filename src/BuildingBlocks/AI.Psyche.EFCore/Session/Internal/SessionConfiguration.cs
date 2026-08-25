using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Blocks.AI.Psyche.EFCore.Session.Internal;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<VKPsycheSessionEntity>
{
    public void Configure(EntityTypeBuilder<VKPsycheSessionEntity> builder)
    {
        builder.ToTable("VK_AI_Psyche_Session");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Mode).HasConversion<byte>().IsRequired();
        builder.Property(e => e.Status).HasConversion<byte>().IsRequired();
        builder.Property(e => e.ForkPointRef).HasMaxLength(256);
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("IX_VK_AI_Psyche_Session_TenantId");
        builder.HasIndex(e => e.ParentSessionId).HasDatabaseName("IX_VK_AI_Psyche_Session_ParentSessionId");
    }
}
