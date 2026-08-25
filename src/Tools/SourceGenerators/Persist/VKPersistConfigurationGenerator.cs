using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Persist;

/// <summary>
/// Incremental Source Generator that emits standard EF Core <see cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{T}"/>
/// classes based on [VKPersistEntity], [VKPersistTable], [VKPersistColumn], [VKPersistIndex], and standard DataAnnotations.
/// Adheres strictly to CS.08 (table naming, column max length, explicit index names).
/// </summary>
[Generator]
public sealed class VKPersistConfigurationGenerator : IIncrementalGenerator
{
    private const string PersistEntityAttributeFullName = "VK.Blocks.Core.VKPersistEntityAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entityDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PersistEntityAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformTarget(ctx))
            .Where(static t => t is not null);

        context.RegisterSourceOutput(entityDeclarations, static (ctx, entity) => EmitConfiguration(ctx, entity!));
    }

    private static EntityConfigInfo? TransformTarget(GeneratorAttributeSyntaxContext ctx)
    {
        var entitySymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == PersistEntityAttributeFullName);

        if (attr is null) return null;

        // Check if GenerateConfiguration is explicitly false
        foreach (var namedArg in attr.NamedArguments)
        {
            if (string.Equals(namedArg.Key, "GenerateConfiguration", StringComparison.OrdinalIgnoreCase))
            {
                if (namedArg.Value.Value is bool b && !b)
                {
                    return null;
                }
            }
        }

        // Table Name & Schema (Extracted from [VKPersistEntity])
        string tableName = entitySymbol.Name;
        if (tableName.EndsWith("Entity"))
        {
            tableName = tableName.Substring(0, tableName.Length - 6);
        }
        string? schema = null;

        // Check positional constructor args: VKPersistEntity(string tableName) or VKPersistEntity(Type domainType, string tableName)
        foreach (var ctorArg in attr.ConstructorArguments)
        {
            if (ctorArg.Value is string tName && !string.IsNullOrWhiteSpace(tName))
            {
                tableName = tName;
            }
        }

        // Check named arguments: TableName, Schema
        foreach (var namedArg in attr.NamedArguments)
        {
            if (string.Equals(namedArg.Key, "TableName", StringComparison.OrdinalIgnoreCase) && namedArg.Value.Value is string tName && !string.IsNullOrWhiteSpace(tName))
            {
                tableName = tName;
            }
            if (string.Equals(namedArg.Key, "Schema", StringComparison.OrdinalIgnoreCase) && namedArg.Value.Value is string sName && !string.IsNullOrWhiteSpace(sName))
            {
                schema = sName;
            }
        }

        // Collect properties
        var properties = entitySymbol.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer).ToList();
        var keyProps = new List<(string Name, int Order)>();
        var columnConfigs = new List<ColumnPropertyInfo>();
        var indexConfigs = new List<IndexPropertyInfo>();
        var ignoredProps = new List<string>();

        foreach (var prop in properties)
        {
            var propAttrs = prop.GetAttributes();

            // 1. Ignore
            if (propAttrs.Any(a => a.AttributeClass?.Name is "VKPersistIgnoreAttribute" or "NotMappedAttribute"))
            {
                ignoredProps.Add(prop.Name);
                continue;
            }

            // 2. Primary Key (Strictly Explicit: [VKPersistKey] or [Key])
            var keyAttr = propAttrs.FirstOrDefault(a => a.AttributeClass?.Name is "VKPersistKeyAttribute" or "KeyAttribute");
            if (keyAttr is not null)
            {
                int order = 0;
                foreach (var na in keyAttr.NamedArguments)
                {
                    if (string.Equals(na.Key, "Order", StringComparison.OrdinalIgnoreCase) && na.Value.Value is int ord)
                        order = ord;
                }
                keyProps.Add((prop.Name, order));
            }

            // 3. MaxLength / StringLength / Required
            int? maxLength = null;
            bool isRequired = false;
            var strLenAttr = propAttrs.FirstOrDefault(a => a.AttributeClass?.Name is "StringLengthAttribute" or "MaxLengthAttribute");
            if (strLenAttr is not null && strLenAttr.ConstructorArguments.Length > 0 && strLenAttr.ConstructorArguments[0].Value is int len)
            {
                maxLength = len;
            }

            if (propAttrs.Any(a => a.AttributeClass?.Name == "RequiredAttribute"))
            {
                isRequired = true;
            }

            // 4. VKPersistColumn
            string? typeName = null;
            int? precision = null;
            int? scale = null;
            string? collation = null;
            string? customColumnName = null;

            var colAttr = propAttrs.FirstOrDefault(a => a.AttributeClass?.Name == "VKPersistColumnAttribute");
            if (colAttr is not null)
            {
                foreach (var na in colAttr.NamedArguments)
                {
                    if (string.Equals(na.Key, "TypeName", StringComparison.OrdinalIgnoreCase) && na.Value.Value is string tn)
                        typeName = tn;
                    if (string.Equals(na.Key, "Precision", StringComparison.OrdinalIgnoreCase) && na.Value.Value is int p && p >= 0)
                        precision = p;
                    if (string.Equals(na.Key, "Scale", StringComparison.OrdinalIgnoreCase) && na.Value.Value is int s && s >= 0)
                        scale = s;
                    if (string.Equals(na.Key, "Collation", StringComparison.OrdinalIgnoreCase) && na.Value.Value is string col)
                        collation = col;
                    if (string.Equals(na.Key, "Name", StringComparison.OrdinalIgnoreCase) && na.Value.Value is string cn)
                        customColumnName = cn;
                }
            }

            // 5. ValueConverter / Enum conversion
            string? converterTypeName = null;
            var convAttr = propAttrs.FirstOrDefault(a => a.AttributeClass?.Name == "VKPersistConverterAttribute");
            if (convAttr is not null && convAttr.ConstructorArguments.Length > 0 && convAttr.ConstructorArguments[0].Value is INamedTypeSymbol convSymbol)
            {
                converterTypeName = convSymbol.ToDisplayString();
            }
            else
            {
                // Auto-convert Enums to byte (or byte? for nullable enums) if no converter specified
                var rawType = prop.Type;
                if (rawType is INamedTypeSymbol nts && nts.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && nts.TypeArguments.Length == 1)
                {
                    if (nts.TypeArguments[0].TypeKind == TypeKind.Enum)
                    {
                        converterTypeName = "byte?";
                    }
                }
                else if (rawType.TypeKind == TypeKind.Enum)
                {
                    converterTypeName = "byte";
                }
            }

            if (maxLength.HasValue || isRequired || typeName is not null || precision.HasValue || scale.HasValue || collation is not null || customColumnName is not null || converterTypeName is not null)
            {
                columnConfigs.Add(new ColumnPropertyInfo(
                    PropertyName: prop.Name,
                    MaxLength: maxLength,
                    IsRequired: isRequired,
                    TypeName: typeName,
                    Precision: precision,
                    Scale: scale,
                    Collation: collation,
                    CustomColumnName: customColumnName,
                    ConverterTypeName: converterTypeName
                ));
            }

            // 6. Index
            foreach (var idxAttr in propAttrs.Where(a => a.AttributeClass?.Name == "VKPersistIndexAttribute"))
            {
                bool isUnique = false;
                string? group = null;
                int order = 0;
                string? explicitName = null;

                foreach (var na in idxAttr.NamedArguments)
                {
                    if (string.Equals(na.Key, "IsUnique", StringComparison.OrdinalIgnoreCase) && na.Value.Value is bool u)
                        isUnique = u;
                    if (string.Equals(na.Key, "Group", StringComparison.OrdinalIgnoreCase) && na.Value.Value is string g)
                        group = g;
                    if (string.Equals(na.Key, "Order", StringComparison.OrdinalIgnoreCase) && na.Value.Value is int o)
                        order = o;
                    if (string.Equals(na.Key, "Name", StringComparison.OrdinalIgnoreCase) && na.Value.Value is string n)
                        explicitName = n;
                }

                indexConfigs.Add(new IndexPropertyInfo(
                    PropertyName: prop.Name,
                    IsUnique: isUnique,
                    Group: group,
                    Order: order,
                    ExplicitName: explicitName
                ));
            }
        }

        return new EntityConfigInfo(
            Namespace: entitySymbol.ContainingNamespace.ToDisplayString(),
            EntityName: entitySymbol.Name,
            EntityFullName: entitySymbol.ToDisplayString(),
            TableName: tableName,
            Schema: schema,
            KeyProperties: keyProps.OrderBy(k => k.Order).Select(k => k.Name).Distinct().ToList(),
            Columns: columnConfigs,
            Indices: indexConfigs,
            IgnoredProperties: ignoredProps
        );
    }

    private static void EmitConfiguration(SourceProductionContext ctx, EntityConfigInfo info)
    {
        var configClassName = $"{info.EntityName}Configuration";
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
        sb.AppendLine();
        sb.AppendLine($"namespace {info.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Compile-time generated EF Core configuration for <see cref=\"{info.EntityFullName}\"/>.");
        sb.AppendLine($"/// Adheres to CS.08 (explicit table, max length, standardized index naming).");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"internal sealed partial class {configClassName} : IEntityTypeConfiguration<{info.EntityFullName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{info.EntityFullName}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine("        VK.Blocks.Core.VKGuard.NotNull(builder);");
        sb.AppendLine();

        // 1. ToTable
        if (!string.IsNullOrWhiteSpace(info.Schema))
        {
            sb.AppendLine($"        builder.ToTable(\"{info.TableName}\", \"{info.Schema}\");");
        }
        else
        {
            sb.AppendLine($"        builder.ToTable(\"{info.TableName}\");");
        }

        // 2. HasKey
        if (info.KeyProperties.Count == 1)
        {
            sb.AppendLine($"        builder.HasKey(e => e.{info.KeyProperties[0]});");
        }
        else if (info.KeyProperties.Count > 1)
        {
            var keyArgs = string.Join(", ", info.KeyProperties.Select(k => $"e.{k}"));
            sb.AppendLine($"        builder.HasKey(e => new {{ {keyArgs} }});");
        }

        // 3. Ignored properties
        foreach (var ign in info.IgnoredProperties)
        {
            sb.AppendLine($"        builder.Ignore(e => e.{ign});");
        }

        // 4. Column constraints
        foreach (var col in info.Columns)
        {
            var propChain = new StringBuilder();
            propChain.Append($"        builder.Property(e => e.{col.PropertyName})");

            if (!string.IsNullOrWhiteSpace(col.CustomColumnName))
            {
                propChain.Append($".HasColumnName(\"{col.CustomColumnName}\")");
            }
            if (!string.IsNullOrWhiteSpace(col.TypeName))
            {
                propChain.Append($".HasColumnType(\"{col.TypeName}\")");
            }
            if (col.MaxLength.HasValue && col.MaxLength.Value > 0)
            {
                propChain.Append($".HasMaxLength({col.MaxLength.Value})");
            }
            if (col.IsRequired)
            {
                propChain.Append(".IsRequired()");
            }
            if (col.Precision.HasValue && col.Precision.Value > 0)
            {
                if (col.Scale.HasValue && col.Scale.Value >= 0)
                {
                    propChain.Append($".HasPrecision({col.Precision.Value}, {col.Scale.Value})");
                }
                else
                {
                    propChain.Append($".HasPrecision({col.Precision.Value})");
                }
            }
            if (!string.IsNullOrWhiteSpace(col.Collation))
            {
                propChain.Append($".UseCollation(\"{col.Collation}\")");
            }
            if (!string.IsNullOrWhiteSpace(col.ConverterTypeName))
            {
                propChain.Append($".HasConversion<{col.ConverterTypeName}>()");
            }

            propChain.Append(";");
            sb.AppendLine(propChain.ToString());
        }

        // 5. Indices (grouping single vs composite)
        var singleIndices = info.Indices.Where(i => string.IsNullOrWhiteSpace(i.Group)).ToList();
        foreach (var idx in singleIndices)
        {
            var idxName = idx.ExplicitName ?? (idx.IsUnique ? $"UX_{info.TableName}_{idx.PropertyName}" : $"IX_{info.TableName}_{idx.PropertyName}");
            var idxCall = new StringBuilder();
            idxCall.Append($"        builder.HasIndex(e => e.{idx.PropertyName})");
            if (idx.IsUnique)
            {
                idxCall.Append(".IsUnique()");
            }
            idxCall.Append($".HasDatabaseName(\"{idxName}\");");
            sb.AppendLine(idxCall.ToString());
        }

        var compositeGroups = info.Indices.Where(i => !string.IsNullOrWhiteSpace(i.Group)).GroupBy(i => i.Group!).ToList();
        foreach (var grp in compositeGroups)
        {
            var orderedProps = grp.OrderBy(x => x.Order).Select(x => x.PropertyName).ToList();
            var propsExpr = string.Join(", ", orderedProps.Select(p => $"e.{p}"));
            var isUnique = grp.Any(x => x.IsUnique);
            var defaultIdxName = isUnique
                ? $"UX_{info.TableName}_{string.Join("_", orderedProps)}"
                : $"IX_{info.TableName}_{string.Join("_", orderedProps)}";

            var explicitName = grp.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ExplicitName))?.ExplicitName ?? defaultIdxName;

            var idxCall = new StringBuilder();
            idxCall.Append($"        builder.HasIndex(e => new {{ {propsExpr} }})");
            if (isUnique)
            {
                idxCall.Append(".IsUnique()");
            }
            idxCall.Append($".HasDatabaseName(\"{explicitName}\");");
            sb.AppendLine(idxCall.ToString());
        }

        // Custom partial hook
        sb.AppendLine();
        sb.AppendLine($"        ConfigureCustom(builder);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    static partial void ConfigureCustom(EntityTypeBuilder<{info.EntityFullName}> builder);");
        sb.AppendLine("}");

        ctx.AddSource($"{configClassName}.g.cs", sb.ToString());
    }

    private record EntityConfigInfo(
        string Namespace,
        string EntityName,
        string EntityFullName,
        string TableName,
        string? Schema,
        List<string> KeyProperties,
        List<ColumnPropertyInfo> Columns,
        List<IndexPropertyInfo> Indices,
        List<string> IgnoredProperties
    );

    private record ColumnPropertyInfo(
        string PropertyName,
        int? MaxLength,
        bool IsRequired,
        string? TypeName,
        int? Precision,
        int? Scale,
        string? Collation,
        string? CustomColumnName,
        string? ConverterTypeName
    );

    private record IndexPropertyInfo(
        string PropertyName,
        bool IsUnique,
        string? Group,
        int Order,
        string? ExplicitName
    );
}
