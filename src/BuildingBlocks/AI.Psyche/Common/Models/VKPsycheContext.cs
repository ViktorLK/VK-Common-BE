using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Execution payload context flowing through the general orchestrator stages and weaving tasks.
/// Acts as the unified, thread-safe state container for prompt assembly and lifecycle management.
/// </summary>
public sealed class VKPsycheContext
{
    /// <summary>
    /// Gets the original request payload containing overrides and arguments.
    /// </summary>
    public required VKPsycheRequest Request { get; init; }

    /// <summary>
    /// Gets the mutable response builder for accumulating execution results.
    /// </summary>
    public VKPsycheResponseBuilder ResponseBuilder { get; } = new();

    /// <summary>
    /// Gets the unique correlation ID to trace this execution across logging, profiling, and diagnostics.
    /// Guaranteed to be non-empty.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Gets the timestamp when this pipeline execution was initiated.
    /// Guaranteed to be resolved.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }


    /// <summary>
    /// Gets a value indicating whether the current execution is running under Sandbox trial mode.
    /// Downstream consumers (Efferent state mutations, Corpus usage tracking, Engram memory consolidation) MUST inspect this to strictly skip permanent DB side-effects.
    /// </summary>
    public bool IsSandbox => State<VKSessionThread>()?.Mode == VKSessionMode.Sandbox;

    // ==========================================
    // 3. Structured Prompt Containers (Thread-Safe Lock-Free Dual Engine)
    // ==========================================

    private ImmutableList<VKPromptSegment> _segments = [];
    private ImmutableList<VKEchoFragment> _echoes = [];

    /// <summary>
    /// Gets all active prompt segments across all tiers (Core Tapestry).
    /// </summary>
    public IReadOnlyList<VKPromptSegment> Segments => _segments;

    /// <summary>
    /// Gets all active conversation history dialogue turns (Core Pillar 3 - Timeline).
    /// </summary>
    public IReadOnlyList<VKEchoFragment> Echoes => _echoes;

    /// <summary>
    /// Safely adds a prompt segment into the active tapestry collection.
    /// Uses CAS (Compare-And-Swap) for atomic, lock-free thread safety.
    /// </summary>
    /// <param name="segment">The prompt segment to add.</param>
    public void AddSegment(VKPromptSegment segment)
    {
        VKGuard.NotNull(segment);
        if (string.IsNullOrWhiteSpace(segment.Content))
        {
            return;
        }

        ImmutableList<VKPromptSegment> initial, updated;
        do
        {
            initial = _segments;
            updated = initial.Add(segment);
        }
        while (Interlocked.CompareExchange(ref _segments, updated, initial) != initial);
    }

    /// <summary>
    /// Overrides the active prompt segments collection.
    /// </summary>
    /// <param name="segments">The new list of active prompt segments.</param>
    public void SetSegments(IReadOnlyList<VKPromptSegment> segments)
    {
        VKGuard.NotNull(segments);
        Interlocked.Exchange(ref _segments, [.. segments]);
    }

    /// <summary>
    /// Adds a conversation history dialogue turn into the context under the Echo pillar.
    /// Uses CAS (Compare-And-Swap) for atomic, lock-free thread safety.
    /// </summary>
    /// <param name="fragment">The dialogue turn fragment to add.</param>
    public void AddEcho(VKEchoFragment fragment)
    {
        VKGuard.NotNull(fragment);
        if (string.IsNullOrWhiteSpace(fragment.Content))
        {
            return;
        }

        ImmutableList<VKEchoFragment> initial, updated;
        do
        {
            initial = _echoes;
            updated = initial.Add(fragment);
        }
        while (Interlocked.CompareExchange(ref _echoes, updated, initial) != initial);
    }

    /// <summary>
    /// Overrides the retained dialogue turns (typically used during token budget truncation).
    /// </summary>
    /// <param name="echoes">The retained dialogue turns.</param>
    public void SetEchoes(IReadOnlyList<VKEchoFragment> echoes)
    {
        VKGuard.NotNull(echoes);
        Interlocked.Exchange(ref _echoes, [.. echoes]);
    }

    // ==========================================
    // 5. Extensibility Container
    // ==========================================

    private readonly ConcurrentDictionary<Type, object> _states = new();

    /// <summary>
    /// Attaches an extensibility object to this context for downstream stages.
    /// </summary>
    /// <typeparam name="T">The type of the extension.</typeparam>
    /// <param name="value">The extension instance to store.</param>
    public void SetState<T>(T value) where T : class
    {
        _states[typeof(T)] = VKGuard.NotNull(value);
    }

    /// <summary>
    /// Retrieves a previously attached extensibility object from this context.
    /// </summary>
    /// <typeparam name="T">The type of the extension to retrieve.</typeparam>
    /// <returns>The stored extension instance, or null if not found.</returns>
    public T? State<T>() where T : class
        => _states.TryGetValue(typeof(T), out object? v) ? (T)v : null;


    // ==========================================
    // 6. Request Payload & Argument Overrides
    // ==========================================

    private readonly ConcurrentDictionary<Type, object> _argsOverrides = new();

    /// <summary>
    /// Sets or overrides runtime arguments for downstream pipeline stages.
    /// </summary>
    /// <typeparam name="T">The type of the arguments.</typeparam>
    /// <param name="args">The argument instance.</param>
    public void SetArgs<T>(T args) where T : class
    {
        _argsOverrides[typeof(T)] = VKGuard.NotNull(args);
    }

    /// <summary>
    /// Gets the strongly typed arguments from runtime overrides or the request payload.
    /// </summary>
    /// <typeparam name="T">The type of the arguments.</typeparam>
    /// <returns>The arguments if present, or null.</returns>
    public T? Args<T>() where T : class
        => _argsOverrides.TryGetValue(typeof(T), out object? v) ? (T)v : Request.GetArgs<T>();

    /// <summary>
    /// Gets a value indicating whether to only run the prompt weaving stages, bypassing the LLM call.
    /// </summary>
    public bool IsWeaveOnly => Request.WeaveOnly;

    // ==========================================
    // 7. Execution Context & Abort (Physical Pipeline)
    // ==========================================

    /// <summary>
    /// Gets the service provider for resolving dependencies during execution.
    /// </summary>
    public required IServiceProvider Services { get; init; }

    private int _isAborted;
    private int _isCompleted;

    /// <summary>
    /// Aborts the current pipeline execution with failure.
    /// </summary>
    public void Abort()
    {
        Interlocked.Exchange(ref _isAborted, 1);
    }

    /// <summary>
    /// Gets a value indicating whether the pipeline execution has been aborted.
    /// </summary>
    public bool IsAborted => Interlocked.CompareExchange(ref _isAborted, 0, 0) == 1;

    /// <summary>
    /// Marks the pipeline execution as successfully completed early.
    /// Skips terminal LLM invocation and subsequent after stages, returning success.
    /// </summary>
    public void Complete()
    {
        Interlocked.Exchange(ref _isCompleted, 1);
    }

    /// <summary>
    /// Gets a value indicating whether the pipeline execution has completed early.
    /// </summary>
    public bool IsCompleted => Interlocked.CompareExchange(ref _isCompleted, 0, 0) == 1;
}
