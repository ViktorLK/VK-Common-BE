---
layer: 3
id: ai-psyche-manifest
scope: building-blocks/ai-psyche
# [Specialized Rules for AI.Psyche] Parallel Pipeline, Onion Middleware, and Compile-time Expression Trees
requires: CS.01, CS.03, CS.04, AP.01, AP.03, BB.01, BB.03, BB.07, DL.01, OR.03, AP.05
---

# 🧠 VK.Blocks AI.Psyche Manifest (Layer 3)

The `AI.Psyche` module is a high-performance prompt orchestration and onion-middleware execution pipeline designed for agentic workflows.

## Specialized Rules for Psyche

### 1. Zero-Infrastructure & Thread-Safe Context
- **InMemory Defaults**: All stores (`IVKPersonaStore`, `IVKEchoStore`, etc.) MUST provide lightweight InMemory implementations as their defaults to support zero-infrastructure startup.
- **Concurrent Access Control**: `VKPsycheContext` is highly stateful and concurrently modified during stage execution. Thread-safe mutations (e.g., using `System.Threading.Lock` or concurrent collections) are MANDATORY for shared pipeline state.

### 2. Compiled Expression Trees for Knowledge Matching
- **Performance Compilation**: Key-matching and regex-matching rules in `Knowledge` stage MUST be compiled into expression trees (`System.Linq.Expressions`) and cached via `ConcurrentDictionary` to minimize CPU/GC overhead.
- **Regex Protection**: ReDoS attacks MUST be mitigated by specifying strict match timeouts (default 100ms) on all compiled/runtime Regex instances.

### 3. Parallel Stage Execution & Task Ordering
- **Parallel Groups**: Stages (e.g., `Persona`, `Echo`, `Directive`) designated with matching `ParallelGroup` order index MUST run concurrently using `VKWeavingStepRunner` or `Task.WhenAll`.
- **Determinism**: Weaving tasks MUST follow strict, deterministic ordering (`VKWeavingTaskOrder.cs`) to prevent layout instability during System and Timeline assemblies.

### 4. Sliding Token-Aware Pruning
- **Eviction Strategy**: Echo (conversation history) processing MUST enforce a dual budget limit (`MaxTurns`/`MaxWindowSize` and token capacity limits via `TokenBudgetRatio`). History eviction MUST dynamically prune oldest turns when approaching limits without throwing exceptions.

### 5. Onion Middleware Structure
- **Middleware Delegates**: Interceptions of `IVKChatEngine` calls MUST adhere strictly to the Onion Middleware Pattern (`IVKPsycheMiddleware` delegating via `VKPsycheMiddlewareDelegate`). Ensure custom middleware registers safely using `TryAddEnumerable`.
