---
trigger: manual
---

<ruleset category="PS" name="Planning & Decision Standards">

<rule id="PS.01" type="B" name="Mandatory Architecture Decision Audit">
ALL implementation plans (`implementation_plan.md`) generated for this repository MUST include a dedicated **"Architectural Decision Audit"** section before the Proposed Changes.

#### 1. Audit Requirements
- **Contract Impact**: Explicitly state if the change modifies any Level 1 Public API or core architectural pattern.
- **ADR Necessity**: Make a definitive "Yes/No" determination on whether an ADR (Architecture Decision Record) is required per DL.03.
- **Traceability**: If an ADR is required, adding the task `[ ] Generate ADR` to the plan's TODOs is MANDATORY.

#### 2. Decision Logic
An ADR is **MANDATORY** if the change:
- Introduces a new cross-cutting interface.
- Modifies the signature of `Result<T>` or common `VKXxxOptions`.
- Changes the standard DI registration sequence (BB.03).
- Adopts a new external dependency that impacts the BuildingBlock's "Industrial DNA".
</rule>

<rule id="PS.02" type="B" name="Closed-Loop Walkthrough">
The `walkthrough.md` MUST verify the "Decision Traceability":
- If an ADR was planned, the walkthrough MUST link to the newly created `.md` file in `docs/04-ArchitectureDecisionRecords/`.
- Failure to document a significant architectural shift is considered a violation of **DL.03**.
</rule>

<rule id="PS.03" type="B" name="RFC-First Policy (Top-Down Design)">
For any **Complex Feature** or **Experimental Logic** (e.g., AI Cognitive models, complex state machines, or novel algorithms), an RFC (Request for Comments) MUST be created in `docs/02-RFCs/` before any atomic tasks are added to the backlog.

#### Workflow Integration
1. **Draft RFC**: AI or User creates the proposal using `docs/00-Blueprints/02-RfcBlueprint.md`.
2. **Approval**: User reviews and marks as `✅ Approved`.
3. **Backlog Decomposition**: Once approved, the RFC is decomposed into multiple atomic tasks using `vk_be_add_backlog_item`.
</rule>

<rule id="PS.04" type="A" name="Contextual Instruction Priority (MCP-Driven)">
When performing tasks restricted to a specific module or Lab project, AI MUST prioritize localized instructions over global rules through active discovery.

#### Dynamic Discovery via MCP
- **Mandatory Action**: Before starting an implementation plan or refactoring, AI MUST execute `vk_be_get_module_context` on the target directory.
- **Token Efficiency**: Do not attempt to guess or remember local rules; trust the MCP tool's output as the primary source of truth for the specific scope.
- **Core Standard Protection**: If a local prompt conflicts with a Type A Core Standard, AI MUST explicitly flag this in the Implementation Plan.
</rule>

<rule id="PS.05" type="A" name="Evidence-Based Structural Modification">
AI is strictly prohibited from modifying directory structures, namespaces, file names, or class naming based on memory, assumption, or generic convention.

#### Mandatory L3 Specification Grounding
- **Evidence Requirement**: Any structural refactoring, vertical slice layout modification, or symbol visibility change MUST be grounded in explicit L3 specifications fetched via `vk_be_get_architectural_rule` (e.g., `BB.01`, `AP.03`, `BB.03`).
- **No Speculative Naming**: Do not invent folder categories or namespace segments without passing the registered subfolder taxonomy or verifying the module's manifest.
</rule>

</ruleset>
