using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Provides VK-prefixed extension methods for EF Core types to maintain architectural consistency.
/// </summary>
public static class VKEntityExtensions
{
    /// <summary>
    /// Gets the entity associated with the entry.
    /// </summary>
    public static TEntity VKEntity<TEntity>(this EntityEntry<TEntity> entry) where TEntity : class => entry.Entity;

    /// <summary>
    /// Gets the entity associated with the non-generic entry.
    /// </summary>
    public static object VKEntity(this EntityEntry entry) => entry.Entity;

    /// <summary>
    /// Configures an entity type in the model.
    /// </summary>
    public static EntityTypeBuilder VKEntity(this ModelBuilder modelBuilder, Type type) => modelBuilder.Entity(type);

    /// <summary>
    /// Configures an entity type in the model.
    /// </summary>
    public static EntityTypeBuilder<TEntity> VKEntity<TEntity>(this ModelBuilder modelBuilder) where TEntity : class => modelBuilder.Entity<TEntity>();
}
