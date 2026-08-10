using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Execution arguments passed via VKPsycheRequest.WithArgs to specify Eidos contract governance coordinates.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKAIEidosRequestArgs
{
    /// <summary>
    /// Gets the business scenario identifier to resolve contract for.
    /// Optional if TargetType is provided.
    /// </summary>
    public string? Scenario { get; internal init; }

    /// <summary>
    /// Gets an optional explicit contract overriding DB/Registry resolution.
    /// </summary>
    public VKAIEidosResponseContract? ExplicitContract { get; internal init; }

    /// <summary>
    /// Gets an optional target DTO type for strong-typed binding.
    /// </summary>
    public Type? TargetType { get; internal init; }

    /// <summary>
    /// Gets a value indicating whether to automatically inject NarrativeText field into the projected JSON schema.
    /// </summary>
    public bool InjectNarrativeField { get; internal init; } = false;

    /// <summary>
    /// Gets a value indicating whether narrative responses can be split into multiple segment array elements. Default is true.
    /// </summary>
    public bool AllowNarrativeSegmentation { get; internal init; } = true;

    /// <summary>
    /// Gets an optional explicit preferred expression mode (e.g. ToolCall, StructuredOutput, PromptJson).
    /// </summary>
    public VKAIEidosExpressionMode? PreferredMode { get; internal init; }

    internal VKAIEidosRequestArgs() { }

    /// <summary>
    /// Creates request args for strong-typed DTO governance, with optional DB scenario overlay.
    /// </summary>
    public static VKAIEidosRequestArgs FromType<T>(string? scenario = null, VKAIEidosExpressionMode? preferredMode = null)
        => new() { TargetType = typeof(T), Scenario = scenario, PreferredMode = preferredMode };

    /// <summary>
    /// Creates request args for target DTO type governance, with optional DB scenario overlay.
    /// </summary>
    public static VKAIEidosRequestArgs FromType(Type targetType, string? scenario = null, VKAIEidosExpressionMode? preferredMode = null)
        => new() { TargetType = VKGuard.NotNull(targetType), Scenario = scenario, PreferredMode = preferredMode };

    /// <summary>
    /// Creates request args for DB/Registry dynamic scenario governance without strong-typed DTO.
    /// </summary>
    public static VKAIEidosRequestArgs FromScenario(string scenario, VKAIEidosExpressionMode? preferredMode = null)
        => new() { Scenario = VKGuard.NotNullOrWhiteSpace(scenario), PreferredMode = preferredMode };

    /// <summary>
    /// Creates request args using an explicit override contract, bypassing DB resolution.
    /// </summary>
    public static VKAIEidosRequestArgs FromExplicitContract(
        VKAIEidosResponseContract contract,
        Type? targetType = null,
        VKAIEidosExpressionMode? preferredMode = null)
        => new() { ExplicitContract = VKGuard.NotNull(contract), TargetType = targetType, PreferredMode = preferredMode };
}
