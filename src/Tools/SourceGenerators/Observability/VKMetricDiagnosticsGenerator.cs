using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.Observability;

/// <summary>
/// Source generator that automatically generates instrument fields and partial method implementations
/// for methods decorated with [VKMetricHistogram], [VKMetricCounter], and [VKMetricUpDownCounter].
/// Follows BB.04 and OR.01.
/// </summary>
[Generator]
public sealed class VKMetricDiagnosticsGenerator : IIncrementalGenerator
{
    private const string HistogramAttrFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKMetricHistogramAttribute";
    private const string CounterAttrFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKMetricCounterAttribute";
    private const string UpDownCounterAttrFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKMetricUpDownCounterAttribute";
    private const string TagAttrFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKMetricTagAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0,
                transform: static (ctx, ct) => GetMetricMethodInfo(ctx, ct))
            .Where(static m => m is not null);

        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var groupedByClass = methodDeclarations
            .Collect()
            .Combine(assemblyName);

        context.RegisterSourceOutput(groupedByClass, (spc, pair) => Execute(spc, pair.Left!, pair.Right, GetType()));
    }

    private static MetricMethodModel? GetMetricMethodInfo(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.Node is not MethodDeclarationSyntax methodSyntax)
            return null;

        var methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(methodSyntax, ct) as IMethodSymbol;
        if (methodSymbol is null || !methodSymbol.IsStatic || !methodSymbol.IsPartialDefinition)
            return null;

        var containingType = methodSymbol.ContainingType;
        if (containingType is null)
            return null;

        // Find metric attribute
        AttributeData? metricAttr = null;
        string instrumentKind = "Histogram";

        foreach (var attr in methodSymbol.GetAttributes())
        {
            var attrFullName = attr.AttributeClass?.ToDisplayString();
            if (attrFullName == HistogramAttrFullName)
            {
                metricAttr = attr;
                instrumentKind = "Histogram";
                break;
            }
            if (attrFullName == CounterAttrFullName)
            {
                metricAttr = attr;
                instrumentKind = "Counter";
                break;
            }
            if (attrFullName == UpDownCounterAttrFullName)
            {
                metricAttr = attr;
                instrumentKind = "UpDownCounter";
                break;
            }
        }

        if (metricAttr is null)
            return null;

        var metricName = metricAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
        if (string.IsNullOrWhiteSpace(metricName))
            return null;

        var unit = metricAttr.NamedArguments.FirstOrDefault(x => x.Key == "Unit").Value.Value?.ToString();
        var description = metricAttr.NamedArguments.FirstOrDefault(x => x.Key == "Description").Value.Value?.ToString();

        // Parameter inspection
        var parameters = methodSymbol.Parameters;
        IParameterSymbol? valueParam = null;
        var tagParams = new List<MetricTagModel>();

        foreach (var param in parameters)
        {
            var tagAttr = param.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == TagAttrFullName);
            if (tagAttr is not null)
            {
                var tagKey = tagAttr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? param.Name;
                tagParams.Add(new MetricTagModel(param.Name, tagKey));
            }
            else if (valueParam is null)
            {
                valueParam = param;
            }
            else
            {
                // Extra unannotated param defaults to tag with parameter name
                tagParams.Add(new MetricTagModel(param.Name, param.Name));
            }
        }

        string valueTypeName;
        string? valueParamName = null;

        if (valueParam is not null)
        {
            valueTypeName = valueParam.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            valueParamName = valueParam.Name;
        }
        else
        {
            valueTypeName = instrumentKind == "Histogram" ? "double" : "long";
        }

        var paramList = parameters.Select(p => new ParameterModel(
            p.Name,
            p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            p.RefKind == RefKind.None ? "" : "this "
        )).ToList();

        return new MetricMethodModel(
            Namespace: containingType.ContainingNamespace.ToDisplayString(),
            ClassName: containingType.Name,
            ClassModifiers: GetModifiers(containingType),
            MethodName: methodSymbol.Name,
            InstrumentKind: instrumentKind,
            MetricName: metricName!,
            Unit: unit,
            Description: description,
            ValueTypeName: valueTypeName,
            ValueParamName: valueParamName,
            Parameters: paramList,
            Tags: tagParams
        );
    }

    private static string GetModifiers(INamedTypeSymbol classSymbol)
    {
        var accessibility = classSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            _ => "internal"
        };
        var isStatic = classSymbol.IsStatic ? " static" : string.Empty;
        var isSealed = classSymbol.IsSealed ? " sealed" : string.Empty;
        return $"{accessibility}{isStatic}{isSealed}";
    }

    private static void Execute(
        SourceProductionContext ctx,
        ImmutableArray<MetricMethodModel?> methods,
        string? assemblyName,
        Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
            return;

        var validMethods = methods.Where(m => m is not null).Select(m => m!).ToList();
        if (validMethods.Count == 0)
            return;

        var groups = validMethods.GroupBy(m => $"{m.Namespace}.{m.ClassName}");

        foreach (var group in groups)
        {
            var first = group.First();
            var sb = SourceCodeBuilder.CreateWithHeader();

            sb.AppendLine("using System.Diagnostics;");
            sb.AppendLine("using System.Diagnostics.Metrics;");
            sb.AppendLine();
            sb.AppendLine($"namespace {first.Namespace};");
            sb.AppendLine();
            sb.AppendLine($"{first.ClassModifiers} partial class {first.ClassName}");
            sb.AppendLine("{");

            // 1. Generate Instrument Fields
            foreach (var m in group)
            {
                var fieldName = $"_m_{m.MethodName}";
                var unitArg = m.Unit is not null ? $", unit: \"{m.Unit}\"" : string.Empty;
                var descArg = m.Description is not null ? $", description: \"{m.Description}\"" : string.Empty;

                sb.AppendLine($"    private static readonly {m.InstrumentKind}<{m.ValueTypeName}> {fieldName} = Meter.Create{m.InstrumentKind}<{m.ValueTypeName}>(");
                sb.AppendLine($"        \"{m.MetricName}\"{unitArg}{descArg});");
                sb.AppendLine();
            }

            // 2. Generate Method Implementations
            foreach (var m in group)
            {
                var fieldName = $"_m_{m.MethodName}";
                var methodParams = string.Join(", ", m.Parameters.Select(p => $"{p.TypeName} {p.Name}"));

                sb.AppendLine($"    public static partial void {m.MethodName}({methodParams})");
                sb.AppendLine("    {");

                if (m.Tags.Count > 0)
                {
                    sb.AppendLine("        var tags = new TagList");
                    sb.AppendLine("        {");
                    for (int i = 0; i < m.Tags.Count; i++)
                    {
                        var tag = m.Tags[i];
                        var comma = i < m.Tags.Count - 1 ? "," : string.Empty;
                        sb.AppendLine($"            {{ \"{tag.TagKey}\", {tag.ParamName} }}{comma}");
                    }
                    sb.AppendLine("        };");
                    sb.AppendLine();

                    if (m.InstrumentKind == "Histogram")
                    {
                        var val = m.ValueParamName ?? "0.0";
                        sb.AppendLine($"        {fieldName}.Record({val}, tags);");
                    }
                    else
                    {
                        var val = m.ValueParamName ?? "1";
                        sb.AppendLine($"        {fieldName}.Add({val}, tags);");
                    }
                }
                else
                {
                    if (m.InstrumentKind == "Histogram")
                    {
                        var val = m.ValueParamName ?? "0.0";
                        sb.AppendLine($"        {fieldName}.Record({val});");
                    }
                    else
                    {
                        var val = m.ValueParamName ?? "1";
                        sb.AppendLine($"        {fieldName}.Add({val});");
                    }
                }

                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            ctx.AddSource($"{first.ClassName}.Metrics.g.cs", sb.ToString());
        }
    }

    private sealed record MetricMethodModel(
        string Namespace,
        string ClassName,
        string ClassModifiers,
        string MethodName,
        string InstrumentKind,
        string MetricName,
        string? Unit,
        string? Description,
        string ValueTypeName,
        string? ValueParamName,
        List<ParameterModel> Parameters,
        List<MetricTagModel> Tags
    );

    private sealed record ParameterModel(string Name, string TypeName, string Modifier);
    private sealed record MetricTagModel(string ParamName, string TagKey);
}
