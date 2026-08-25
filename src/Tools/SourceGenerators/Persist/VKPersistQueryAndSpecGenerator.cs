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
/// Incremental Source Generator that emits strongly-typed Query Object Extensions
/// (e.g. GetByXxxAsync, ExistsByXxxAsync, CountByXxxAsync, ListByXxxAsync)
/// and Specification classes for entities with [VKPersistEntity] and [VKPersistIndex].
/// </summary>
[Generator]
public sealed class VKPersistQueryAndSpecGenerator : IIncrementalGenerator
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

        context.RegisterSourceOutput(entityDeclarations, static (ctx, entity) => EmitQueriesAndSpecs(ctx, entity!));
    }

    private static EntityQueryMeta? TransformTarget(GeneratorAttributeSyntaxContext ctx)
    {
        var entitySymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == PersistEntityAttributeFullName);

        if (attr is null) return null;

        foreach (var namedArg in attr.NamedArguments)
        {
            if (string.Equals(namedArg.Key, "GenerateQueriesAndSpecs", StringComparison.OrdinalIgnoreCase))
            {
                if (namedArg.Value.Value is bool b && !b)
                    return null;
            }
        }

        var properties = entitySymbol.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer).ToList();
        var queryFields = new List<QueryFieldMeta>();

        bool hasSoftDelete = entitySymbol.AllInterfaces.Any(i => i.Name is "IVKSoftDelete" or "ISoftDelete");
        bool hasEnabled = properties.Any(p => p.Name is "IsEnabled" or "IsActive");

        foreach (var prop in properties)
        {
            var propAttrs = prop.GetAttributes();
            if (propAttrs.Any(a => a.AttributeClass?.Name is "VKPersistIgnoreAttribute" or "NotMappedAttribute"))
                continue;

            var indexAttr = propAttrs.FirstOrDefault(a => a.AttributeClass?.Name == "VKPersistIndexAttribute");
            bool isExplicitIndexed = indexAttr is not null;
            bool generateQuery = true;
            bool isUnique = false;

            if (indexAttr is not null)
            {
                foreach (var na in indexAttr.NamedArguments)
                {
                    if (string.Equals(na.Key, "GenerateQuery", StringComparison.OrdinalIgnoreCase) && na.Value.Value is bool gq)
                        generateQuery = gq;
                    if (string.Equals(na.Key, "IsUnique", StringComparison.OrdinalIgnoreCase) && na.Value.Value is bool u)
                        isUnique = u;
                }
            }

            // Also auto-detect common Id/Code lookup patterns if named like *Id or Code or TenantId
            bool shouldInclude = (isExplicitIndexed && generateQuery) ||
                                 (prop.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && prop.Name != "Id") ||
                                 prop.Name.Equals("Code", StringComparison.OrdinalIgnoreCase);

            if (shouldInclude)
            {
                queryFields.Add(new QueryFieldMeta(
                    PropertyName: prop.Name,
                    PropertyTypeName: prop.Type.ToDisplayString(),
                    IsUnique: isUnique
                ));
            }
        }

        var entityName = entitySymbol.Name;
        var baseName = entityName.EndsWith("Entity") ? entityName.Substring(0, entityName.Length - 6) : entityName;

        return new EntityQueryMeta(
            Namespace: entitySymbol.ContainingNamespace.ToDisplayString(),
            EntityName: entityName,
            EntityFullName: entitySymbol.ToDisplayString(),
            BaseName: baseName,
            HasSoftDelete: hasSoftDelete,
            HasEnabled: hasEnabled,
            QueryFields: queryFields.Distinct().ToList()
        );
    }

    private static void EmitQueriesAndSpecs(SourceProductionContext ctx, EntityQueryMeta info)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine("using VK.Blocks.Persistence;");
        sb.AppendLine();
        sb.AppendLine($"namespace {info.Namespace};");
        sb.AppendLine();

        // 1. Query Extensions
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Compile-time generated query extensions for <see cref=\"{info.EntityFullName}\"/>.");
        sb.AppendLine($"/// Follows CS.01, CS.03, CS.04.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public static class {info.EntityName}QueryExtensions");
        sb.AppendLine("{");

        foreach (var field in info.QueryFields)
        {
            var paramName = char.ToLowerInvariant(field.PropertyName[0]) + field.PropertyName.Substring(1);

            // GetByXxxAsync
            sb.AppendLine($"    /// <summary>Finds a single <see cref=\"{info.EntityFullName}\"/> by {field.PropertyName}.</summary>");
            sb.AppendLine($"    public static Task<{info.EntityFullName}?> GetBy{field.PropertyName}Async(");
            sb.AppendLine($"        this IVKEntityReadRepository<{info.EntityFullName}> repository,");
            sb.AppendLine($"        {field.PropertyTypeName} {paramName},");
            sb.AppendLine($"        CancellationToken cancellationToken = default)");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        VKGuard.NotNull(repository);");
            sb.AppendLine($"        return repository.GetFirstOrDefaultAsync(e => e.{field.PropertyName} == {paramName}, cancellationToken: cancellationToken);");
            sb.AppendLine($"    }}");
            sb.AppendLine();

            // ExistsByXxxAsync
            sb.AppendLine($"    /// <summary>Checks existence of <see cref=\"{info.EntityFullName}\"/> by {field.PropertyName}.</summary>");
            sb.AppendLine($"    public static Task<bool> ExistsBy{field.PropertyName}Async(");
            sb.AppendLine($"        this IVKEntityReadRepository<{info.EntityFullName}> repository,");
            sb.AppendLine($"        {field.PropertyTypeName} {paramName},");
            sb.AppendLine($"        CancellationToken cancellationToken = default)");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        VKGuard.NotNull(repository);");
            sb.AppendLine($"        return repository.AnyAsync(e => e.{field.PropertyName} == {paramName}, cancellationToken: cancellationToken);");
            sb.AppendLine($"    }}");
            sb.AppendLine();

            // ListByXxxAsync (if not strictly unique)
            if (!field.IsUnique)
            {
                sb.AppendLine($"    /// <summary>Lists all <see cref=\"{info.EntityFullName}\"/> matching {field.PropertyName}.</summary>");
                sb.AppendLine($"    public static Task<IReadOnlyList<{info.EntityFullName}>> ListBy{field.PropertyName}Async(");
                sb.AppendLine($"        this IVKEntityReadRepository<{info.EntityFullName}> repository,");
                sb.AppendLine($"        {field.PropertyTypeName} {paramName},");
                sb.AppendLine($"        CancellationToken cancellationToken = default)");
                sb.AppendLine($"    {{");
                sb.AppendLine($"        VKGuard.NotNull(repository);");
                sb.AppendLine($"        return repository.GetListAsync(e => e.{field.PropertyName} == {paramName}, cancellationToken: cancellationToken);");
                sb.AppendLine($"    }}");
                sb.AppendLine();

                // CountByXxxAsync
                sb.AppendLine($"    /// <summary>Counts <see cref=\"{info.EntityFullName}\"/> by {field.PropertyName}.</summary>");
                sb.AppendLine($"    public static Task<int> CountBy{field.PropertyName}Async(");
                sb.AppendLine($"        this IVKEntityReadRepository<{info.EntityFullName}> repository,");
                sb.AppendLine($"        {field.PropertyTypeName} {paramName},");
                sb.AppendLine($"        CancellationToken cancellationToken = default)");
                sb.AppendLine($"    {{");
                sb.AppendLine($"        VKGuard.NotNull(repository);");
                sb.AppendLine($"        return repository.CountAsync(e => e.{field.PropertyName} == {paramName}, cancellationToken: cancellationToken);");
                sb.AppendLine($"    }}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");
        sb.AppendLine();

        // 2. Specifications Factory
        var specClassName = $"{info.BaseName}Specifications";
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Compile-time generated specifications factory for <see cref=\"{info.EntityFullName}\"/>.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public static class {specClassName}");
        sb.AppendLine("{");

        foreach (var field in info.QueryFields)
        {
            var paramName = char.ToLowerInvariant(field.PropertyName[0]) + field.PropertyName.Substring(1);
            sb.AppendLine($"    public static VKSpecification<{info.EntityFullName}> By{field.PropertyName}({field.PropertyTypeName} {paramName})");
            sb.AppendLine($"        => new By{field.PropertyName}Spec({paramName});");
            sb.AppendLine();
            sb.AppendLine($"    private sealed class By{field.PropertyName}Spec({field.PropertyTypeName} val) : VKSpecification<{info.EntityFullName}>(e => e.{field.PropertyName} == val) {{ }}");
            sb.AppendLine();
        }

        if (info.HasEnabled)
        {
            sb.AppendLine($"    public static VKSpecification<{info.EntityFullName}> ActiveOnly()");
            sb.AppendLine($"        => new ActiveOnlySpec();");
            sb.AppendLine();
            sb.AppendLine($"    private sealed class ActiveOnlySpec() : VKSpecification<{info.EntityFullName}>(e => e.IsEnabled == true) {{ }}");
            sb.AppendLine();
        }

        if (info.HasSoftDelete)
        {
            sb.AppendLine($"    public static VKSpecification<{info.EntityFullName}> NotDeleted()");
            sb.AppendLine($"        => new NotDeletedSpec();");
            sb.AppendLine();
            sb.AppendLine($"    private sealed class NotDeletedSpec() : VKSpecification<{info.EntityFullName}>(e => !e.IsDeleted) {{ }}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        ctx.AddSource($"{info.EntityName}QueriesAndSpecs.g.cs", sb.ToString());
    }

    private record EntityQueryMeta(
        string Namespace,
        string EntityName,
        string EntityFullName,
        string BaseName,
        bool HasSoftDelete,
        bool HasEnabled,
        List<QueryFieldMeta> QueryFields
    );

    private record QueryFieldMeta(
        string PropertyName,
        string PropertyTypeName,
        bool IsUnique
    );
}
