---
name: vk-sprint-plan
description: Plan and launch a 1-week personal Sprint, setting an overarching goal and committing backlog tasks.
---

# Skill: Sprint Planning (`vk-sprint-plan`)

Use this skill when starting a new personal Sprint cycle.

## Steps

1. **Verify Backlog Health**:
   - Call `vk_be_backlog_lint` from `vk-blocks-be-manager` to ensure no broken front-matters or ID duplicates exist.
   - If an active sprint is still running, prompt to close it first via `vk-sprint-review` or call `vk_be_sprint_close`.

2. **Select Candidates from Product Backlog**:
   - Read `docs/07-Backlogs/Product_Backlog.md`.
   - Prioritize items with `P0` and `P1` (Type A rule violations or urgent debt).
   - Propose a focused scope of 2~5 tasks to the user that align with a single cohesive theme.

3. **Agree on Sprint Goal**:
   - Formulate a clear, 1-sentence Sprint Goal (e.g., "Core 品質負債の解消と結果型整合").

4. **Launch Sprint**:
   - Call `vk_be_sprint_start` from `vk-blocks-be-manager` with:
     - `goal`: The 1-sentence Sprint goal.
     - `committedTaskIds`: Comma-separated task IDs (e.g. `CORE-024,CORE-023`).
     - `durationDays`: Default is 7.
   - The tool generates `docs/07-Backlogs/Sprints/S-{Year}-{Week}.md` and updates `Active_Sprint.md`.

