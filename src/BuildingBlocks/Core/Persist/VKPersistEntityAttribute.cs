using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a persistence entity class to trigger compile-time Source Generation of:
/// 1. Domain ↔ Entity Mapper (ToDomain, ToEntity, MapOnto).
/// 2. Global Repository Type Aliases (e.g. IVK{Entity}Repository -> IVKEntityRepository{TEntity}).
/// 3. EF Core EntityTypeConfiguration (Schema, Table, Keys, Columns, Indices).
/// 4. Query Objects &amp; Specifications.
/// Follows AP.01, AP.03, BB.01.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VKPersistEntityAttribute : Attribute
{
    /// <summary>
    /// Gets the optional pure domain model or aggregate root type.
    /// If provided, Domain ↔ Entity mapper extension methods are generated.
    /// </summary>
    public Type? DomainType { get; }

    /// <summary>
    /// Gets or sets the explicit target database table name.
    /// If omitted, defaults to the entity class name stripped of "Entity" suffix.
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Gets or sets the database schema name (e.g. "psyche", "identity").
    /// </summary>
    public string? Schema { get; init; }

    /// <summary>
    /// Gets or sets the names of nested Domain Value Object properties that are flattened onto this Entity.
    /// </summary>
    public string[]? FlattenBy { get; init; }

    /// <summary>
    /// Gets or sets the names of child collection properties (e.g. 1-to-many child entity collections) to project.
    /// </summary>
    public string[]? ProjectBy { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate global using repository type aliases for this entity.
    /// Defaults to true.
    /// </summary>
    public bool GenerateRepositoryAlias { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate EF Core IEntityTypeConfiguration for this entity.
    /// Defaults to true.
    /// </summary>
    public bool GenerateConfiguration { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate strongly-typed query extensions (GetBy/ExistsBy/CountBy) and Specifications.
    /// Defaults to true.
    /// </summary>
    public bool GenerateQueriesAndSpecs { get; init; } = true;

    public VKPersistEntityAttribute()
    {
    }

    public VKPersistEntityAttribute(string tableName)
    {
        TableName = tableName;
    }

    public VKPersistEntityAttribute(Type domainType)
    {
        DomainType = domainType;
    }

    public VKPersistEntityAttribute(Type domainType, string tableName)
    {
        DomainType = domainType;
        TableName = tableName;
    }
}
