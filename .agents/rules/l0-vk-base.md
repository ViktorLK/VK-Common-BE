---
trigger: always_on
---

<layer id="vk-base" level="L0" name="VK IT Base & Engineering Mindset" prefix="BASE">
<charter>
scope: Universal IT work: architecture, backend, frontend, DevOps, review, debugging.
principle: First-principles engineering, deterministic behavior, zero implicit assumptions.
</charter>

<locked_rules>
BASE-01 [LOCKED] Contract-First: Define interfaces, DTOs, domain models, error codes, and invariants BEFORE writing implementation code.
BASE-02 [LOCKED] Zero Implicit Globals: Static mutable state, black-box singletons, and direct environment reads outside the Composition Root are prohibited. Time, ID generation, storage, logging, and execution context MUST be injected through abstractions.
BASE-03 [LOCKED] Compile-Time First: Any constraint that can be enforced by the type system, analyzers, linters, or code generation MUST NOT be deferred to runtime.
BASE-04 [LOCKED] Anti-Over-Friendliness: Frameworks resolve objective technical ambiguity only. Never guess business intent or add "smart" implicit behavior; require explicit intent through APIs. Neutral, documented Fallback-tier defaults (e.g., NoOp) are allowed.
BASE-08 [LOCKED] Honesty and Conflict Surfacing: Never fabricate APIs, packages, versions, or behaviors. If an instruction conflicts with a rule, state the rule ID, the contradiction, and a compliant alternative. Never deviate silently; deviation requires an explicit OVERRIDE.
</locked_rules>

<rules>
BASE-05 Explicit Failure Model: Foreseeable business and operational failures return typed `VKResult<T>`; never bare nulls, never exceptions as control flow. Exceptions are reserved for programming errors and broken invariants; foreseeable infrastructure failures (timeout, unavailable, external error) are mapped to Result at the adapter boundary. Error codes are named constants defined centrally, never inline string literals.
BASE-06 Radical YAGNI: Without a confirmed consumer, do not add abstractions, options, speculative packages, or empty adapters. Build only what is needed now.
BASE-07 Production-Grade Delivery: No pseudocode, placeholder stubs, or incomplete algorithms. Emitted code must compile and run. Behavior changes ship with tests. Bare TODO is prohibited; technical-debt markers must state a condition, e.g. `TODO[remove-after-source-gen]`.
BASE-09 Language Convention: Identifiers, code, comments, and commit messages are written in English. Explanations and ADRs are written in Japanese.
BASE-10 Surgical Change Discipline: Deliver the minimal diff that satisfies the specification; do not refactor unrelated code. If ambiguity arises, ask at most one clarifying question; otherwise state explicit assumptions and proceed. End every substantive response with the assumptions made and anything left unverified.
BASE-11 Documentation Linking: Internal document links across directories MUST use repository-root-relative paths (`/docs/...`, `/src/...`, `/.agents/...`). Upward relative paths (`../`) are prohibited; local `./` is permitted only within the same directory or below.
BASE-12 Principles Over Signatures: Manifests and boundary documents state invariant principles and contracts, not transient method signatures.
</rules>

<precedence>
1. A lower layer may tighten or specialize a rule; it can never relax a higher-layer rule.
2. Deviating from a non-LOCKED rule requires `OVERRIDE <RULE_ID>: <justification> (ADR-<AREA>-<NN>)`.
3. [LOCKED] rules have no override channel. If one is wrong, revise the layer that owns it.
4. When a request conflicts with an active rule: surface the collision, present the compliant alternative, then proceed on stated assumptions.
5. Technically decidable questions get a clear conclusion with reasons. Preference-based questions get a recommendation with reasons, explicitly marked as a preference decision that belongs to the user.
6. Cite rules by rule ID only, never by tag name or section name.
</precedence>

<output_protocol>
Applies to every response that produces code, an ADR, or a manifest. Pure discussion is exempt.

- **Handshake**: `Active: [{Layers}:{Module}] | Context: {Path} | Sync: [{RuleID},...]` — MUST be the very first line of the final response (no intermediate tool outputs). Example: `Active: [L0-L3:Auth] | Context: src/features/auth | Sync: [CS.01, AP.01]`.
- **Sync**: MUST list ALL Rule IDs verified or consulted in the current turn.
- **Hard-Lock**: Missing rule verification for a relevant scenario → code output PROHIBITED.
- **Context Switch**: Module change → re-check local docs/manifest + update handshake.
- **Code**: Strict mode (TypeScript strict: true / C# 12+). English only (BASE-09).
- **Tags**: `// [RuleID]` at architectural boundaries (boundary guards, Result wrappers, DI/Provider, public exports). Pure internal logic exempt.
- **Audit**: Placed at the end of the response:
  - Standard: `Audit: ✅` / `Audit: 🚩 [RuleID] {rationale}` (self-correct immediately).
  - Phase A (New Block / Package only): Verify consumer (BASE-06) and manifest (BASE-12).
    </output_protocol>

<anti_patterns>

- Catching and swallowing exceptions, or turning business validation failures into thrown exceptions.
- Calling DateTime.UtcNow, Guid.NewGuid, or reading environment variables inside domain logic.
- Adding generic extension points that have no consumer.
- Emitting "TODO: implement later".
- Silently changing behavior to satisfy a request that conflicts with a rule.

</anti_patterns>

<audit_checklist phase="B">

- BASE-08: Rule collisions surfaced? No fabricated APIs?
- BASE-10: Assumptions and unverified items listed? Diff minimal?
- BASE-01: Contracts and invariants defined before implementation?
- BASE-02: No static mutable state; external capabilities injected?
- BASE-03: Constraints enforced at compile time where possible?
- BASE-05: Failure paths expressed as Result; no bare null or inline error strings?
- BASE-07: No placeholders or naked TODOs; behavior changes covered by tests?

</audit_checklist>

<audit_checklist phase="A">

- BASE-06: Real consumer confirmed for the new unit?
- BASE-12: Manifest written: layer, allowed and forbidden dependencies, public surface, defaults?

</audit_checklist>
</layer>
