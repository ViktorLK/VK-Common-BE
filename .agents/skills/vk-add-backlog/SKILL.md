---
name: vk-add-backlog
description: Create a prioritized atomic backlog task in docs/07-Backlogs/ and sync Active_Sprint.md using vk_be_add_backlog_item and docs/00-Blueprints/07-BacklogItemBlueprint.md (DL.04).
---

# Skill: Add Atomic Backlog Item (`DL.04`)

Use this skill when technical debt, audit findings (from `docs/06-AuditReports/`), or approved RFC tasks (from `docs/02-RFCs/`) need to be recorded as actionable backlog items in `docs/07-Backlogs/{ModuleName}/`.

## Steps

1. **Load the Single Source of Truth Blueprint**:
   - Read `docs/00-Blueprints/07-BacklogItemBlueprint.md` to determine the priority classification (`High` for Type A violations, `Medium` for Type B or features, `Low` for optimizations) and structured description format.

2. **Invoke Deterministic MCP Tool**:
   - Call `vk_be_add_backlog_item` from `vk-blocks-be-manager` with `module`, `title`, structured `description` (Problem/Context + Proposed Changes per `07-BacklogItemBlueprint.md`), `priority`, `target`, and `reference` (Rule IDs, ADR, or Audit Report path).
   - The MCP tool automatically assigns the next `{MODULE}-{NNN}` ID, writes the file under `docs/07-Backlogs/{Module}/`, and updates `docs/07-Backlogs/Active_Sprint.md`.

