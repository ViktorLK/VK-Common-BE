using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public VKPsycheResponseBuilder Response { get; } = new();

    // ==========================================
    // 3. Active Prompt Fragments (Thread-Safe Collection)
    // ==========================================

    private readonly Lock _lockFragments = new();
    private readonly List<VKPromptFragment> _fragments = [];

    /// <summary>
    /// Gets all active prompt fragments currently accumulated in the context.
    /// </summary>
    public IReadOnlyList<VKPromptFragment> Fragments
    {
        get
        {
            lock (_lockFragments)
            {
                return [.. _fragments];
            }
        }
    }

    /// <summary>
    /// Safely adds a newly extracted prompt fragment into the active collection.
    /// </summary>
    /// <param name="fragment">The prompt fragment to add.</param>
    public void AddFragment(VKPromptFragment fragment)
    {
        VKGuard.NotNull(fragment);
        lock (_lockFragments)
        {
            _fragments.Add(fragment);
        }
    }

    /// <summary>
    /// Safely overrides the active fragments collection (typically used during truncation/pruning).
    /// </summary>
    /// <param name="fragments">The new list of active prompt fragments.</param>
    public void SetFragments(IReadOnlyList<VKPromptFragment> fragments)
    {
        VKGuard.NotNull(fragments);
        lock (_lockFragments)
        {
            _fragments.Clear();
            _fragments.AddRange(fragments);
        }
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
    // 6. Original Request Payload
    // ==========================================


    /// <summary>
    /// Gets the strongly typed arguments from the request payload.
    /// </summary>
    /// <typeparam name="T">The type of the arguments.</typeparam>
    /// <returns>The arguments if present, or null.</returns>
    public T? Args<T>() where T : class => Request.GetArgs<T>();

    /// <summary>
    /// Gets a value indicating whether to only run the prompt weaving stages, bypassing the LLM call.
    /// </summary>
    public bool IsWeaveOnly => Args<VKWeavingArgs>()?.WeaveOnly == true;

    // ==========================================
    // 7. Execution Context & Abort (Physical Pipeline)
    // ==========================================

    /// <summary>
    /// Gets the service provider for resolving dependencies during execution.
    /// </summary>
    public required IServiceProvider Services { get; init; }

    private int _isAborted;

    /// <summary>
    /// Aborts the current pipeline execution.
    /// </summary>
    public void Abort()
    {
        Interlocked.Exchange(ref _isAborted, 1);
    }

    /// <summary>
    /// Gets a value indicating whether the pipeline execution has been aborted.
    /// </summary>
    public bool IsAborted => Interlocked.CompareExchange(ref _isAborted, 0, 0) == 1;
}
