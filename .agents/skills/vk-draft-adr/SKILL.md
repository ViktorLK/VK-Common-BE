---
name: vk-draft-adr
description: Generate an Architecture Decision Record (ADR) and update the module ADR index in docs/04-ArchitectureDecisionRecords/ using vk_be_draft_adr and docs/00-Blueprints/04-AdrBlueprint.md (DL.03, PS.02).
---

# Skill: Draft Architecture Decision Record (`DL.03`, `PS.02`)

Use this skill whenever an architectural decision point is reached (new cross-cutting interface, pattern change, trade-off resolution) to record an ADR in `docs/04-ArchitectureDecisionRecords/{ModuleName}/`.

## Steps

1. **Bootstrap Context (`PS.04`)**:
   - Call `vk_be_get_module_context` on the target module directory (e.g., `src/BuildingBlocks/{ModuleName}`).
   - Fetch architectural rules `PS.01, PS.02` via `vk_be_get_architectural_rule`.

2. **Prepare ADR Analysis & Invoke Deterministic MCP Tool**:
   - Synthesize the architectural decision parameters:
     - `sourceDir`: Module directory or name (e.g. `src/BuildingBlocks/Authentication`).
     - `title`: Concise, decisive English title (e.g. `Centralized Identity Claims Resolution as SSOT`).
     - `decision`: Concrete decision, patterns, and code design.
     - `context`: Background context, triggers, and problem statement.
     - `alternatives`: Rejected alternatives with approach and reasons.
     - `consequences`: Positive benefits, negative trade-offs, and mitigations.
     - `implementation`: Security considerations, lifecycle, and patterns.
     - `summary`: Japanese 1-2 sentence executive summary for the index.
     - `keywords`: 2-3 relevant tags (e.g. `SSOT, Identity, Zero-Allocation`).
     - `supersedes`: (Optional) Previous ADR number if replacing an older decision (e.g. `001`).
   - Call `vk_be_draft_adr` from `vk-blocks-be-manager`.
   - The MCP tool deterministically:
     - Computes next sequential `adr-{NNN}` number and safe kebab-case filename.
     - Auto-updates previous superseded ADRs (status and link).
     - Writes the finalized ADR markdown to `docs/04-ArchitectureDecisionRecords/{ModuleName}/adr-{NNN}-{slug}.md`.
     - Automatically indexes the decision and recalculates metrics in `docs/04-ArchitectureDecisionRecords/{ModuleName}/README.md`.

3. **Verify and Report**:
   - Confirm the returned file path and index update status.
   - Report the published ADR to the user with link and decision highlights.

