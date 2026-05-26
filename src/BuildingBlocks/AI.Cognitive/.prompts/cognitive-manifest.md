---
layer: 3
id: cognitive-manifest
scope: module
extends: industrial-dna
requires: CS.01, CS.03, BB.01
---

# Cognitive Manifest: AI.Cognitive Rule Set

This manifest governs all architectural, logical, and prompt engineering behaviors inside the `AI.Cognitive` module.

## COG01: Trait Encapsulation [standalone]
- **Rule**: Traits defined in the **Persona** module are private assets of the ego state.
- **Enforcement**: No external slice or feature (such as **Agents** or **Orchestration**) may read or mutate trait values directly. All updates must be routed through private interfaces in the **Persona** module.

## COG02: Weaver Sovereignty [standalone]
- **Rule**: Phase Shift logic (determining how personality traits drift based on emotional signals) MUST reside exclusively within the **Persona** module.
- **Enforcement**: Feature folders under other slices must never contain trait-shifting algorithms.

## COG03: Non-Blocking Stream & Reasoning Gate [uses: CS.03, CS.06]
- **Rule**: The Server-Sent Events (SSE) stream managed by **Orchestration** must never be blocked by high-latency operations.
- **Enforcement**: Slow calculations in **Reasoning** or **Agents** MUST be bypassed or served from cache. All emissions from **Orchestration** must be annotated with `EmissionPhase` { Weaving | Emitting }.

## COG04: Hybrid Three-Layer Memory Stack [uses: CS.01, CS.04]
- **Rule**: Memory retrieval in the **Memory** module must be orchestrated across three distinct layers: Vector, Graph, and Structured.
- **Enforcement**: No single layer may serve as the sole source of truth. Querying must combine all three layers in the **Memory** module into a consolidated `Result<VKMemoryContext>`.

## COG05: Intent & Interaction Mode Orchestration [uses: CS.01]
- **Rule**: The **Orchestration** module must determine the active interaction mode { TechAdvisor | TreeHole | Cactus | RolePlay } per request using intent routing from the **Reasoning** module.
- **Enforcement**: Mode changes trigger an automatic snapshot of session state and emit a `Result<OrchestratorEvent>`.

## COG06: Lorebook Context & Token Budget [uses: CS.04]
- **Rule**: Lorebook fragments injected into the active context window by the **Knowledge** module must strictly adhere to the token budget.
- **Enforcement**: Token budget overflow is treated as a fatal `VKResult` failure in the **Knowledge** module, never as a warning.

## COG07: 3-Tier Prompt Formatting Strategy [standalone]
- **Rule**: All prompt engineering within the cognitive engine must strictly adhere to the following 3-tier formatting strategy:
  1. **Macro-Skeleton & Sovereignty Isolation (L1 Core Codex, L4 Scenario Knowledge)** $\rightarrow$ Exclusively use XML tags. Lock architectural invariants with `<system_codex>` and RAG-retrieved knowledge with `<knowledge_entries>`.
  2. **Knowledge Expression & Rule Trees (L3 Soul Mirror Content)** $\rightarrow$ Exclusively use Markdown. Inside XML tags, utilize Markdown headers (e.g., `# Background`) and lists (e.g., `- Catchphrase`) for high signal-to-noise ratio content representation.
  3. **Dynamic Variables & Immediate Tail-Anchoring (L5 Biological Pulse, Blue-Light Directives)** $\rightarrow$ Exclusively use Brackets/Braces. At the critical assembly point closest to the model's response generation (post User Input), utilize constructs like `[Author's Note: Focus on sensory details]` to enforce absolute attention lock.
- **Enforcement**: All `IVKPromptFormatter` implementations must adhere to this typography. XML for structure, Markdown for content, brackets for immediate attention overrides.
