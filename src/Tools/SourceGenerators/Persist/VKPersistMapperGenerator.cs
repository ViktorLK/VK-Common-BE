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
/// Universal Incremental Source Generator that produces industrial-grade Domain ↔ Persistence Entity mappers.
/// Pure meta-programming: ZERO hardcoded business class names.
/// Supports FlattenProperties and ChildCollections projection.
/// </summary>
[Generator]
public sealed class VKPersistMapperGenerator : IIncrementalGenerator
{
    private const string PersistEntityAttributeFullName = "VK.Blocks.Core.VKPersistEntityAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var persistEntityTargets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PersistEntityAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformEntityTarget(ctx))
            .Where(static t => t is not null);

        context.RegisterSourceOutput(persistEntityTargets, (ctx, target) => EmitSource(ctx, target!));
    }

    private static PersistenceMappingInfo? TransformEntityTarget(GeneratorAttributeSyntaxContext ctx)
    {
        var entitySymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attr = ctx.Attributes[0];

        if (attr.ConstructorArguments.Length < 1)
            return null;

        var domainType = attr.ConstructorArguments[0].Value as INamedTypeSymbol;
        if (domainType is null)
            return null;

        var flattenBy = new List<string>();
        var projectBy = new List<string>();

        foreach (var named in attr.NamedArguments)
        {
            if (string.Equals(named.Key, "FlattenBy", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(named.Key, "FlattenProperties", StringComparison.OrdinalIgnoreCase))
            {
                if (named.Value.Values.Length > 0)
                {
                    foreach (var val in named.Value.Values)
                    {
                        if (val.Value is string s && !string.IsNullOrWhiteSpace(s))
                        {
                            flattenBy.Add(s);
                        }
                    }
                }
            }
            if (string.Equals(named.Key, "ProjectBy", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(named.Key, "ChildCollections", StringComparison.OrdinalIgnoreCase))
            {
                if (named.Value.Values.Length > 0)
                {
                    foreach (var val in named.Value.Values)
                    {
                        if (val.Value is string s && !string.IsNullOrWhiteSpace(s))
                        {
                            projectBy.Add(s);
                        }
                    }
                }
            }
        }

        var entityName = entitySymbol.Name;
        var baseName = entityName.EndsWith("Entity") ? entityName.Substring(0, entityName.Length - 6) : entityName;
        var mapperClassName = $"{baseName}Mapper";

        var accessibility = entitySymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            _ => "public"
        };

        return new PersistenceMappingInfo(
            Namespace: entitySymbol.ContainingNamespace.ToDisplayString(),
            ClassName: mapperClassName,
            Modifiers: $"{accessibility} static partial",
            DomainType: domainType,
            EntityType: entitySymbol,
            FlattenBy: flattenBy,
            ProjectBy: projectBy
        );
    }

    private static void EmitSource(SourceProductionContext ctx, PersistenceMappingInfo info)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {info.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"{info.Modifiers} class {info.ClassName}");
        sb.AppendLine("{");

        var domainProps = info.DomainType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer).ToImmutableArray();
        var entityProps = info.EntityType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer).ToImmutableArray();

        // 1. Generate ToDomain(this Entity entity)
        GenerateToDomain(sb, info, domainProps, entityProps);

        // 2. Generate ToEntity(this Domain domain)
        GenerateToEntity(sb, info, domainProps, entityProps);

        // 3. Generate MapOnto(this Domain domain, Entity trackedEntity)
        GenerateMapOnto(sb, info, domainProps, entityProps);

        sb.AppendLine("}");

        ctx.AddSource($"{info.ClassName}.g.cs", sb.ToString());
    }

    private static void GenerateToDomain(
        StringBuilder sb,
        PersistenceMappingInfo info,
        ImmutableArray<IPropertySymbol> domainProps,
        ImmutableArray<IPropertySymbol> entityProps)
    {
        var domainName = info.DomainType.ToDisplayString();
        var entityName = info.EntityType.ToDisplayString();

        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Converts a persistence entity <see cref=\"{entityName}\"/> to a domain model <see cref=\"{domainName}\"/>.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static {domainName} ToDomain(this {entityName} entity)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(entity);");
        sb.AppendLine();
        sb.AppendLine($"        var domain = new {domainName}");
        sb.AppendLine("        {");

        foreach (var domainProp in domainProps)
        {
            if (domainProp.IsReadOnly || domainProp.SetMethod is null)
                continue;

            // 1. Flattened Value Object
            if (info.FlattenBy.Contains(domainProp.Name) && domainProp.Type is INamedTypeSymbol nestedType)
            {
                sb.AppendLine($"            {domainProp.Name} = new {nestedType.ToDisplayString()}");
                sb.AppendLine("            {");
                var nestedProps = nestedType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer && p.SetMethod is not null);
                foreach (var np in nestedProps)
                {
                    var matchingEntityProp = entityProps.FirstOrDefault(p => string.Equals(p.Name, np.Name, StringComparison.OrdinalIgnoreCase));
                    if (matchingEntityProp is not null)
                    {
                        sb.AppendLine($"                {np.Name} = {FormatEntityToDomainParam(matchingEntityProp, np.Type)},");
                    }
                }
                sb.AppendLine("            },");
            }
            // 2. Child Collection Projection
            else if (info.ProjectBy.Contains(domainProp.Name))
            {
                var matchingEntityProp = entityProps.FirstOrDefault(p => string.Equals(p.Name, domainProp.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingEntityProp is not null && domainProp.Type is INamedTypeSymbol domCollType && matchingEntityProp.Type is INamedTypeSymbol entCollType)
                {
                    var domElemType = domCollType.TypeArguments.FirstOrDefault();
                    var entElemType = entCollType.TypeArguments.FirstOrDefault();

                    if (domElemType is not null && entElemType is not null)
                    {
                        sb.AppendLine($"            {domainProp.Name} = entity.{matchingEntityProp.Name} is null or {{ Count: 0 }}");
                        sb.AppendLine($"                ? Array.Empty<{domElemType.ToDisplayString()}>()");
                        sb.AppendLine($"                : entity.{matchingEntityProp.Name}.Select(child => new {domElemType.ToDisplayString()}");
                        sb.AppendLine("                {");

                        var domChildProps = domElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer && p.SetMethod is not null);
                        var entChildProps = entElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer);
                        foreach (var dcp in domChildProps)
                        {
                            var matchingChildEntityProp = entChildProps.FirstOrDefault(p => string.Equals(p.Name, dcp.Name, StringComparison.OrdinalIgnoreCase));
                            if (matchingChildEntityProp is not null)
                            {
                                sb.AppendLine($"                    {dcp.Name} = child.{matchingChildEntityProp.Name},");
                            }
                        }
                        sb.AppendLine("                }).ToList(),");
                    }
                }
            }
            else
            {
                var matchingEntityProp = entityProps.FirstOrDefault(p => string.Equals(p.Name, domainProp.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingEntityProp is not null)
                {
                    sb.AppendLine($"            {domainProp.Name} = {FormatEntityToDomainParam(matchingEntityProp, domainProp.Type)},");
                }
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine();
        sb.AppendLine($"        OnToDomainCustom(entity, domain);");
        sb.AppendLine("        return domain;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    static partial void OnToDomainCustom({entityName} entity, {domainName} domain);");
        sb.AppendLine();
    }

    private static void GenerateToEntity(
        StringBuilder sb,
        PersistenceMappingInfo info,
        ImmutableArray<IPropertySymbol> domainProps,
        ImmutableArray<IPropertySymbol> entityProps)
    {
        var domainName = info.DomainType.ToDisplayString();
        var entityName = info.EntityType.ToDisplayString();

        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Creates a new persistence entity <see cref=\"{entityName}\"/> from domain model <see cref=\"{domainName}\"/>.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static {entityName} ToEntity(this {domainName} domain)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(domain);");
        sb.AppendLine();
        sb.AppendLine($"        var entity = new {entityName}");
        sb.AppendLine("        {");

        // Map flatten props lookup table
        var flattenLookup = new Dictionary<string, (string ParentPropName, IPropertySymbol NestedProp)>(StringComparer.OrdinalIgnoreCase);
        foreach (var flatName in info.FlattenBy)
        {
            var parentProp = domainProps.FirstOrDefault(p => p.Name == flatName);
            if (parentProp?.Type is INamedTypeSymbol nestedType)
            {
                foreach (var np in nestedType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer))
                {
                    flattenLookup[np.Name] = (flatName, np);
                }
            }
        }

        foreach (var entityProp in entityProps)
        {
            if (entityProp.IsReadOnly || entityProp.SetMethod is null)
                continue;

            if (string.Equals(entityProp.Name, "Id", StringComparison.OrdinalIgnoreCase))
            {
                var domainHasId = domainProps.Any(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
                if (domainHasId)
                {
                    sb.AppendLine($"            Id = domain.Id,");
                }
                continue;
            }

            // 1. Child collection
            if (info.ProjectBy.Contains(entityProp.Name))
            {
                var matchingDomainProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, entityProp.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingDomainProp is not null && matchingDomainProp.Type is INamedTypeSymbol domCollType && entityProp.Type is INamedTypeSymbol entCollType)
                {
                    var domElemType = domCollType.TypeArguments.FirstOrDefault();
                    var entElemType = entCollType.TypeArguments.FirstOrDefault();

                    if (domElemType is not null && entElemType is not null)
                    {
                        sb.AppendLine($"            {entityProp.Name} = domain.{matchingDomainProp.Name} is null or {{ Count: 0 }}");
                        sb.AppendLine($"                ? new List<{entElemType.ToDisplayString()}>()");
                        sb.AppendLine($"                : domain.{matchingDomainProp.Name}.Select(child => new {entElemType.ToDisplayString()}");
                        sb.AppendLine("                {");

                        var domChildProps = domElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer);
                        var entChildProps = entElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer && p.SetMethod is not null);

                        foreach (var ecp in entChildProps)
                        {
                            var matchingChildDomainProp = domChildProps.FirstOrDefault(p => string.Equals(p.Name, ecp.Name, StringComparison.OrdinalIgnoreCase));
                            if (matchingChildDomainProp is not null)
                            {
                                sb.AppendLine($"                    {ecp.Name} = child.{matchingChildDomainProp.Name},");
                                continue;
                            }

                            if (string.Equals(ecp.Name, $"{info.EntityType.Name.Replace("Entity", "")}Id", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(ecp.Name, $"{info.DomainType.Name.Replace("Entry", "").Replace("Aggregate", "")}Id", StringComparison.OrdinalIgnoreCase) ||
                                (ecp.Name.EndsWith("Id") && !ecp.Name.StartsWith("Tenant")))
                            {
                                sb.AppendLine($"                    {ecp.Name} = domain.Id,");
                                continue;
                            }
                        }
                        sb.AppendLine("                }).ToList(),");
                    }
                }
            }
            else
            {
                var matchingDomainProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, entityProp.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingDomainProp is not null)
                {
                    sb.AppendLine($"            {entityProp.Name} = {FormatDomainToEntityProp(matchingDomainProp, entityProp.Type, "domain")},");
                }
                else if (flattenLookup.TryGetValue(entityProp.Name, out var flatInfo))
                {
                    sb.AppendLine($"            {entityProp.Name} = {FormatDomainToEntityProp(flatInfo.NestedProp, entityProp.Type, $"domain.{flatInfo.ParentPropName}")},");
                }
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine();
        sb.AppendLine($"        OnToEntityCustom(domain, entity);");
        sb.AppendLine("        return entity;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    static partial void OnToEntityCustom({domainName} domain, {entityName} entity);");
        sb.AppendLine();
    }

    private static void GenerateMapOnto(
        StringBuilder sb,
        PersistenceMappingInfo info,
        ImmutableArray<IPropertySymbol> domainProps,
        ImmutableArray<IPropertySymbol> entityProps)
    {
        var domainName = info.DomainType.ToDisplayString();
        var entityName = info.EntityType.ToDisplayString();

        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// In-place writes domain model state onto an already tracked persistence entity <see cref=\"{entityName}\"/>.");
        sb.AppendLine($"    /// Preserves EF Core snapshot change tracking for minimal column updates.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static void MapOnto(this {domainName} domain, {entityName} trackedEntity)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(domain);");
        sb.AppendLine("        VKGuard.NotNull(trackedEntity);");
        sb.AppendLine();

        var flattenLookup = new Dictionary<string, (string ParentPropName, IPropertySymbol NestedProp)>(StringComparer.OrdinalIgnoreCase);
        foreach (var flatName in info.FlattenBy)
        {
            var parentProp = domainProps.FirstOrDefault(p => p.Name == flatName);
            if (parentProp?.Type is INamedTypeSymbol nestedType)
            {
                foreach (var np in nestedType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer))
                {
                    flattenLookup[np.Name] = (flatName, np);
                }
            }
        }

        foreach (var entityProp in entityProps)
        {
            if (entityProp.IsReadOnly || entityProp.SetMethod is null)
                continue;

            if (string.Equals(entityProp.Name, "Id", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entityProp.Name, "CreatedAt", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entityProp.Name, "CreatedBy", StringComparison.OrdinalIgnoreCase) ||
                info.ProjectBy.Contains(entityProp.Name))
            {
                continue;
            }

            var matchingDomainProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, entityProp.Name, StringComparison.OrdinalIgnoreCase));
            if (matchingDomainProp is not null)
            {
                sb.AppendLine($"        trackedEntity.{entityProp.Name} = {FormatDomainToEntityProp(matchingDomainProp, entityProp.Type, "domain")};");
            }
            else if (flattenLookup.TryGetValue(entityProp.Name, out var flatInfo))
            {
                sb.AppendLine($"        trackedEntity.{entityProp.Name} = {FormatDomainToEntityProp(flatInfo.NestedProp, entityProp.Type, $"domain.{flatInfo.ParentPropName}")};");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"        OnMapOntoCustom(domain, trackedEntity);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    static partial void OnMapOntoCustom({domainName} domain, {entityName} trackedEntity);");
    }

    private static string FormatEntityToDomainParam(IPropertySymbol entityProp, ITypeSymbol targetType)
    {
        if (SymbolEqualityComparer.Default.Equals(entityProp.Type, targetType))
        {
            return $"entity.{entityProp.Name}";
        }

        if (entityProp.Type.ToDisplayString().StartsWith("System.DateTimeOffset?") && targetType.ToDisplayString() == "System.DateTimeOffset")
        {
            return $"entity.{entityProp.Name} ?? entity.CreatedAt";
        }

        if (targetType.TypeKind == TypeKind.Enum && entityProp.Type.SpecialType == SpecialType.System_Byte)
        {
            return $"({targetType.ToDisplayString()})entity.{entityProp.Name}";
        }

        // Single-Value Object: check if targetType has [VKValueObject] or has Create(entityProp.Type)
        if (targetType is INamedTypeSymbol namedTargetType)
        {
            var isVo = namedTargetType.HasAttribute("VK.Blocks.Core.VKValueObjectAttribute") ||
                       namedTargetType.GetMembers("Value").OfType<IPropertySymbol>().Any();

            if (isVo)
            {
                var createMethod = namedTargetType.GetMembers("Create")
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => m.IsStatic && m.Parameters.Length == 1);

                if (createMethod is not null)
                {
                    if (createMethod.ReturnType.Name.Contains("Result"))
                    {
                        return $"{namedTargetType.ToDisplayString()}.Create(entity.{entityProp.Name}).Value!";
                    }

                    return $"{namedTargetType.ToDisplayString()}.Create(entity.{entityProp.Name})";
                }

                var ctor = namedTargetType.Constructors
                    .FirstOrDefault(c => c.Parameters.Length == 1 &&
                                         SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, entityProp.Type));
                if (ctor is not null)
                {
                    return $"new {namedTargetType.ToDisplayString()}(entity.{entityProp.Name})";
                }
            }
        }

        return $"entity.{entityProp.Name}";
    }

    private static string FormatDomainToEntityProp(IPropertySymbol domainProp, ITypeSymbol targetEntityType, string domainPrefix)
    {
        if (SymbolEqualityComparer.Default.Equals(domainProp.Type, targetEntityType))
        {
            return $"{domainPrefix}.{domainProp.Name}";
        }

        if (domainProp.Type.TypeKind == TypeKind.Enum && targetEntityType.SpecialType == SpecialType.System_Byte)
        {
            return $"(byte){domainPrefix}.{domainProp.Name}";
        }

        // Single-Value Object: check if domainProp.Type has [VKValueObject] or .Value property
        if (domainProp.Type is INamedTypeSymbol namedDomainType)
        {
            var valueProp = namedDomainType.GetMembers("Value")
                .OfType<IPropertySymbol>()
                .FirstOrDefault();

            if (valueProp is not null && (namedDomainType.HasAttribute("VK.Blocks.Core.VKValueObjectAttribute") ||
                                          SymbolEqualityComparer.Default.Equals(valueProp.Type, targetEntityType)))
            {
                return $"{domainPrefix}.{domainProp.Name}.{valueProp.Name}";
            }
        }

        return $"{domainPrefix}.{domainProp.Name}";
    }

    private record PersistenceMappingInfo(
        string Namespace,
        string ClassName,
        string Modifiers,
        INamedTypeSymbol DomainType,
        INamedTypeSymbol EntityType,
        List<string> FlattenBy,
        List<string> ProjectBy
    );
}
