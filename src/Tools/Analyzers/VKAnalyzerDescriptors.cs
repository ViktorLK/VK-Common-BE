using Microsoft.CodeAnalysis;

namespace VK.Tools.Analyzers;

/// <summary>
/// Central registry of diagnostic descriptors enforced by VK.Tools.Analyzers.
/// </summary>
/// <remarks>
/// Global VK Diagnostic ID Allocation:
/// <list type="bullet">
/// <item><description><c>VK0001</c>–<c>VK0999</c>: <c>VK.Tools.Analyzers</c> (General architecture &amp; code governance: AP.01, CS.03, CS.06, etc.)</description></item>
/// <item><description><c>VK1000</c>–<c>VK1099</c>: <c>VK.Tools.SourceGenerators</c> — Observability (OR.01, BB.04)</description></item>
/// <item><description><c>VK1100</c>–<c>VK1199</c>: <c>VK.Tools.SourceGenerators</c> — Domain / Enum Governance</description></item>
/// <item><description><c>VK1200</c>–<c>VK1299</c>: <c>VK.Tools.SourceGenerators</c> — Authentication</description></item>
/// <item><description><c>VK2000</c>–<c>VK2999</c>: <c>VK.Tools.SourceGenerators</c> — Persistence</description></item>
/// <item><description>Legacy reserved (do not reuse): <c>VK001</c> (<c>VKDiagnostics.TypeMustBePartialRule</c>), <c>VKCORE001</c> (<c>VKValueStringBuilderUsingAnalyzer</c>)</description></item>
/// </list>
/// </remarks>
internal static class VKAnalyzerDescriptors
{
    private const string DesignCategory = "VK.Design";
    private const string DeterminismCategory = "VK.Determinism";
    private const string AsyncCategory = "VK.Async";

    /// <summary>
    /// VK0001: Non-abstract classes must be sealed by default (AP.01).
    /// </summary>
    public static readonly DiagnosticDescriptor MissingSealedModifier = new(
        id: "VK0001",
        title: "Type should be sealed",
        messageFormat: "Type '{0}' should be marked as sealed per AP.01 unless explicitly designed for inheritance",
        category: DesignCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All non-abstract classes without protected or virtual members should be sealed by default.");

    /// <summary>
    /// VK0002: Direct DateTime/DateTimeOffset system clock access is prohibited (CS.06).
    /// </summary>
    public static readonly DiagnosticDescriptor NonDeterministicClockAccess = new(
        id: "VK0002",
        title: "Avoid non-deterministic system clock access",
        messageFormat: "Direct access to '{0}' is prohibited per CS.06; inject TimeProvider instead",
        category: DeterminismCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "BuildingBlocks must inject TimeProvider instead of reading DateTime.Now, DateTime.UtcNow, DateTimeOffset.Now, or DateTimeOffset.UtcNow directly.");

    /// <summary>
    /// VK0003: Direct Guid.NewGuid() calls are prohibited (CS.06).
    /// </summary>
    public static readonly DiagnosticDescriptor NonDeterministicGuidGeneration = new(
        id: "VK0003",
        title: "Avoid direct Guid.NewGuid() calls",
        messageFormat: "Direct call to 'Guid.NewGuid()' is prohibited per CS.06; inject IVKGuidGenerator instead",
        category: DeterminismCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "BuildingBlocks must inject IVKGuidGenerator instead of calling Guid.NewGuid() directly.");

    /// <summary>
    /// VK0004: Direct System.Text.Json.JsonSerializer calls are prohibited (CS.06).
    /// </summary>
    public static readonly DiagnosticDescriptor DirectJsonSerializerUsage = new(
        id: "VK0004",
        title: "Avoid direct JsonSerializer calls",
        messageFormat: "Direct call to 'JsonSerializer.{0}' is prohibited per CS.06; inject IVKJsonSerializer instead",
        category: DeterminismCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "BuildingBlocks must inject IVKJsonSerializer instead of calling System.Text.Json.JsonSerializer directly.");

    /// <summary>
    /// VK0005: Synchronous blocking on async Task/ValueTask operations is prohibited (CS.03).
    /// </summary>
    public static readonly DiagnosticDescriptor SyncOverAsyncBlocking = new(
        id: "VK0005",
        title: "Avoid synchronous blocking on async operations",
        messageFormat: "Blocking call '{0}' on a task is prohibited per CS.03; use 'await' instead",
        category: AsyncCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling .Result, .Wait(), or .GetAwaiter().GetResult() on Task or ValueTask risks thread-pool starvation and deadlocks.");

    /// <summary>
    /// VK0006: Initializing properties with default! is prohibited (AP.01).
    /// </summary>
    public static readonly DiagnosticDescriptor DefaultBangPropertyInitializer = new(
        id: "VK0006",
        title: "Avoid default! in property initializers",
        messageFormat: "Property '{0}' must not be initialized with 'default!' per AP.01; use 'required', constructor initialization, or a nullable type",
        category: DesignCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Non-nullable properties must use the 'required' keyword, constructor initialization, or an explicit default value instead of suppressing nullability with 'default!'.");
}
