using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Entities;

public sealed class PwpPersonaConfiguration : IEntityTypeConfiguration<PwpPersonaEntity>
{
    public void Configure(EntityTypeBuilder<PwpPersonaEntity> builder)
    {
        builder.ToTable("VK_AI_Persona");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1024);
        builder.Property(e => e.Personality).HasMaxLength(4000);
        builder.Property(e => e.Scenario).HasMaxLength(4000);
        builder.Property(e => e.FirstMessage).HasMaxLength(4000);
        builder.Property(e => e.DialogueExamples).HasMaxLength(4000);
        builder.Property(e => e.Traits).HasMaxLength(2000);
        builder.Property(e => e.CreatedBy).HasMaxLength(128);
        builder.Property(e => e.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(e => new { e.TenantId, e.Name }).HasDatabaseName("IX_VK_AI_Persona_TenantId_Name");
    }
}
