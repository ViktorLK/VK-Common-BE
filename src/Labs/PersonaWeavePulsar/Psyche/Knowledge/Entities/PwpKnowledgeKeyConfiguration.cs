using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;

public sealed class PwpKnowledgeKeyConfiguration : IEntityTypeConfiguration<PwpKnowledgeKeyEntity>
{
    public void Configure(EntityTypeBuilder<PwpKnowledgeKeyEntity> builder)
    {
        builder.ToTable("VK_AI_Knowledge_Key");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Text).HasMaxLength(256).IsRequired();

        builder.HasIndex(k => k.KnowledgeEntryId).HasDatabaseName("IX_VK_AI_Knowledge_Key_KnowledgeEntryId");
    }
}
