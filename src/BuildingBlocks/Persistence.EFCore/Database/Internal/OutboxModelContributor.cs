using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Model creating contributor that configures the standard schema mapping for <see cref="VKOutboxMessage"/>.
/// Follows AP.01, CS.08, AP.03.
/// </summary>
internal sealed class OutboxModelContributor : IVKModelCreatingContributor
{
    /// <inheritdoc />
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        VKGuard.NotNull(modelBuilder); // [AP.01]

        var entity = modelBuilder.Entity<VKOutboxMessage>();

        entity.ToTable("vk_outbox_messages");

        entity.HasKey(e => e.Id)
            .HasName("pk_vk_outbox_messages"); // [CS.08]

        entity.Property(e => e.EventType)
            .HasMaxLength(256) // [CS.08]
            .IsRequired();

        entity.Property(e => e.Payload)
            .IsRequired();

        entity.Property(e => e.OccurredOn)
            .IsRequired();

        entity.Property(e => e.ProcessedOn)
            .IsRequired(false);

        entity.Property(e => e.RetryCount)
            .HasDefaultValue(0)
            .IsRequired();

        // Index for pending message retrieval [CS.08]
        entity.HasIndex(e => new { e.ProcessedOn, e.OccurredOn })
            .HasDatabaseName("ix_vk_outbox_messages_pending");
    }
}
