using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.DependencyInjection;

/// <summary>
/// Source generator that automatically creates an "Args" record for classes marked with [VKGenerateArgs].
/// These Args records are used for request-scoped overrides of global options.
/// </summary>
[Generator]
public sealed class VKArgsGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "VK.Blocks.Core.VKGenerateArgsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, _) => TransformToTarget(ctx))
            .Where(static t => t is not null);

        context.RegisterSourceOutput(targets, (ctx, target) => EmitArgsSource(ctx, target!));
    }

    private static ArgsTargetInfo? TransformToTarget(GeneratorAttributeSyntaxContext ctx)
    {
        var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
        
        var properties = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public 
                        && !p.IsStatic 
                        && !p.IsReadOnly) 
            .Select(p => new PropertyInfo(
                Name: p.Name,
                Type: p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier)),
                // Ensure everything becomes nullable in Args
                IsAlreadyNullable: p.Type.NullableAnnotation == NullableAnnotation.Annotated || p.Type.ToDisplayString().EndsWith("?")
            ))
            .ToImmutableArray();

        return new ArgsTargetInfo(
            Namespace: symbol.ContainingNamespace.ToDisplayString(),
            ClassName: symbol.Name,
            Properties: properties
        );
    }

    private static void EmitArgsSource(SourceProductionContext ctx, ArgsTargetInfo target)
    {
        var baseName = ExtractBaseName(target.ClassName);
        var argsClassName = $"{baseName}Args";
        var extensionsClassName = $"{argsClassName}Extensions";

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated request-scoped arguments for <see cref=\"{target.ClassName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public partial record {argsClassName}");
        sb.AppendLine("{");

        foreach (var prop in target.Properties)
        {
            var propType = prop.Type;
            
            // If the property type is another "Options" class, use the corresponding "Args" class
            if (propType.EndsWith("Options"))
            {
                propType = propType.Substring(0, propType.Length - "Options".Length) + "Args";
            }
            else if (propType.EndsWith("Options?"))
            {
                propType = propType.Substring(0, propType.Length - "Options?".Length) + "Args?";
            }

            var nullableType = prop.IsAlreadyNullable || propType.EndsWith("?") ? propType : $"{propType}?";
            sb.AppendLine($"    public {nullableType} {prop.Name} {{ get; init; }}");
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Extension methods for <see cref=\"{argsClassName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static partial class {extensionsClassName}");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Merges the local arguments with the global options.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static {target.ClassName} Merge(this {argsClassName}? args, {target.ClassName} options)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (args is null) return options;");
        sb.AppendLine();
        sb.AppendLine("        return options with");
        sb.AppendLine("        {");

        foreach (var prop in target.Properties)
        {
            if (prop.Type.EndsWith("Options") || prop.Type.EndsWith("Options?"))
            {
                sb.AppendLine($"            {prop.Name} = args.{prop.Name}.Merge(options.{prop.Name}),");
            }
            else
            {
                sb.AppendLine($"            {prop.Name} = args.{prop.Name} ?? options.{prop.Name},");
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{argsClassName}.g.cs", sb.ToString());
    }

    private static string ExtractBaseName(string className)
    {
        if (className.EndsWith("Options"))
        {
            return className.Substring(0, className.Length - "Options".Length);
        }

        return className;
    }

    private record ArgsTargetInfo(
        string Namespace,
        string ClassName,
        ImmutableArray<PropertyInfo> Properties
    );

    private record PropertyInfo(
        string Name,
        string Type,
        bool IsAlreadyNullable
    );
}
