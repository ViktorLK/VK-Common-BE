using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.DependencyInjection;

/// <summary>
/// Source generator that automatically creates an IVKOptionsProvider and its default implementation
/// for classes marked with [VKOptionsProvider].
/// </summary>
[Generator]
public sealed class VKOptionsProviderGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "VK.Blocks.Core.VKOptionsProviderAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: GetTargetClass)
            .WhereNotNull();

        context.RegisterSourceOutput(classTargets, EmitSource);
    }

    private static OptionsTarget? GetTargetClass(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, ct) as INamedTypeSymbol;

        if (symbol is null)
        {
            return null;
        }

        var attribute = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == AttributeFullName ||
            a.AttributeClass?.Name == "VKOptionsProviderAttribute" ||
            a.AttributeClass?.Name == "VKOptionsProvider");

        if (attribute is null)
        {
            var featureAttribute = symbol.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKFeatureAttribute" ||
                a.AttributeClass?.Name == "VKFeatureAttribute" ||
                a.AttributeClass?.Name == "VKFeature");

            if (featureAttribute is null)
            {
                return null;
            }

            var generateArgs = featureAttribute.NamedArguments
                .FirstOrDefault(n => n.Key == "GenerateArgs")
                .Value.Value as bool? ?? false;

            if (!generateArgs)
            {
                return null;
            }
        }

        return new OptionsTarget(
            Namespace: symbol.ContainingNamespace.ToDisplayString(),
            ClassName: symbol.Name
        );
    }

    private static void EmitSource(SourceProductionContext ctx, OptionsTarget target)
    {
        var baseName = ExtractBaseName(target.ClassName);
        var interfaceName = $"IVK{baseName}OptionsProvider";
        var implementationName = $"VK{baseName}OptionsDefaultProvider";

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated provider interface for <see cref=\"{target.ClassName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public partial interface {interfaceName} : IVKOptionsProvider<{target.ClassName}>;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated default implementation of <see cref=\"{interfaceName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed class {implementationName}(IOptions<{target.ClassName}> options) : {interfaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IOptions<{target.ClassName}> _options = options;");
        sb.AppendLine();
        sb.AppendLine($"    public {target.ClassName} GetOptions() => _options.Value;");
        sb.AppendLine("}");

        ctx.AddSource($"{interfaceName}.g.cs", sb.ToString());
    }

    private static string ExtractBaseName(string className)
    {
        if (className.StartsWith("VK"))
        {
            return className.Substring(2);
        }

        return className;
    }

    private sealed record OptionsTarget(string Namespace, string ClassName);
}
