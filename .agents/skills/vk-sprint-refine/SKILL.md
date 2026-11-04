---
name: vk-sprint-refine
description: Refine backlog items with Story Points (Fibonacci), ranking, and DoR (Definition of Ready) checks.
---

# Skill: Sprint Backlog Refinement (`vk-sprint-refine`)

Use this skill periodically (e.g., weekly before Sprint Planning) to estimate unestimated tasks, order priorities, and promote items to `Ready` status.

## Steps

1. **Identify Unestimated Candidates**:
   - Inspect `docs/07-Backlogs/Product_Backlog.md`.
   - Filter for `P0` and `P1` tasks with `Points: -` (unestimated) or `Ready: —`.

2. **Estimate with Fibonacci Points (`ADR-004`)**:
   - Assign Story Points from the Fibonacci sequence: `1, 2, 3, 5, 8, 13`.
   - **Split Rule**: If a task exceeds `13` points, it is too large for a 1-week sprint. Decompose it into multiple atomic stories/tasks using `vk-sprint-add-task`.

3. **Verify Definition of Ready (DoR)**:
   - Ensure the task meets all 4 DoR criteria:
     1. Points assigned (`1, 2, 3, 5, 8, 13`).
     2. Description has `### 1. Problem / Context` and `### 2. Proposed Changes`.
     3. DoD has at least one verification item besides the title.
     4. Category is not `feature`.

4. **Invoke MCP Tool**:
   - Call `vk_be_backlog_refine` from `vk-blocks-be-manager` with:
     - `id`: Target Task ID (e.g. `#024`).
     - `points`: Fibonacci points (e.g. `3`).
     - `rank`: Optional relative order within priority (e.g. `1`).
     - `priority`: Optional updated priority (`P0`, `P1`, `P2`, `P3`).
     - `markReady`: Set to `true` to validate DoR and set `ready: true`.
   - The tool validates DoR, updates front-matter in-place, and regenerates `Product_Backlog.md`.
