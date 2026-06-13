using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public immutable request payload representing the input coordinates for prompt weaving.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKPsycheRequest
{
    /// <summary>
    /// Gets the target Persona identifier that this context uses to retrieve prompt configurations.
    /// </summary>
    public required VKPersonaId PersonaId { get; init; }

    /// <summary>
    /// Gets the unique session identifier to track dialogue history.
    /// </summary>
    public required VKSessionId SessionId { get; init; }

    /// <summary>
    /// Gets the fresh input message provided by the user in this turn.
    /// </summary>
    public required string UserInput { get; init; }

    /// <summary>
    /// Gets the optional correlation ID to trace this weaving execution through logging and metrics.
    /// </summary>
    public string? CorrelationId { get; init; }

    private readonly Dictionary<Type, object> _args = [];

    public VKPsycheRequest WithArgs<T>(T args) where T : class
    {
        _args[typeof(T)] = VKGuard.NotNull(args);
        return this;
    }

    public T? GetArgs<T>() where T : class
        => _args.TryGetValue(typeof(T), out object? v) ? (T)v : null;

    internal IEnumerable<object> GetAllArgs() => _args.Values;
}
