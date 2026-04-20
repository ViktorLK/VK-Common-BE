using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Blocks.Generators.Extensions;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.DependencyInjection;

/// <summary>
/// Source generator that automatically injects the 'Instance' property and 'IVKBlockMarkerProvider' interface
/// into any partial class implementing IVKBlockMarker.
/// </summary>
[Generator]
public sealed class VKBlockMarkerGenerator : IIncrementalGenerator
{
    private const string TargetInterfaceFullName = VKBlocksConstants.VKBlocksPrefix + "Core.DependencyInjection.IVKBlockMarker";
    private const string ProviderInterfaceFullName = VKBlocksConstants.VKBlocksPrefix + "Core.DependencyInjection.IVKBlockMarkerProvider";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var classTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { BaseList: not null },
                transform: GetTargetClass)
            .WhereNotNull();

        context.RegisterSourceOutput(
            classTargets.Combine(assemblyName),
            (ctx, pair) => EmitSource(ctx, pair.Left, pair.Right, this.GetType()));
    }

    private static BlockTarget? GetTargetClass(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, ct) as INamedTypeSymbol;

        if (symbol is null || symbol.IsAbstract)
        {
            return null;
        }

        // Check if it implements IVKBlockMarker
        if (!symbol.AllInterfaces.Any(i => i.ToDisplayString() == TargetInterfaceFullName))
        {
            return null;
        }

        // Implementation must be a partial class to allow injection
        var isPartial = classDeclaration.Modifiers.Any(m => m.Text == "partial");

        return new BlockTarget(
            Namespace: symbol.ContainingNamespace.ToDisplayString(),
            ClassName: symbol.Name,
            IsPartial: isPartial
        );
    }

    private static void EmitSource(SourceProductionContext ctx, BlockTarget target, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        if (!target.IsPartial)
        {
            // Note: In a full industrial implementation, we would report a diagnostic warning here
            // informing the developer that the marker class must be partial.
            return;
        }

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using VK.Blocks.Core.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"partial class {target.ClassName} : IVKBlockMarkerProvider<{target.ClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Automatically injected singleton instance for recursive dependency validation (Zero-Reflection).");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static IVKBlockMarker Instance {{ get; }} = new {target.ClassName}();");
        sb.AppendLine("}");

        ctx.AddSource($"{target.ClassName}.Instance.g.cs", sb.ToString());
    }

    private sealed record BlockTarget(string Namespace, string ClassName, bool IsPartial);
}
