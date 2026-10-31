---
trigger: always_on
---

<layer id="vk-arch" level="L1" name="VK System Architecture Principles" prefix="ARCH" inherits="vk-base">
<charter>
scope: System, module, and package boundaries. Language- and framework-agnostic.
principle: High cohesion, loose coupling, unidirectional dependencies, explicit composition roots.
</charter>

<locked_rules>
ARCH-01 [LOCKED] Strict Unidirectional Layering: Layers are declared in each module's manifest. Dependencies point only toward lower layers; lower layers have zero awareness of higher ones. Lateral dependencies between peers are prohibited unless declared in the manifest. Concrete layer vocabulary belongs to the stack layers below this one.
ARCH-02 [LOCKED] Port-and-Adapter Decoupling: Application and domain logic depend only on ports (abstractions). Concrete infrastructure adapters (ORM, cache, cloud SDK) are resolved exclusively at the Composition Root.
</locked_rules>

<rules>
ARCH-03 Core Purity: A capability belongs in Core only if it passes the Isolation Test: "Is it meaningful in isolation from all I/O?" Core carries no I/O, web, or UI-framework dependency and no environmental side effects. Provider-specific implementations live outside Core.
ARCH-04 Packaging Integrity: Every package or subpath segment corresponds to a real, independently referenceable dependency. Split into a separate package only when a genuinely distinct third-party dependency or a physical deployment boundary appears; otherwise stay a slice or subpath inside the existing package. Directories are not release packages.
ARCH-05 Co-Located Defaults, Deterministic Overrides: Neutral Fallback implementations (NoOp, InMemory) ship in the same package as their contract and are registered idempotently (e.g., TryAdd semantics). Two tiers exist: Fallback and Provided. Replacing a default requires an explicit builder API, and the resolved implementation must not depend on registration order. No separate `.InMemory` packages.
ARCH-06 Vertical Slices: Group code by feature slice. Folders reflect role, not C# accessibility or technical type; no Common/, Utils/, or Helpers/ dumping grounds. A new folder category must pass the open subfolder registry gate. ◇ `Internal/` is the one registered exception, for slice-private implementation.
ARCH-07 Minimal Public Exposure: Internal by default. Every public symbol is explicit, intentional, and reflected in the public API baseline. No wildcard re-exports. Public error codes, event schemas, and configuration keys are contracts; changing them is breaking.
ARCH-08 Composition Root Isolation: Wiring happens only at the Composition Root. Module registrations are self-contained and idempotent, and registration across modules is independent. Fluent chaining stays within a single module's own surface; parent-before-child ordering is enforced by types where possible. The container is validated at host startup.
ARCH-09 Boundary-Established Context: Execution context (tenant, user, correlation, culture) is established at inbound boundaries (HTTP middleware, queue consumers; background jobs re-establish scope from the message). It reaches inner code only explicitly: as method parameters or through injected context abstractions. Libraries never read ambient sources, never re-derive context, and own no tenant or user business semantics unless that is their domain.
ARCH-10 Orchestration Purity: Coordinators, facades, and pipelines orchestrate; they never implement domain algorithms or state-transition rules. Domain invariants stay in the owning library.
ARCH-11 Extension Taxonomy: Variation uses Strategy plus a Resolver with an explicit fallback chain. Sequential workflows use Composite pipelines. Onion-style middleware (wrapping a next delegate) is a different execution shape and stays separate from Composite. Add a hierarchy level only for a different execution shape, never for an implementation variant.
ARCH-12 Perimeter Cross-Cutting: Retries, timeouts, circuit breakers, rate limiting, caching, and tracing attach as decorators or middleware at ports and boundaries, never inside domain or application logic. Protocol-agnostic resilience lives in the lower library; protocol or framework glue lives in the upper one. Telemetry names derive from the module's single identifier, with no redundant constants.
ARCH-13 Typed Configuration: Every module exposes strongly typed Options with centrally defined defaults, validated at startup. Per-call overrides use typed, nullable Args models. Libraries never read environment variables.
</rules>

<decision_method>
Evaluation order: correctness and security > boundary clarity > maintainability and cognitive load > developer experience > performance (unless benchmarked).

ADR template. Headings in English, body in Japanese.

- ID and Status (Proposed | Accepted | LOCKED | Superseded). ID format: ADR-<AREA>-<NN>.
- Context: constraints and triggers.
- Options: at least two, with pros and cons.
- Decision and rationale.
- Consequences and trade-offs.

How to answer architecture questions:

- Technically decidable: give a clear conclusion with reasons.
- Preference-based: give a recommendation with reasons, mark it as a preference decision, and leave the decision to the user.
- Check the proposal against the ARCH rules first; if it conflicts, cite the rule ID before proposing alternatives.

</decision_method>

<manifest_spec>
One manifest per module, named `{module}-manifest.md`, written as abstract boundary principles rather than method names.

- layer: the layer declared by the owning system.
- purpose: one sentence on what the module owns and what it delegates.
- allowed_dependencies and forbidden_dependencies.
- public_surface: the intentional contracts and extension points, as principles.
- defaults_and_overrides: fallback implementations, exclusivity rules, override mechanism.

</manifest_spec>

<anti_patterns>

- Infrastructure types (DbContext, connection multiplexers) leaking into domain or Core contracts.
- A contract package depending on a concrete type from a peer module, creating lateral coupling.
- Speculative generic layers for features with a single implementation.
- Behavior that changes with the order of registration calls.
- Manager, Helper, or Utils god classes and folders.
- Domain algorithms implemented inside an orchestration layer.
- "Smart" defaults that quietly make business decisions.

</anti_patterns>

<audit_checklist phase="B">

- ARCH-01: Dependencies flow only downward, with no undeclared lateral references?
- ARCH-02: Domain and application code free of concrete infrastructure references?
- ARCH-07: New symbols internal by default; public surface intentional?
- ARCH-09: Context established at the boundary and passed explicitly or via injected abstractions?
- ARCH-12: Resilience and tracing attached at the perimeter, not in business logic?

</audit_checklist>

<audit_checklist phase="A">

- ARCH-03: Passes the Core Isolation Test?
- ARCH-04: Packaging justified by a distinct third-party dependency or deployment boundary?
- ARCH-05: Fallback defaults co-located; override API explicit and order-independent?
- ARCH-13: Typed Options with startup validation?
- ARCH-06: Slice structure follows the registry; no dumping-ground folders?
- ARCH-01: Manifest drafted per manifest_spec; ADR written if the design involves a real trade-off?

</audit_checklist>
</layer>
