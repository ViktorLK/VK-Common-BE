using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public immutable request payload representing the single source of business context for prompt weaving.
/// Complies with AP.01 (sealed record) and AP.07 (non-intrusive explicit context).
/// </summary>
public sealed record VKPsycheRequest
{
    // ==========================================
    // 1. Turn & Session Flow
    // ==========================================

    /// <summary>
    /// Gets the fresh input message provided by the user in this turn.
    /// </summary>
    public required string UserInput { get; init; }

    /// <summary>
    /// Gets the unique session identifier to track dialogue history. Default is <see cref="VKSessionId.Empty"/>.
    /// </summary>
    public VKSessionId SessionId { get; init; } = VKSessionId.Empty;

    /// <summary>
    /// Gets the optional correlation ID to trace this weaving execution through logging and metrics.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets a value indicating whether to only assemble the prompt tapestry without triggering downstream LLM invocation. Default is false.
    /// </summary>
    public bool WeaveOnly { get; init; } = false;

    /// <summary>
    /// Gets the optional timestamp when this request was originally initiated.
    /// If null, defaults to pipeline initiation timestamp.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    // ==========================================
    // 2. Cognitive Identity & Entity Coordinates
    // ==========================================

    /// <summary>
    /// Gets the optional profile identifier explicitly provided by the caller.
    /// </summary>
    public VKProfileId? ProfileId { get; init; }

    /// <summary>
    /// Gets the list of Directive identifiers (0..N: supports global safety, policy, and task rules).
    /// </summary>
    public IReadOnlyList<VKDirectiveId> DirectiveIds { get; init; } = [];

    /// <summary>
    /// Gets the list of Persona identifiers (1..N: supports single agent or multi-agent collaboration).
    /// </summary>
    public IReadOnlyList<VKPersonaId> PersonaIds { get; init; } = [];

    /// <summary>
    /// Gets the list of explicitly specified Knowledge identifiers (0..N).
    /// </summary>
    public IReadOnlyList<VKKnowledgeId> KnowledgeIds { get; init; } = [];

    /// <summary>
    /// Gets the list of explicitly specified Pattern identifiers (0..N).
    /// </summary>
    public IReadOnlyList<VKPatternId> PatternIds { get; init; } = [];

    // ==========================================
    // 3. Dynamic Extension Arguments
    // ==========================================

    private ImmutableDictionary<Type, object> Args { get; init; } = ImmutableDictionary<Type, object>.Empty;

    public VKPsycheRequest WithArgs<T>(T args) where T : class
    {
        VKGuard.NotNull(args);
        return this with { Args = Args.SetItem(typeof(T), args) };
    }

    public T? GetArgs<T>() where T : class
        => Args.TryGetValue(typeof(T), out object? v) ? (T)v : null;

    internal IEnumerable<object> GetAllArgs() => Args.Values;
}
