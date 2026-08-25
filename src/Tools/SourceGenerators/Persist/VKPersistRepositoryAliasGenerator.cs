using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Persist;

/// <summary>
/// Incremental Source Generator that emits strongly-typed Public Repository Interfaces
/// and Internal Sealed Implementation Classes for entities marked with <c>[VKPersistEntity]</c>.
/// Produces:
/// 1. <c>public interface IVK{Entity}Repository : IVKEntityRepository{Entity}</c> (Public API, exported across projects)
/// 2. <c>internal sealed class {Entity}Repository : VKEFCoreRepository{Entity}, IVK{Entity}Repository</c> (Zero overhead)
/// 3. <c>internal static void RegisterPersistenceRepositories(IServiceCollection services)</c> (Automated DI hook)
/// Pure meta-programming: ZERO hardcoded business class names, ZERO manual boilerplate.
/// </summary>
[Generator]
public sealed class VKPersistRepositoryAliasGenerator : IIncrementalGenerator
{
    private const string PersistEntityAttributeFullName = "VK.Blocks.Core.VKPersistEntityAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entityDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: static (ctx, _) => TransformSyntaxTarget(ctx))
            .Where(static t => t is not null);

        context.RegisterSourceOutput(entityDeclarations, static (ctx, entity) => EmitEntityRepository(ctx, entity!));

        // Group by Assembly to emit DI registration extension
        var collected = entityDeclarations.Collect();
        context.RegisterSourceOutput(collected, static (ctx, list) => EmitDIExtensions(ctx, list!));
    }

    private static EntityRepositoryInfo? TransformSyntaxTarget(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is not ClassDeclarationSyntax classSyntax) return null;
        var symbol = ctx.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
        if (symbol is null) return null;

        var attr = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == PersistEntityAttributeFullName);

        if (attr is null) return null;

        return TransformTarget(symbol, attr);
    }

    private static EntityRepositoryInfo? TransformTarget(INamedTypeSymbol entitySymbol, AttributeData attr)
    {
        // Check GenerateRepositoryAlias property if explicitly set to false
        foreach (var namedArg in attr.NamedArguments)
        {
            if (string.Equals(namedArg.Key, "GenerateRepositoryAlias", System.StringComparison.OrdinalIgnoreCase))
            {
                if (namedArg.Value.Value is bool b && !b)
                {
                    return null;
                }
            }
        }

        var entityName = entitySymbol.Name;
        var baseName = entityName.EndsWith("Entity")
            ? entityName.Substring(0, entityName.Length - 6)
            : entityName;

        var interfaceName = baseName.StartsWith("VK")
            ? $"I{baseName}EntityRepository"
            : $"IVK{baseName}EntityRepository";

        var implementationName = baseName.StartsWith("VK")
            ? $"{baseName.Substring(2)}EntityRepository"
            : $"{baseName}EntityRepository";

        return new EntityRepositoryInfo(
            Namespace: entitySymbol.ContainingNamespace.ToDisplayString(),
            EntityFullName: entitySymbol.ToDisplayString(),
            EntityName: entityName,
            InterfaceName: interfaceName,
            ImplementationName: implementationName
        );
    }

    private static void EmitEntityRepository(SourceProductionContext ctx, EntityRepositoryInfo info)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine("using VK.Blocks.Persistence;");
        sb.AppendLine("using VK.Blocks.Persistence.EFCore;");
        sb.AppendLine();
        sb.AppendLine($"namespace {info.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Dedicated strongly-typed repository interface for <see cref=\"{info.EntityFullName}\"/>.");
        sb.AppendLine($"/// Inherits full CRUD and bulk capabilities from <see cref=\"IVKEntityRepository{{T}}\"/>.");
        sb.AppendLine($"/// Exported public interface accessible across assemblies.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public interface {info.InterfaceName} : IVKEntityRepository<{info.EntityFullName}>");
        sb.AppendLine("{");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// EF Core implementation of <see cref=\"{info.InterfaceName}\"/>.");
        sb.AppendLine($"/// Follows AP.01 (sealed class default).");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"internal sealed class {info.ImplementationName}(");
        sb.AppendLine($"    DbContext context,");
        sb.AppendLine($"    ILogger<VKEFCoreRepository<{info.EntityFullName}>> logger,");
        sb.AppendLine($"    IVKCursorSerializer cursorSerializer,");
        sb.AppendLine($"    IVKEntityLifecycleProcessor processor)");
        sb.AppendLine($"    : VKEFCoreRepository<{info.EntityFullName}>(context, logger, cursorSerializer, processor), {info.InterfaceName}");
        sb.AppendLine("{");
        sb.AppendLine("}");

        ctx.AddSource($"{info.InterfaceName}.g.cs", sb.ToString());
    }

    private static void EmitDIExtensions(SourceProductionContext ctx, ImmutableArray<EntityRepositoryInfo?> list)
    {
        var valid = list.Where(x => x is not null).Select(x => x!).Distinct().ToList();
        if (valid.Count == 0) return;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine();
        sb.AppendLine("namespace VK.Blocks.Persistence.EFCore;");
        sb.AppendLine();
        sb.AppendLine("public static class GeneratedPersistenceRepositoriesExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Registers all strongly-typed entity repository implementations generated by VKPersistenceRepositoryAliasGenerator.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IServiceCollection AddGeneratedPersistenceRepositories(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var item in valid)
        {
            sb.AppendLine($"        services.TryAddScoped<{item.Namespace}.{item.InterfaceName}, {item.Namespace}.{item.ImplementationName}>();");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource("GeneratedPersistenceRepositoriesExtensions.g.cs", sb.ToString());
    }

    private record EntityRepositoryInfo(
        string Namespace,
        string EntityFullName,
        string EntityName,
        string InterfaceName,
        string ImplementationName
    );
}
