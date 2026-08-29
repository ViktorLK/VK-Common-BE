using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Diagnostics;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.Persist;

/// <summary>
/// Incremental Source Generator that produces zero-reflection persistence mappers for [VKPersistEntity]:
/// 1. ToDomain(this Entity entity) -> Rehydrates or instantiates Domain Aggregate.
/// 2. ToEntity(this Domain domain) -> Projections for INSERT operations.
/// 3. MapOnto(this Domain domain, Entity trackedEntity) -> In-place update with 3-phase differential synchronization for collections.
/// Follows AP.01, CS.01, CS.05, CS.08.
/// </summary>
[Generator]
public sealed class VKPersistMapperGenerator : IIncrementalGenerator
{
    private const string AttributeName = "VKPersistEntityAttribute";
    private const string AttributeFullName = $"VK.Blocks.Core.{AttributeName}";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformTarget(ctx))
            .Where(static t => t is not null);

        context.RegisterSourceOutput(targets, static (ctx, info) => EmitSource(ctx, info!));
    }

    private static PersistenceMappingInfo? TransformTarget(GeneratorAttributeSyntaxContext ctx)
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
        var domainProps = GetAllProperties(info.DomainType);
        var entityProps = GetAllProperties(info.EntityType);

        // Run Guardrail Validations (VK2010 ~ VK2013)
        ValidateMappingGuards(ctx, info, domainProps, entityProps);

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

        // 1. Generate ToDomain(this Entity entity)
        GenerateToDomain(sb, info, domainProps, entityProps);

        // 2. Generate ToEntity(this Domain domain)
        GenerateToEntity(sb, info, domainProps, entityProps);

        // 3. Generate MapOnto(this Domain domain, Entity trackedEntity)
        GenerateMapOnto(sb, info, domainProps, entityProps);

        sb.AppendLine("}");

        ctx.AddSource($"{info.ClassName}.g.cs", sb.ToString());
    }

    private static void ValidateMappingGuards(
        SourceProductionContext ctx,
        PersistenceMappingInfo info,
        ImmutableArray<IPropertySymbol> domainProps,
        ImmutableArray<IPropertySymbol> entityProps)
    {
        var entityLocation = info.EntityType.Locations.FirstOrDefault() ?? Location.None;

        // 1. Validate FlattenBy (VK2010, VK2013)
        foreach (var flatName in info.FlattenBy)
        {
            var parentProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, flatName, StringComparison.OrdinalIgnoreCase));
            if (parentProp?.Type is INamedTypeSymbol nestedType)
            {
                if (HasNestedComplexTypesOrCollections(nestedType, info.DomainType))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        VKDiagnosticDescriptors.NestingDepthExceedsLimit,
                        entityLocation,
                        $"{info.DomainType.Name}.{parentProp.Name}"));
                }

                var voProps = nestedType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer);
                foreach (var vp in voProps)
                {
                    if (!entityProps.Any(ep => string.Equals(ep.Name, vp.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            VKDiagnosticDescriptors.FlattenByPropertyMissingOnEntity,
                            entityLocation,
                            nestedType.Name,
                            vp.Name,
                            info.EntityType.Name));
                    }
                }
            }
        }

        // 2. Validate ProjectBy (VK2010, VK2011, VK2012)
        foreach (var projName in info.ProjectBy)
        {
            var entityProp = entityProps.FirstOrDefault(p => string.Equals(p.Name, projName, StringComparison.OrdinalIgnoreCase));
            var domainProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, projName, StringComparison.OrdinalIgnoreCase));

            if (entityProp?.Type is INamedTypeSymbol entCollType && domainProp?.Type is INamedTypeSymbol domCollType)
            {
                var entElemType = entCollType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
                var domElemType = domCollType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;

                if (entElemType is not null && domElemType is not null)
                {
                    if (HasNestedComplexTypesOrCollections(domElemType, info.DomainType) || HasNestedComplexTypesOrCollections(entElemType, info.EntityType))
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            VKDiagnosticDescriptors.NestingDepthExceedsLimit,
                            entityLocation,
                            $"{info.DomainType.Name}.{domainProp.Name}"));
                    }

                    var foreignKeyProp = FindParentForeignKey(entElemType, info);
                    if (foreignKeyProp is null)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            VKDiagnosticDescriptors.ProjectByMissingForeignKey,
                            entityLocation,
                            entElemType.Name,
                            projName,
                            info.EntityType.Name));
                    }

                    var discriminatorKeys = GetDiscriminatorKeys(entElemType, foreignKeyProp);
                    if (discriminatorKeys.Length == 0)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            VKDiagnosticDescriptors.ProjectByMissingKey,
                            entityLocation,
                            entElemType.Name,
                            projName));
                    }
                }
            }
        }
    }

    private static bool HasNestedComplexTypesOrCollections(INamedTypeSymbol typeSymbol, INamedTypeSymbol? parentType = null)
    {
        foreach (var prop in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (prop.IsStatic || prop.IsIndexer) continue;

            // Skip parent navigation reference
            if (parentType is not null && SymbolEqualityComparer.Default.Equals(prop.Type, parentType))
            {
                continue;
            }

            // Check if collection (IEnumerable and not string)
            if (prop.Type.AllInterfaces.Any(i => i.Name == "IEnumerable") && prop.Type.SpecialType != SpecialType.System_String)
            {
                return true;
            }

            // Check if nested custom class/record (not primitive, enum, datetime, guid, string, or strongly-typed ID)
            if (prop.Type.TypeKind == TypeKind.Class &&
                prop.Type.SpecialType == SpecialType.None &&
                prop.Type.ToDisplayString() != "System.DateTimeOffset" &&
                prop.Type.ToDisplayString() != "System.DateTime" &&
                prop.Type.ToDisplayString() != "System.TimeSpan" &&
                prop.Type.ToDisplayString() != "System.Guid" &&
                !prop.Type.Name.EndsWith("Id"))
            {
                return true;
            }
        }

        return false;
    }

    private static IPropertySymbol? FindParentForeignKey(INamedTypeSymbol childEntity, PersistenceMappingInfo info)
    {
        var childProps = GetAllProperties(childEntity);
        var expectedEntityFk = $"{info.EntityType.Name.Replace("Entity", "")}Id";
        var expectedDomainFk = $"{info.DomainType.Name.Replace("Entry", "").Replace("Aggregate", "")}Id";

        return childProps.FirstOrDefault(p =>
            string.Equals(p.Name, expectedEntityFk, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Name, expectedDomainFk, StringComparison.OrdinalIgnoreCase) ||
            (p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && !p.Name.StartsWith("Tenant", StringComparison.OrdinalIgnoreCase) && !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)));
    }

    private static ImmutableArray<IPropertySymbol> GetDiscriminatorKeys(INamedTypeSymbol childEntity, IPropertySymbol? parentFk)
    {
        var childProps = GetAllProperties(childEntity);
        var persistKeyProps = childProps
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name.Contains("VKPersistKey") == true))
            .ToList();

        if (persistKeyProps.Count > 0)
        {
            if (parentFk is not null)
            {
                persistKeyProps.RemoveAll(p => string.Equals(p.Name, parentFk.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (persistKeyProps.Count > 0)
            {
                return persistKeyProps.ToImmutableArray();
            }
        }

        var idProp = childProps.FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
        if (idProp is not null)
        {
            return ImmutableArray.Create(idProp);
        }

        return ImmutableArray<IPropertySymbol>.Empty;
    }

    private static ImmutableArray<IPropertySymbol> GetAllProperties(INamedTypeSymbol symbol)
    {
        var list = new List<IPropertySymbol>();
        var current = symbol;
        while (current is not null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!member.IsStatic && !member.IsIndexer && !list.Any(p => p.Name == member.Name))
                {
                    list.Add(member);
                }
            }
            current = current.BaseType;
        }
        return list.ToImmutableArray();
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

        var rehydrateMethod = info.DomainType.GetMembers("Rehydrate")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.IsStatic);

        if (rehydrateMethod is not null)
        {
            sb.AppendLine($"        var domain = {domainName}.Rehydrate(");
            var args = new List<string>();
            foreach (var param in rehydrateMethod.Parameters)
            {
                args.Add($"            {FormatDomainPropertyRehydrateArg(param, info, entityProps)}");
            }
            sb.AppendLine(string.Join(",\n", args));
            sb.AppendLine("        );");
            sb.AppendLine();
            sb.AppendLine($"        OnToDomainCustom(entity, domain);");
            sb.AppendLine("        return domain;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    static partial void OnToDomainCustom({entityName} entity, {domainName} domain);");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"        var domain = new {domainName}");
        sb.AppendLine("        {");

        foreach (var domainProp in domainProps)
        {
            if (domainProp.IsReadOnly || domainProp.SetMethod is null)
                continue;

            // 1. Flattened Value Object
            if (info.FlattenBy.Any(f => string.Equals(f, domainProp.Name, StringComparison.OrdinalIgnoreCase)) && domainProp.Type is INamedTypeSymbol nestedType)
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
            else if (info.ProjectBy.Any(p => string.Equals(p, domainProp.Name, StringComparison.OrdinalIgnoreCase)))
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
            var parentProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, flatName, StringComparison.OrdinalIgnoreCase));
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
            if (info.ProjectBy.Any(p => string.Equals(p, entityProp.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var matchingDomainProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, entityProp.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingDomainProp is not null && matchingDomainProp.Type is INamedTypeSymbol domCollType && entityProp.Type is INamedTypeSymbol entCollType)
                {
                    var domElemType = domCollType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
                    var entElemType = entCollType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;

                    if (domElemType is not null && entElemType is not null)
                    {
                        var foreignKeyProp = FindParentForeignKey(entElemType, info);

                        sb.AppendLine($"            {entityProp.Name} = domain.{matchingDomainProp.Name} is null or {{ Count: 0 }}");
                        sb.AppendLine($"                ? new List<{entElemType.ToDisplayString()}>()");
                        sb.AppendLine($"                : domain.{matchingDomainProp.Name}.Select(child => new {entElemType.ToDisplayString()}");
                        sb.AppendLine("                {");

                        var domChildProps = domElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer);
                        var entChildProps = entElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer && p.SetMethod is not null);

                        foreach (var ecp in entChildProps)
                        {
                            if (foreignKeyProp is not null && string.Equals(ecp.Name, foreignKeyProp.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                sb.AppendLine($"                    {ecp.Name} = domain.Id,");
                                continue;
                            }

                            var matchingChildDomainProp = domChildProps.FirstOrDefault(p => string.Equals(p.Name, ecp.Name, StringComparison.OrdinalIgnoreCase));
                            if (matchingChildDomainProp is not null)
                            {
                                sb.AppendLine($"                    {ecp.Name} = child.{matchingChildDomainProp.Name},");
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
            var parentProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, flatName, StringComparison.OrdinalIgnoreCase));
            if (parentProp?.Type is INamedTypeSymbol nestedType)
            {
                foreach (var np in nestedType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer))
                {
                    flattenLookup[np.Name] = (flatName, np);
                }
            }
        }

        // 1. Scalar and Flattened Properties update
        foreach (var entityProp in entityProps)
        {
            if (entityProp.IsReadOnly || entityProp.SetMethod is null)
                continue;

            if (string.Equals(entityProp.Name, "Id", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entityProp.Name, "CreatedAt", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entityProp.Name, "CreatedBy", StringComparison.OrdinalIgnoreCase) ||
                info.ProjectBy.Any(p => string.Equals(p, entityProp.Name, StringComparison.OrdinalIgnoreCase)))
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

        // 2. 3-Phase Differential Synchronization for ProjectBy Collections
        foreach (var projName in info.ProjectBy)
        {
            var entityProp = entityProps.FirstOrDefault(p => string.Equals(p.Name, projName, StringComparison.OrdinalIgnoreCase));
            var domainProp = domainProps.FirstOrDefault(p => string.Equals(p.Name, projName, StringComparison.OrdinalIgnoreCase));

            if (entityProp?.Type is INamedTypeSymbol entCollType && domainProp?.Type is INamedTypeSymbol domCollType)
            {
                var entElemType = entCollType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
                var domElemType = domCollType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;

                if (entElemType is not null && domElemType is not null)
                {
                    var foreignKeyProp = FindParentForeignKey(entElemType, info);
                    var discriminatorKeys = GetDiscriminatorKeys(entElemType, foreignKeyProp);

                    if (foreignKeyProp is not null && discriminatorKeys.Length > 0)
                    {
                        GenerateCollectionDiffSync(sb, domainProp.Name, entityProp.Name, entElemType, domElemType, foreignKeyProp, discriminatorKeys);
                    }
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine($"        OnMapOntoCustom(domain, trackedEntity);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    static partial void OnMapOntoCustom({domainName} domain, {entityName} trackedEntity);");
    }

    private static void GenerateCollectionDiffSync(
        StringBuilder sb,
        string domainCollName,
        string entityCollName,
        INamedTypeSymbol entElemType,
        INamedTypeSymbol domElemType,
        IPropertySymbol parentFk,
        ImmutableArray<IPropertySymbol> discriminatorKeys)
    {
        sb.AppendLine();
        sb.AppendLine($"        // Differential Synchronization for child collection: {entityCollName}");
        sb.AppendLine($"        if (trackedEntity.{entityCollName} is not null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            var domainItems = domain.{domainCollName} ?? [];");
        sb.AppendLine();

        // Build comparison predicate (e.K1 == d.K1 && e.K2 == d.K2)
        var matchConditions = discriminatorKeys.Select(k => $"e.{k.Name} == d.{k.Name}");
        var matchExpr = string.Join(" && ", matchConditions);

        // Phase 1: Delete
        sb.AppendLine($"            // Phase 1: Delete items no longer present in domain");
        sb.AppendLine($"            var toRemove = trackedEntity.{entityCollName}");
        sb.AppendLine($"                .Where(e => !domainItems.Any(d => {matchExpr}))");
        sb.AppendLine($"                .ToList();");
        sb.AppendLine($"            foreach (var item in toRemove)");
        sb.AppendLine("            {");
        sb.AppendLine($"                trackedEntity.{entityCollName}.Remove(item);");
        sb.AppendLine("            }");
        sb.AppendLine();

        // Phase 2 & 3: Update & Insert
        sb.AppendLine($"            // Phase 2 & 3: Update existing items or Insert new items");
        sb.AppendLine($"            foreach (var d in domainItems)");
        sb.AppendLine("            {");
        sb.AppendLine($"                var existing = trackedEntity.{entityCollName}");
        sb.AppendLine($"                    .FirstOrDefault(e => {matchExpr});");
        sb.AppendLine();
        sb.AppendLine("                if (existing is not null)");
        sb.AppendLine("                {");

        // Update mutable non-key properties
        var domChildProps = domElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer);
        var entChildProps = entElemType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer && p.SetMethod is not null);

        var mutableNonKeyProps = entChildProps.Where(ep =>
            !string.Equals(ep.Name, parentFk.Name, StringComparison.OrdinalIgnoreCase) &&
            !discriminatorKeys.Any(dk => string.Equals(dk.Name, ep.Name, StringComparison.OrdinalIgnoreCase)) &&
            !string.Equals(ep.Name, "CreatedAt", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ep.Name, "CreatedBy", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ep.Name, "IsDeleted", StringComparison.OrdinalIgnoreCase));

        foreach (var ecp in mutableNonKeyProps)
        {
            var matchingChildDomainProp = domChildProps.FirstOrDefault(p => string.Equals(p.Name, ecp.Name, StringComparison.OrdinalIgnoreCase));
            if (matchingChildDomainProp is not null)
            {
                sb.AppendLine($"                    existing.{ecp.Name} = d.{matchingChildDomainProp.Name};");
            }
        }

        sb.AppendLine("                }");
        sb.AppendLine("                else");
        sb.AppendLine("                {");
        sb.AppendLine($"                    trackedEntity.{entityCollName}.Add(new {entElemType.ToDisplayString()}");
        sb.AppendLine("                    {");
        sb.AppendLine($"                        {parentFk.Name} = domain.Id,");

        foreach (var ecp in entChildProps)
        {
            if (string.Equals(ecp.Name, parentFk.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            var matchingChildDomainProp = domChildProps.FirstOrDefault(p => string.Equals(p.Name, ecp.Name, StringComparison.OrdinalIgnoreCase));
            if (matchingChildDomainProp is not null)
            {
                sb.AppendLine($"                        {ecp.Name} = d.{matchingChildDomainProp.Name},");
            }
        }

        sb.AppendLine("                    });");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    private static string FormatDomainPropertyRehydrateArg(
        IParameterSymbol param,
        PersistenceMappingInfo info,
        ImmutableArray<IPropertySymbol> entityProps)
    {
        // 1. Flattened Value Object
        if (info.FlattenBy.Any(f => string.Equals(f, param.Name, StringComparison.OrdinalIgnoreCase)) && param.Type is INamedTypeSymbol nestedType)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"new {nestedType.ToDisplayString()}");
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
            sb.Append("            }");
            return sb.ToString();
        }

        // 2. Child Collection Projection
        if (info.ProjectBy.Any(p => string.Equals(p, param.Name, StringComparison.OrdinalIgnoreCase)))
        {
            var matchingEntityProp = entityProps.FirstOrDefault(p => string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase));
            if (matchingEntityProp is not null && param.Type is INamedTypeSymbol domCollType && matchingEntityProp.Type is INamedTypeSymbol entCollType)
            {
                var domElemType = domCollType.TypeArguments.FirstOrDefault();
                var entElemType = entCollType.TypeArguments.FirstOrDefault();
                if (domElemType is not null && entElemType is not null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"entity.{matchingEntityProp.Name} is null or {{ Count: 0 }}");
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
                    sb.Append("                }).ToList()");
                    return sb.ToString();
                }
            }
        }

        // 3. Direct matching property
        var matchingDirect = entityProps.FirstOrDefault(p => string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase));
        if (matchingDirect is not null)
        {
            return FormatEntityToDomainParam(matchingDirect, param.Type);
        }

        return "default!";
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
