---
name: vk-add-backlog
description: Create a prioritized atomic backlog task with a global incremental ID (#NNN) in docs/07-Backlogs/{ParentModule}/ and regenerate master boards (DL.04).
---

# Skill: Add Atomic Backlog Item (`DL.04`)

Use this skill when technical debt, audit findings (from `docs/06-AuditReports/`), or approved RFC tasks (from `docs/02-RFCs/`) need to be recorded as actionable backlog items in `docs/07-Backlogs/{ParentModule}/`.

## Steps

1. **Load Blueprint & Determine Hierarchy**:
   - Read `docs/00-Blueprints/07-BacklogItemBlueprint.md` to classify:
     - `category`: `feature` (big milestone), `story` (user/dev scenario), `task` (standard code task), `debt` (technical debt / bug), `spike` (research).
     - `parent`: Optional parent task or RFC ID (e.g. `#010` or `RFC-002`).
     - `priority`: `P0` (Critical blocker), `P1` (Type A rule violation), `P2` (Type B or feature), `P3` (Optimization / Low).
     - `status`: `todo` (actionable now) or `icebox` (unrefined idea).

2. **Invoke Deterministic MCP Tool**:
   - Call `vk_be_add_backlog_item` from `vk-blocks-be-manager` with:
     - `module`: Target module (e.g., `Core`, `AI`, `Persistence`). Sub-slices (e.g. `AI.Cognitive`) are automatically aggregated into the parent pillar.
     - `title`: Short actionable summary.
     - `description`: Structured text containing Problem/Context and Proposed Changes.
     - `category`: `task`, `story`, `feature`, `debt`, or `spike`.
     - `parent`: Optional parent ID.
     - `priority`: `P0`, `P1`, `P2`, or `P3`.
     - `status`: `todo` or `icebox`.
     - `target`: File or path affected.
     - `reference`: Rule IDs (e.g. `CS.01,FS-08`) or root-relative docs path (`/docs/...`).
   - The tool automatically assigns the next global ID (`#123`), writes the file with full YAML front-matter under the parent module directory, and regenerates `Product_Backlog.md` and `Active_Sprint.md`.
