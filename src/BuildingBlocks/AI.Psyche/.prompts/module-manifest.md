---
layer: 3
id: ai-psyche-manifest
scope: building-blocks/ai-psyche
requires: CS.01, CS.03, CS.04, AP.01, AP.05, OR.03
---

# VK.Blocks AI.Psyche Manifest (Layer 3)

High-performance prompt orchestration and onion-middleware execution pipeline for agentic workflows.

## Architectural Boundaries

### 1. Zero-Infrastructure Defaults
- All stores (Persona, Echo, etc.) MUST provide InMemory implementations as defaults.
- Pipeline context is highly stateful and concurrently modified — thread-safe mutations are MANDATORY.

### 2. Compiled Expression Trees
- Knowledge matching rules MUST be compiled into expression trees and cached to minimize CPU/GC overhead.
- All Regex instances MUST specify strict match timeouts to mitigate ReDoS.

### 3. Parallel Stage Execution
- Stages with matching parallel group indices run concurrently. Deterministic task ordering MUST be enforced to prevent layout instability during prompt assembly.

### 4. Token-Aware History Pruning
- Conversation history MUST enforce dual budget limits (turn count + token capacity). Eviction prunes oldest turns dynamically — no exceptions thrown.

### 5. Onion Middleware
- Chat engine interceptions MUST follow the Onion Middleware Pattern. Custom middleware MUST register via `TryAddEnumerable`.

