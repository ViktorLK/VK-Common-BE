---
name: vk-draft-rfc
description: Draft a top-down RFC proposal in docs/02-RFCs/ for a new BuildingBlock or complex feature using docs/00-Blueprints/02-RfcBlueprint.md (PS.03).
---

# Skill: Draft RFC Proposal (`PS.03`)

Use this skill when proposing a new BuildingBlock, complex feature slice (e.g., AI cognitive engine, distributed state machine, Source Generator mechanism), or major architectural shift before decomposing tasks into the backlog.

## Steps

1. **Bootstrap Context (`PS.04`)**:
   - If a target module path exists, call `vk_be_get_module_context` on that directory.
   - Fetch architectural rules `PS.03, CS.01, AP.01` via `vk_be_get_architectural_rule`.

2. **Load the Single Source of Truth Blueprint**:
   - Read `docs/00-Blueprints/02-RfcBlueprint.md`.

3. **Determine Next RFC Number**:
   - List existing files in `docs/02-RFCs/` matching `NNN-*.md` and increment to the next 3-digit number (e.g., `002`).

4. **Generate & Save the RFC**:
   - Follow every section of `docs/00-Blueprints/02-RfcBlueprint.md` strictly (Motivation, Goals & Non-Goals per `BASE-06`, Metaphor & Mapping per `PS.03`, Contract Blueprint per `BASE-01`, Trade-offs, and Backlog Decomposition).
   - Save to `docs/02-RFCs/{NNN}-{kebab-case-slug}.md`.

