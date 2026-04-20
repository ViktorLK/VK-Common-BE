using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Observability.Internal;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Observability;

/// <summary>
/// Source generator that automatically adds ActivitySource and Meter to classes decorated with [VKBlockDiagnostics].
/// </summary>
[Generator]
public sealed class VKBlockDiagnosticsGenerator : IIncrementalGenerator
{
    private const string GenericAttributeFullName = VKBlocksConstants.VKBlocksPrefix + "Core.Diagnostics.VKBlockDiagnosticsAttribute`1";
    private const string AppAttributeFullName = VKBlocksConstants.VKBlocksPrefix + "Core.Diagnostics.VKAppDiagnosticsAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var genericClassTargets = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                  GenericAttributeFullName,
                  predicate: IsPartialClass,
                  transform: TransformToTarget)
                .WhereNotNull();

        var appClassTargets = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                  AppAttributeFullName,
                  predicate: IsPartialClass,
                  transform: TransformToTarget)
                .WhereNotNull();

        // Combine both target sources
        context.RegisterSourceOutput(
            genericClassTargets.Combine(assemblyName),
            (ctx, pair) => EmitAttributeSource(ctx, pair.Left, pair.Right, this.GetType()));

        context.RegisterSourceOutput(
            appClassTargets.Combine(assemblyName),
            (ctx, pair) => EmitAttributeSource(ctx, pair.Left, pair.Right, this.GetType()));
    }

    private static bool IsPartialClass(SyntaxNode node, CancellationToken _)
        => node is ClassDeclarationSyntax cls &&
           cls.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword));

    private static BlockInfo? TransformToTarget(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken _)
    {
        var attr = ctx.Attributes[0];
        string blockExpression;
        string versionExpression;
        string? description = null;

        // Check if it's the generic version: [VKBlockDiagnostics<TBlock>]
        if (attr.AttributeClass is { IsGenericType: true, TypeArguments.Length: 1 })
        {
            var typeArg = attr.AttributeClass.TypeArguments[0];
            var typeName = typeArg.ToDisplayString();

            blockExpression = $"{typeName}.Instance.ActivitySourceName";
            versionExpression = $"{typeName}.Instance.Version";
        }
        else
        {
            // Lightweight/App-based (non-generic): [VKAppDiagnostics("name", Version = "1.0.0")]
            var appName = attr.ConstructorArguments.FirstOrDefault().Value?.ToString();
            var appVersion = attr.NamedArguments
                                .FirstOrDefault(x => x.Key == "Version").Value.Value?.ToString()
                            ?? "1.0.0";
            description = attr.NamedArguments
                                .FirstOrDefault(x => x.Key == "Description").Value.Value?.ToString();

            if (appName is null)
                return null;

            blockExpression = $"\"{appName}\"";
            versionExpression = $"\"{appVersion}\"";
        }

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

        // Reconstruct access + static modifier
        var accessibility = classSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "internal"
        };
        var isStatic = classSymbol.IsStatic ? " static" : string.Empty;
        var modifiers = $"{accessibility}{isStatic}";

        return new BlockInfo(
            Namespace: classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: classSymbol.Name,
            Identifier: blockExpression,
            Version: versionExpression,
            Modifiers: modifiers,
            Description: description
        );
    }

    private static void EmitAttributeSource(SourceProductionContext ctx, BlockInfo info, string? assemblyName, Type generatorType)
    {
        // Guard against execution in non-VK.Blocks assemblies (Rule: Global Guard)
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Diagnostics.Metrics;");
        sb.AppendLine();
        sb.AppendLine($"namespace {info.Namespace};");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(info.Description))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {info.Description}");
            sb.AppendLine("/// </summary>");
        }

        sb.AppendLine($"{info.Modifiers} partial class {info.ClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    public static readonly ActivitySource Source");
        sb.AppendLine($"        = new({info.Identifier}, {info.Version});");
        sb.AppendLine();
        sb.AppendLine("    public static readonly Meter Meter");
        sb.AppendLine($"        = new({info.Identifier}, {info.Version});");
        sb.AppendLine("}");

        ctx.AddSource($"{info.ClassName}.g.cs", sb.ToString());
    }
}


