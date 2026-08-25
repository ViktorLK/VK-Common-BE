using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.Diagnostics;

/// <summary>
/// Central registry of all diagnostic descriptors used by VK.Blocks Generators and Analyzers.
/// </summary>
public static class VKDiagnosticDescriptors
{
    private const string Category = $"{VKBlocksConstants.VKBlocksPrefix}.Observability";

    /// <summary>
    /// VK1001: Missing observability metrics recording.
    /// Triggered when an authorization handler doesn't call RecordEvaluation.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingObservabilityMetrics = new(
        id: "VK1001",
        title: "Missing observability metrics recording",
        messageFormat: "Authorization handler '{0}' is missing metrics recording. Call 'Stopwatch.RecordEvaluation()' to comply with OR.01.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "OR.01 requires all authorization handlers to record decision results and evaluation duration for metrics and tracing.");

    /// <summary>
    /// VK1002: Missing evaluation timing measurement.
    /// Triggered when an authorization handler doesn't use Stopwatch to track duration.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingStopwatchUsage = new(
        id: "VK1002",
        title: "Missing evaluation timing measurement",
        messageFormat: "Authorization handler '{0}' is missing Stopwatch timing. Call 'Stopwatch.StartNew()' before evaluation to comply with OR.01.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "OR.01 requires all authorization handlers to record evaluation duration for performance monitoring.");

    private const string EnumCategory = $"{VKBlocksConstants.VKBlocksPrefix}.Governance";

    /// <summary>
    /// VK1101: Enum member must have explicit integer assignment.
    /// </summary>
    public static readonly DiagnosticDescriptor EnumMemberMustHaveExplicitValue = new(
        id: "VK1101",
        title: "Enum member must have an explicit value assignment",
        messageFormat: "Enum member '{0}' must have an explicit integer value assignment (e.g. '{0} = 0').",
        category: EnumCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All enum members in VK.Blocks must have explicit numeric values to guarantee deterministic serialization and database backward compatibility.");

    /// <summary>
    /// VK1102: Enum must explicitly declare underlying type.
    /// </summary>
    public static readonly DiagnosticDescriptor EnumMustDeclareUnderlyingType = new(
        id: "VK1102",
        title: "Enum must explicitly declare an underlying type",
        messageFormat: "Enum '{0}' must explicitly declare an underlying type (e.g. 'public enum {0} : byte').",
        category: EnumCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Enums in VK.Blocks must explicitly declare an underlying type (: byte, : short, : int) to ensure deterministic memory and storage footprint.");

    private const string PersistCategory = $"{VKBlocksConstants.VKBlocksPrefix}.Persistence";

    /// <summary>
    /// VK2001: Missing AddGeneratedPersistenceRepositories() call.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingPersistenceRepositoriesRegistration = new(
        id: "VK2001",
        title: "Missing persistence repository registration",
        messageFormat: "Assembly '{0}' declares [VKPersistEntity] entities but does not call 'services.AddGeneratedPersistenceRepositories()' in DI setup.",
        category: PersistCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Assemblies declaring [VKPersistEntity] must register generated strongly-typed repositories via services.AddGeneratedPersistenceRepositories().");

    /// <summary>
    /// VK2002: Missing AddGeneratedModelContributors() call.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingModelContributorsRegistration = new(
        id: "VK2002",
        title: "Missing model and convention contributor registration",
        messageFormat: "Assembly '{0}' declares [VKPersistEntity] entities but does not call 'services.AddGeneratedModelContributors()' in DI setup.",
        category: PersistCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Assemblies declaring [VKPersistEntity] must register model configuration and type conventions via services.AddGeneratedModelContributors().");
}

