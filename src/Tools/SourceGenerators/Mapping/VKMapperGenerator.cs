using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Mapping;

/// <summary>
/// Source generator that generates mapping logic for classes decorated with [VKMapper].
/// </summary>
[Generator]
public sealed class VKMapperGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "VK.Blocks.Core.VKMapperAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformToTarget(ctx))
            .Where(static t => t is not null);

        context.RegisterSourceOutput(targets, (ctx, target) => EmitMappingSource(ctx, target!));
    }

    private static MappingTargetInfo? TransformToTarget(GeneratorAttributeSyntaxContext ctx)
    {
        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attributeData = ctx.Attributes[0];

        ITypeSymbol? sourceType = null;
        ITypeSymbol? destinationType = null;

        if (attributeData.ConstructorArguments.Length == 2)
        {
            sourceType = attributeData.ConstructorArguments[0].Value as ITypeSymbol;
            destinationType = attributeData.ConstructorArguments[1].Value as ITypeSymbol;
        }
        else
        {
            sourceType = attributeData.NamedArguments.FirstOrDefault(x => x.Key == "SourceType").Value.Value as ITypeSymbol;
            destinationType = attributeData.NamedArguments.FirstOrDefault(x => x.Key == "DestinationType").Value.Value as ITypeSymbol;
        }

        if (sourceType is null || destinationType is null)
            return null;

        return new MappingTargetInfo(
            Namespace: classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: classSymbol.Name,
            Modifiers: GetModifiers(classSymbol),
            SourceType: sourceType,
            DestinationType: destinationType
        );
    }

    private static string GetModifiers(INamedTypeSymbol symbol)
    {
        var accessibility = symbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            _ => "internal"
        };
        return $"{accessibility}{(symbol.IsStatic ? " static" : "")} partial";
    }

    private static void EmitMappingSource(SourceProductionContext ctx, MappingTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"{target.Modifiers} class {target.ClassName} : IVKMapper<{target.SourceType.ToDisplayString()}, {target.DestinationType.ToDisplayString()}>");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <inheritdoc />");
        sb.AppendLine($"    public {target.DestinationType.ToDisplayString()} Map({target.SourceType.ToDisplayString()} source)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (source is null) return default!;");
        sb.AppendLine();
        sb.AppendLine($"        var destination = new {target.DestinationType.ToDisplayString()}");
        sb.AppendLine("        {");

        var sourceProps = target.SourceType.GetMembers().OfType<IPropertySymbol>().ToImmutableArray();
        var destProps = target.DestinationType.GetMembers().OfType<IPropertySymbol>().ToImmutableArray();

        foreach (var destProp in destProps)
        {
            if (destProp.IsReadOnly || destProp.IsStatic)
                continue;

            var sourceProp = sourceProps.FirstOrDefault(p => p.Name == destProp.Name && p.Type.Equals(destProp.Type, SymbolEqualityComparer.Default));
            if (sourceProp != null)
            {
                sb.AppendLine($"            {destProp.Name} = source.{sourceProp.Name},");
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine();
        sb.AppendLine("        OnMapping(source, destination);");
        sb.AppendLine();
        sb.AppendLine("        return destination;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    {(target.Modifiers.Contains("static") ? "static " : "")}partial void OnMapping({target.SourceType.ToDisplayString()} source, {target.DestinationType.ToDisplayString()} destination);");
        sb.AppendLine("}");

        ctx.AddSource($"{target.ClassName}.g.cs", sb.ToString());
    }

    private record MappingTargetInfo(
        string Namespace,
        string ClassName,
        string Modifiers,
        ITypeSymbol SourceType,
        ITypeSymbol DestinationType
    );
}
