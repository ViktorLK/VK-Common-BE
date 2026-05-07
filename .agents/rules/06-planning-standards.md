---
trigger: model_decision
---

# VK.Blocks: Planning & Decision Standards (PS)

### PS.01 — Mandatory Architecture Decision Audit

ALL implementation plans (`implementation_plan.md`) generated for this repository MUST include a dedicated **"Architectural Decision Audit"** section before the Proposed Changes.

#### 1. Audit Requirements

- **Contract Impact**: Explicitly state if the change modifies any Level 1 Public API or core architectural pattern (CS.01-21).
- **ADR Necessity**: Make a definitive "Yes/No" determination on whether an ADR (Architecture Decision Record) is required per DL.03.
- **Traceability**: If an ADR is required, adding the task `[ ] Generate ADR` to the `task.md` or the plan's TODOs is MANDATORY.

#### 2. Decision Logic

An ADR is **MANDATORY** if the change:

- Introduces a new cross-cutting interface.
- Modifies the signature of `Result<T>` or common `VKXxxOptions`.
- Changes the standard DI registration sequence (BB.03).
- Adopts a new external dependency that impacts the BuildingBlock's "Industrial DNA".

### PS.02 — Closed-Loop Walkthrough

The `walkthrough.md` MUST verify the "Decision Traceability":

- If an ADR was planned, the walkthrough MUST link to the newly created `.md` file in `docs/02-ArchitectureDecisionRecords/`.
- Failure to document a significant architectural shift is considered a violation of **DL.03**.

### PS.03 — RFC-First Policy (Top-Down Design)

For any **Complex Feature** or **Experimental Logic** (e.g., AI Cognitive models, complex state machines, or novel algorithms), an RFC (Request for Comments) MUST be created in `docs/06-RFCs/` before any atomic tasks are added to the backlog.

#### 1. RFC Core Components

- **Metaphor**: A clear mental model (biological, physical, or logical) explaining the "why" and "how" in human terms.
- **Mapping**: A structured table or diagram showing how the metaphor components map to code artifacts.
- **Code Blueprint**: The foundational interfaces and record definitions.

#### 2. Workflow Integration

1.  **Draft RFC**: AI or User creates the proposal using the `TEMPLATE.md`.
2.  **Approval**: User reviews and marks as `✅ Approved`.
3.  **Backlog Decomposition**: Once approved, the RFC is decomposed into multiple atomic tasks using `VKAddBacklogItem`.

### PS.04 — Contextual Instruction Priority (MCP-Driven)

When performing tasks restricted to a specific module or Lab project, AI MUST prioritize localized instructions over global rules through active discovery.

#### 1. Dynamic Discovery via MCP

- **Mandatory Action**: Before starting an implementation plan or refactoring, AI MUST execute `VKGetModuleContext` on the target directory.
- **Goal**: Identify localized `.prompts/` (e.g., `pwp-manifest.md`) and Layer 2 manifestos (`research-manifesto.md`).
- **Token Efficiency**: Do not attempt to guess or remember local rules; trust the MCP tool's output as the primary source of truth for the specific scope.

#### 2. Conflict Resolution Hierarchy

- **Priority**: Local Prompt (Layer 3) > Lab Manifesto (Layer 2) > Global Standards (Layer 1).
- **Core Standard Protection**: If a local prompt conflicts with a Core Standard (CS.01-CS.06), the AI MUST explicitly flag this as a **"Standard Violation for Domain/Lab Needs"** in the Implementation Plan for User Review.
- **Naming Exceptions**: If Layer 2/3 explicitly waives the `VK` prefix or `Internal/` convention, AI MUST follow that exception immediately without further questioning.
