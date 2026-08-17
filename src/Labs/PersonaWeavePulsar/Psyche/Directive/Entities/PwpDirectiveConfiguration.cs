using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive.Entities;

public sealed class PwpDirectiveConfiguration : IEntityTypeConfiguration<PwpDirectiveEntity>
{
    public void Configure(EntityTypeBuilder<PwpDirectiveEntity> builder)
    {
        builder.ToTable("VK_AI_Tenant_Directive");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BehaviorRules).HasMaxLength(4000);
        builder.Property(e => e.SafetyRules).HasMaxLength(4000);
        builder.Property(e => e.OutputConstraints).HasMaxLength(4000);
        builder.Property(e => e.Overview).HasMaxLength(4000);
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("IX_VK_AI_Tenant_Directive_TenantId");
    }
}
