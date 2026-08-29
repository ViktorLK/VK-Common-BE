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

namespace VK.Tools.SourceGenerators.Diagnostics;

/// <summary>
/// Source generator that scans all [LoggerMessage] and [VKMetric*] declarations in an assembly
/// and generates an aggregated VKObservabilityCatalog class and Markdown report capability.
/// Follows BB.04 and OR.01.
/// </summary>
[Generator]
public sealed class DiagnosticsCatalogGenerator : IIncrementalGenerator
{
    private const string LoggerMessageAttr = "Microsoft.Extensions.Logging.LoggerMessageAttribute";
    private const string HistogramAttr = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKMetricHistogramAttribute";
    private const string CounterAttr = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKMetricCounterAttribute";
    private const string UpDownCounterAttr = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKMetricUpDownCounterAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0,
                transform: static (ctx, ct) => ExtractTelemetryItem(ctx, ct))
            .Where(static item => item is not null);

        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var combined = methodSymbols.Collect().Combine(assemblyName);

        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left!, pair.Right, GetType()));
    }

    private static TelemetryItem? ExtractTelemetryItem(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.Node is not MethodDeclarationSyntax methodSyntax)
            return null;

        var methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(methodSyntax, ct) as IMethodSymbol;
        if (methodSymbol is null)
            return null;

        var containingType = methodSymbol.ContainingType;
        if (containingType is null)
            return null;

        var qualifiedMemberName = $"{containingType.Name}.{methodSymbol.Name}";

        foreach (var attr in methodSymbol.GetAttributes())
        {
            var attrFullName = attr.AttributeClass?.ToDisplayString();

            // 1. LoggerMessage
            if (attrFullName == LoggerMessageAttr)
            {
                int eventId = 0;
                string level = "Information";
                string message = "";

                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int cId)
                {
                    eventId = cId;
                }
                else
                {
                    var idArg = attr.NamedArguments.FirstOrDefault(x => x.Key == "EventId");
                    if (idArg.Value.Value is int nId) eventId = nId;
                }

                var lvlArg = attr.NamedArguments.FirstOrDefault(x => x.Key == "Level");
                if (lvlArg.Value.Value is int lvlVal)
                {
                    level = lvlVal switch
                    {
                        0 => "Trace",
                        1 => "Debug",
                        2 => "Information",
                        3 => "Warning",
                        4 => "Error",
                        5 => "Critical",
                        _ => "Information"
                    };
                }

                var msgArg = attr.NamedArguments.FirstOrDefault(x => x.Key == "Message");
                if (msgArg.Value.Value is string msgStr)
                {
                    message = msgStr;
                }

                return new TelemetryItem(
                    IsLog: true,
                    EventId: eventId,
                    LogLevel: level,
                    MessageTemplate: message,
                    MetricName: null,
                    InstrumentKind: null,
                    Unit: null,
                    Description: null,
                    MemberName: qualifiedMemberName
                );
            }

            // 2. Metrics
            if (attrFullName == HistogramAttr || attrFullName == CounterAttr || attrFullName == UpDownCounterAttr)
            {
                var kind = attrFullName == HistogramAttr ? "Histogram" : (attrFullName == CounterAttr ? "Counter" : "UpDownCounter");
                var metricName = attr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "";
                var unit = attr.NamedArguments.FirstOrDefault(x => x.Key == "Unit").Value.Value?.ToString();
                var description = attr.NamedArguments.FirstOrDefault(x => x.Key == "Description").Value.Value?.ToString();

                return new TelemetryItem(
                    IsLog: false,
                    EventId: 0,
                    LogLevel: null,
                    MessageTemplate: null,
                    MetricName: metricName,
                    InstrumentKind: kind,
                    Unit: unit,
                    Description: description,
                    MemberName: qualifiedMemberName
                );
            }
        }

        return null;
    }

    private static void Execute(
        SourceProductionContext ctx,
        ImmutableArray<TelemetryItem?> items,
        string? assemblyName,
        Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
            return;

        var validItems = items.Where(x => x is not null).Select(x => x!).ToList();
        if (validItems.Count == 0)
            return;

        var logEvents = validItems.Where(x => x.IsLog).OrderBy(x => x.EventId).ToList();
        var metrics = validItems.Where(x => !x.IsLog).OrderBy(x => x.MetricName).ToList();

        var rootNs = !string.IsNullOrEmpty(assemblyName) ? assemblyName! : VKBlocksConstants.VKBlocksPrefix;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine();
        sb.AppendLine($"namespace {rootNs};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Auto-generated compile-time Observability and Diagnostics Catalog for {assemblyName}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class VKObservabilityCatalog");
        sb.AppendLine("{");

        // 1. Log Events Array
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Registry of all declared structured log events and EventIds.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static readonly IReadOnlyList<LogEventEntry> LogEvents = new LogEventEntry[]");
        sb.AppendLine("    {");
        foreach (var l in logEvents)
        {
            var escapedMsg = (l.MessageTemplate ?? "").Replace("\"", "\"\"");
            sb.AppendLine($"        new({l.EventId}, \"{l.LogLevel}\", @\"{escapedMsg}\", \"{l.MemberName}\"),");
        }
        sb.AppendLine("    };");
        sb.AppendLine();

        // 2. Metrics Array
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Registry of all declared OpenTelemetry metrics and instruments.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static readonly IReadOnlyList<MetricEntry> Metrics = new MetricEntry[]");
        sb.AppendLine("    {");
        foreach (var m in metrics)
        {
            var unitStr = m.Unit != null ? $"\"{m.Unit}\"" : "null";
            var descStr = m.Description != null ? $"@\"{m.Description.Replace("\"", "\"\"")}\"" : "null";
            sb.AppendLine($"        new(\"{m.MetricName}\", \"{m.InstrumentKind}\", {unitStr}, {descStr}, \"{m.MemberName}\"),");
        }
        sb.AppendLine("    };");
        sb.AppendLine();

        // 3. Markdown Generator Method
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Generates a standardized Markdown catalog of all telemetry instruments and log events.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static string GenerateMarkdownReport()");
        sb.AppendLine("    {");
        sb.AppendLine("        var sb = new StringBuilder();");
        sb.AppendLine($"        sb.AppendLine(\"# Observability & Telemetry Catalog: {assemblyName}\");");
        sb.AppendLine("        sb.AppendLine();");
        sb.AppendLine("        sb.AppendLine(\"## 1. Metrics & Instruments\");");
        sb.AppendLine("        sb.AppendLine();");
        sb.AppendLine("        sb.AppendLine(\"| Metric Name | Instrument | Unit | Description | Declaring Member |\");");
        sb.AppendLine("        sb.AppendLine(\"| :--- | :--- | :--- | :--- | :--- |\");");
        sb.AppendLine("        foreach (var m in Metrics)");
        sb.AppendLine("        {");
        sb.AppendLine("            sb.AppendLine($\"| `{m.MetricName}` | {m.InstrumentKind} | {m.Unit ?? \"-\"} | {m.Description ?? \"-\"} | `{m.MemberName}` |\");");
        sb.AppendLine("        }");
        sb.AppendLine("        sb.AppendLine();");
        sb.AppendLine("        sb.AppendLine(\"## 2. Structured Log Events\");");
        sb.AppendLine("        sb.AppendLine();");
        sb.AppendLine("        sb.AppendLine(\"| EventId | Level | Message Template | Declaring Member |\");");
        sb.AppendLine("        sb.AppendLine(\"| :--- | :--- | :--- | :--- |\");");
        sb.AppendLine("        foreach (var l in LogEvents)");
        sb.AppendLine("        {");
        sb.AppendLine("            sb.AppendLine($\"| {l.EventId} | {l.Level} | `{l.MessageTemplate}` | `{l.MemberName}` |\");");
        sb.AppendLine("        }");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Data Records
        sb.AppendLine("    public sealed record LogEventEntry(int EventId, string Level, string MessageTemplate, string MemberName);");
        sb.AppendLine("    public sealed record MetricEntry(string MetricName, string InstrumentKind, string? Unit, string? Description, string MemberName);");
        sb.AppendLine("}");

        ctx.AddSource("VKObservabilityCatalog.g.cs", sb.ToString());
    }

    private sealed record TelemetryItem(
        bool IsLog,
        int EventId,
        string? LogLevel,
        string? MessageTemplate,
        string? MetricName,
        string? InstrumentKind,
        string? Unit,
        string? Description,
        string MemberName
    );
}
