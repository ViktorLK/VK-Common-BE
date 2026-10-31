---
name: vk-sprint-task-done
description: Complete a Sprint task, verify DoD, link git commits, and update status in-place.
---

# Skill: Task Done (`vk-sprint-task-done`)

Use this skill when implementation for a task is finished and ready to be closed.

## Steps

1. **Verify Definition of Done (DoD)**:
   - Check the DoD section in `docs/07-Backlogs/{Module}/{NNN}-*.md`.
   - Ensure all check boxes `- [x]` are checked.
   - **ADR Assessment (`DL.03`)**: If a public contract, architecture pattern, or DI sequence was modified, ensure an ADR was drafted via `vk-draft-adr`.

2. **Transition Task to Done**:
   - Call `vk_be_backlog_transition` from `vk-blocks-be-manager` with:
     - `id`: Target Task ID (e.g. `CORE-024`).
     - `toStatus`: `done`.
   - The MCP tool will:
     - Verify zero unchecked DoD items.
     - Automatically run `git log --all --grep="[{TASK_ID}]"` and append commit hashes to `commits: []` in front-matter.
     - Move the physical file to `docs/07-Backlogs/.Archive/{Module}/`.
     - Regenerate `Active_Sprint.md`, `Product_Backlog.md`, and `README.md`.

3. **Capture New Technical Debt (`DL.04`)**:
   - If any new debt or follow-up was discovered during implementation, invoke `vk-add-backlog` immediately to record it.

