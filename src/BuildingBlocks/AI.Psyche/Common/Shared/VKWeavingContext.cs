using System;
using System.Collections.Generic;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Execution payload context flowing through the general orchestrator stages and weaving tasks.
/// Acts as the unified, thread-safe state container for prompt assembly and lifecycle management.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKWeavingContext
{
    // ==========================================
    // 1. Identification & Correlation (L1)
    // ==========================================

    /// <summary>
    /// Gets the target Persona identifier that this context uses to retrieve prompt configurations.
    /// </summary>
    public required string PersonaId { get; init; }

    /// <summary>
    /// Gets the unique session identifier to track dialogue history.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the correlation ID to trace this weaving execution through logging and metrics.
    /// Default is empty string if not provided (should be initialized by pipeline).
    /// </summary>
    public required string CorrelationId { get; init; }

    // ==========================================
    // 2. Input Parameters & Output Tapestry
    // ==========================================

    /// <summary>
    /// Gets the fresh input message provided by the user in this turn.
    /// </summary>
    public required string UserInput { get; init; }

    /// <summary>
    /// Gets runtime overrides or arguments to adjust layout, budgets, and disabled tiers in this turn.
    /// </summary>
    public VKWeavingArgs? Args { get; init; }

    /// <summary>
    /// Gets or sets the final woven prompt tapestry compiled after all weaving steps.
    /// </summary>
    public VKPromptTapestry? Tapestry { get; set; }

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
    // 4. Truncated & Evicted Dialogue History
    // ==========================================

    private readonly Lock _lockEvicted = new();
    private readonly List<VKPromptFragment> _evicted = [];

    /// <summary>
    /// Gets all dialogue history fragments that were evicted/discarded due to token or turn budgets.
    /// </summary>
    public IReadOnlyList<VKPromptFragment> Evicted
    {
        get
        {
            lock (_lockEvicted)
            {
                return [.. _evicted];
            }
        }
    }

    /// <summary>
    /// Adds a prompt fragment that was evicted due to token constraints.
    /// </summary>
    /// <param name="fragment">The evicted fragment.</param>
    public void AddEvicted(VKPromptFragment fragment)
    {
        VKGuard.NotNull(fragment);
        lock (_lockEvicted)
        {
            _evicted.Add(fragment);
        }
    }

    // ==========================================
    // 5. Extensibility Container
    // ==========================================

    private readonly Dictionary<Type, object> _extensions = new();

    /// <summary>
    /// Attaches an extensibility object to this context for downstream stages.
    /// </summary>
    /// <typeparam name="T">The type of the extension.</typeparam>
    /// <param name="value">The extension instance to store.</param>
    public void SetExtension<T>(T value) where T : class
    {
        _extensions[typeof(T)] = VKGuard.NotNull(value);
    }

    /// <summary>
    /// Retrieves a previously attached extensibility object from this context.
    /// </summary>
    /// <typeparam name="T">The type of the extension to retrieve.</typeparam>
    /// <returns>The stored extension instance, or null if not found.</returns>
    public T? GetExtension<T>() where T : class
    {
        return _extensions.TryGetValue(typeof(T), out var v) ? (T)v : null;
    }

    // ==========================================
    // 6. Strong-typed Feature Overrides (L1)
    // ==========================================

    /// <summary>
    /// Gets or sets request-scoped overrides for the Echo feature.
    /// </summary>
    public VKEchoArgs? Echo { get; set; }

    /// <summary>
    /// Gets or sets request-scoped overrides for the Knowledge feature.
    /// </summary>
    public VKKnowledgeArgs? Knowledge { get; set; }

    /// <summary>
    /// Gets or sets request-scoped overrides for the Persona feature.
    /// </summary>
    public VKPersonaArgs? Persona { get; set; }

    /// <summary>
    /// Gets or sets request-scoped overrides for the Directive feature.
    /// </summary>
    public VKDirectiveArgs? Directive { get; set; }
}
