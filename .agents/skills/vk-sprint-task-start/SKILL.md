---
name: vk-sprint-task-start
description: Transition a backlog item to 'doing', set up branch context, record started date, and review git commit conventions within a Sprint.
---

# Skill: Task Start (`vk-sprint-task-start`)

Use this skill when beginning work on a specific committed backlog task.

## Steps

1. **Verify Sprint Membership & WIP Limit**:
   - Ensure the task is committed to the current active Sprint. If it is a newly discovered urgent task, first add it to the Sprint using `vk_be_sprint_scope(id: "#NNN", action: "add", reason: "...")`.
   - **WIP Limit Check**: A developer should not have more than 2 tasks concurrently in `doing`. If 2 tasks are already in progress, finish or postpone one before starting another (or pass `allowExceedWip: true` with rationale).

2. **Transition Task Status**:
   - Call `vk_be_backlog_transition` from `vk-blocks-be-manager` with:
     - `id`: Target Task ID (e.g. `#024`).
     - `toStatus`: `doing`.
     - `allowOutsideSprint`: Set `true` only if deliberately working on a task outside of any active Sprint.
   - The tool updates front-matter, records the `started` timestamp (used to calculate Cycle Time in the dashboard), and moves the task into the `🏃 Doing` section of `Active_Sprint.md`.

3. **Branch & Commit Conventions (Personal Workflow)**:
   - **Do NOT create or switch git branches per task**. Continue development directly on the active local branch (e.g. `local-2026`).
   - Remind the developer of commit message syntax so commits can be auto-linked when closed:
     ```text
     feat(module): brief description [#NNN]
     fix(module): brief description [#NNN]
     ```
   - (Sprint or milestone PRs will be submitted collectively via the PR branch `ro/study...`).
4. **Read Task Blueprint & Rules**:
   - Read the task file in `docs/07-Backlogs/{Module}/pbi-{NNN}-*.md`.
   - Check the `Ref` field for rule IDs (e.g. `CS.01`, `BB.01`) and fetch them via `vk_be_get_architectural_rule`.
