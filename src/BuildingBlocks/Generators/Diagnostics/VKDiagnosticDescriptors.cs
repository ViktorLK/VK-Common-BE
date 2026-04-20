using Microsoft.CodeAnalysis;
using VK.Blocks.Generators.Utilities;

namespace VK.Blocks.Generators.Diagnostics;

/// <summary>
/// Central registry of all diagnostic descriptors used by VK.Blocks Generators and Analyzers.
/// </summary>
public static class VKDiagnosticDescriptors
{
    private const string Category = VKBlocksConstants.VKBlocksPrefix + "Observability";

    /// <summary>
    /// VK1001: Missing observability metrics recording.
    /// Triggered when an authorization handler doesn't call RecordEvaluation.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingObservabilityMetrics = new(
        id: "VK1001",
        title: "Missing observability metrics recording",
        messageFormat: "Authorization handler '{0}' is missing metrics recording. Call 'Stopwatch.RecordEvaluation()' to comply with Rule 6.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Rule 6 requires all authorization handlers to record decision results and evaluation duration for metrics and tracing.");

    /// <summary>
    /// VK1002: Missing evaluation timing measurement.
    /// Triggered when an authorization handler doesn't use Stopwatch to track duration.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingStopwatchUsage = new(
        id: "VK1002",
        title: "Missing evaluation timing measurement",
        messageFormat: "Authorization handler '{0}' is missing Stopwatch timing. Call 'Stopwatch.StartNew()' before evaluation to comply with Rule 6.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Rule 6 requires all authorization handlers to record evaluation duration for performance monitoring.");
}
