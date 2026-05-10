---
layer: 3
id: pwp-manifest
scope: module
extends: vk-research-manifesto
# [PWP Specific Requirements] Supplementing the Research Manifesto
requires: CS.04
requires: AP.05
requires: BB.01
---

# PWP Rule Set: PersonaWeavePulsar

## PWP01: Signal Agnosticism + Conflict Resolution [uses: CS.01]

All external inputs MUST be normalized to **ISignal** { Intensity, Polarity, Valence, Weight } before reaching the Weaver. The Weaver has zero knowledge of signal origin. Conflicting signals MUST be resolved inside the Weaver via an internal priority queue. Slices declare Weight; Weaver arbitrates.

## PWP02: Trait Encapsulation [standalone]

Tier 1 Traits are private assets of the Weaver. No Slice may read or mutate Trait values directly.

## PWP03: Weaver Sovereignty [standalone]

Phase Shift logic (which Traits shift, by how much) MUST reside exclusively in the Weaver via `IWeavingHeuristic`. Never in a Slice.

## PWP04: Hierarchical Override [extends: AP.05]

Override chain is MANDATORY: Global Policy → Entity Type → Session → Request. No level may be skipped.

## PWP05: Non-Blocking Emission + Reasoning Gate [uses: CS.03, CS.04, CS.06]

The SSE stream must never be blocked by a heuristic. Slow heuristics MUST be bypassed or cache-served. Tier 3 MUST annotate each emission with `EmissionPhase` { Weaving | Emitting }. Weaver-layer behavioral outcomes MUST be deterministic and idempotent (CS.06) for identical **ISignal** inputs.

## PWP06: Slice Contract Isolation [extends: BB.01]

Inter-Slice communication via `Result<T>`-wrapped contracts only. A Slice must never reference another Slice's non-public implementation details. Use Interfaces/Contracts for all cross-slice boundary interactions.

## PWP07: Streaming Resource Discipline [extends: CS.03]

CancellationToken MUST propagate through all Tiers. SSE disconnection triggers immediate resource release. No orphaned inference calls.

## PWP08: Hybrid Memory Architecture [uses: CS.01, CS.03, CS.04]

The system MUST implement a three-layer memory stack (Vector Layer, Graph Layer, Structured Layer). No single Layer may serve as the sole source of truth. Memory retrieval MUST be orchestrated across all three layers and returned as `Result<MemoryContext>`.

## PWP09: Intent Orchestration [uses: CS.01]

The Orchestrator MUST determine the active interaction mode { TechAdvisor | TreeHole | Cactus | RolePlay } per request. Detection is dual-channel (LLM-based or Explicit Override). On mode switch, the Orchestrator MUST snapshot session state and emit `Result<OrchestratorEvent>`.

## PWP10: Proactive Engine [uses: CS.01, CS.06]

The system MUST support AI-initiated interactions via three source channels (Biometric, Temporal, Associative). All proactive emissions MUST be routed through Tier 3 (Reflex) and must not interrupt an active stream. Priority: Biometric > Temporal > Associative.

## PWP11: Lorebook Context Management [uses: CS.01, CS.04]

A dedicated **ILorebookManager** is responsible for injecting WorldBook fragments into the active context window without overflow. Token budget is a hard constraint — overflow is a fatal error, not a warning (CS.04).

## PWP12: Emotional Feedback Loop [uses: CS.01]

**ISignal** is bidirectional. The system MUST support OutputSignal synthesized from AI self-evaluation and user reaction. The Weaver consumes OutputSignal for Persona Evolution, bounded by Tier 1 Anchor constraints.

---

## PWP Audit Checklist (Phase C: Domain Audit)

- [ ] **Signal Normalization (PWP01)**: All external inputs normalized to ISignal.
- [ ] **Trait Encapsulation (PWP02)**: No Slice direct access to Tier 1 Traits.
- [ ] **Weaver Sovereignty (PWP03)**: Phase shift logic exclusive to IWeavingHeuristic.
- [ ] **Override Chain (PWP04)**: Global -> Entity -> Session -> Request followed.
- [ ] **Non-Blocking Stream (PWP05)**: No heuristic blocks SSE; EmissionPhase annotated.
- [ ] **Slice Isolation (PWP06)**: Inter-Slice via Result<T> only.
- [ ] **Memory Orchestration (PWP08)**: All 3 layers (Vector/Graph/Structured) queried.
- [ ] **Proactive Priority (PWP10)**: Biometric > Temporal > Associative enforced.
- [ ] **Lorebook Budget (PWP11)**: Token overflow is handled as a fatal error.
- [ ] **Feedback Loop (PWP12)**: OutputSignal bounded by Anchor constraints.
