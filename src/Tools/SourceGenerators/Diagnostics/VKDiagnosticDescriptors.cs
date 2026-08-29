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

    /// <summary>
    /// VK1010: Duplicate EventId detected across LoggerMessage declarations.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateEventId = new(
        id: "VK1010",
        title: "Duplicate EventId detected",
        messageFormat: "Duplicate EventId '{0}' detected on method '{1}'. EventId '{0}' was already declared on '{2}'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "OR.01 and BB.04 require every log event within a module to have a unique EventId to prevent telemetry collisions.");

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
    /// VK2001: Missing AddGeneratedAggregateRepositories() call.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingAggregateRepositoriesRegistration = new(
        id: "VK2001",
        title: "Missing aggregate repository registration",
        messageFormat: "Assembly '{0}' declares [VKPersistEntity] entities but does not call 'services.AddGeneratedAggregateRepositories()' in DI setup.",
        category: PersistCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Assemblies declaring [VKPersistEntity] must register generated strongly-typed aggregate repositories via services.AddGeneratedAggregateRepositories().");

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

    /// <summary>
    /// VK2010: Nesting depth in [VKPersistEntity] exceeds 1-level limit.
    /// </summary>
    public static readonly DiagnosticDescriptor NestingDepthExceedsLimit = new(
        id: "VK2010",
        title: "Nesting depth in [VKPersistEntity] exceeds 1-level limit",
        messageFormat: "Property '{0}' in [VKPersistEntity] exceeds the 1-level nesting limit. Decompose nested models into independent Aggregate Roots per DDD small aggregate principles or implement 'OnMapOntoCustom' for manual handling.",
        category: PersistCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "VKPersistEntity supports at most 1 level of child collection projection or value object flattening to enforce DDD small aggregate boundaries.");

    /// <summary>
    /// VK2011: Child entity in ProjectBy missing parent foreign key.
    /// </summary>
    public static readonly DiagnosticDescriptor ProjectByMissingForeignKey = new(
        id: "VK2011",
        title: "Child entity in ProjectBy missing parent foreign key",
        messageFormat: "Child entity '{0}' in ProjectBy collection '{1}' must declare a foreign key property pointing to parent '{2}'.",
        category: PersistCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every child entity in a ProjectBy collection must declare a foreign key property matching the parent entity/domain ID.");

    /// <summary>
    /// VK2012: Child entity in ProjectBy missing primary or discriminator key.
    /// </summary>
    public static readonly DiagnosticDescriptor ProjectByMissingKey = new(
        id: "VK2012",
        title: "Child entity in ProjectBy missing primary or discriminator key",
        messageFormat: "Child entity '{0}' in ProjectBy collection '{1}' must declare an 'Id' property or [VKPersistKey] attribute(s) to enable differential synchronization.",
        category: PersistCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Child entities in ProjectBy collections must declare an Id property or [VKPersistKey] attributes to support deterministic Delete/Update/Insert diff synchronization.");

    /// <summary>
    /// VK2013: FlattenBy value object property missing on persistence entity.
    /// </summary>
    public static readonly DiagnosticDescriptor FlattenByPropertyMissingOnEntity = new(
        id: "VK2013",
        title: "FlattenBy value object property missing on persistence entity",
        messageFormat: "Value object property '{0}.{1}' in FlattenBy does not have a matching column on entity '{2}'.",
        category: PersistCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All properties of a flattened value object in FlattenBy must have corresponding database columns on the persistence entity.");
}

