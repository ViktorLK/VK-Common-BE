---
name: vk-sprint-task-done
description: Complete a Sprint task, verify DoD, link git commits, verify build, and update status in-place.
---

# Skill: Task Done (`vk-sprint-task-done`)

Use this skill when implementation for a task is finished and ready to be closed.

## Steps

1. **Verify Definition of Done (DoD)**:
   - Verify the global DoD in `docs/07-Backlogs/Definition_of_Done.md` first; task-specific DoD only adds to it.
   - Check the DoD section in `docs/07-Backlogs/{Module}/pbi-{NNN}-*.md`.
   - Use `completeDod: true` only after every item has actually been verified.
   - **ADR Assessment (`DL.03`)**: If a public contract, architecture pattern, or DI sequence was modified, ensure an ADR was drafted via `vk-draft-adr`.

2. **Transition Task to Done**:
   - Call `vk_be_backlog_transition` from `vk-blocks-be-manager` with:
     - `id`: Target Task ID (e.g. `#024`).
     - `toStatus`: `done`.
     - `completeDod`: Set `true` to automatically check off all DoD check boxes if build and commit validation pass.
     - `verifyBuild`: Set `true` to ensure zero compilation errors and warnings (`-warnaserror`).
     - `allowNoCommits`: Set `true` with explanatory note ONLY if the task intentionally produced no code commits (e.g. spike, pure doc).
   - The MCP tool will:
     - Run build verification (`-warnaserror`) if requested.
     - Automatically find commits via `git log --all --fixed-strings --grep="[{TASK_ID}]"` and append commit hashes to `commits: []` in front-matter. If 0 commits are found and `allowNoCommits` is false, it rejects completion.
     - Check off DoD items (if `completeDod: true`) and verify zero unchecked items remain.
     - Update the physical file in-place with `closed` timestamp (zero git-mv).
     - Regenerate `Active_Sprint.md`, `Product_Backlog.md`, and `README.md`.

3. **Capture New Technical Debt (`DL.04`)**:
   - If any new debt or follow-up was discovered during implementation, invoke `vk-sprint-add-task` immediately to record it.
