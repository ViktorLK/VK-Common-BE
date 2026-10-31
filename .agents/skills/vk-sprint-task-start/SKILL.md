---
name: vk-sprint-task-start
description: Transition a backlog item to 'doing', set up branch context, and review git commit conventions within a Sprint.
---

# Skill: Task Start (`vk-sprint-task-start`)

Use this skill when beginning work on a specific committed backlog task.

## Steps

1. **Transition Task Status**:
   - Call `vk_be_backlog_transition` from `vk-blocks-be-manager` with:
     - `id`: Target Task ID (e.g. `CORE-024`).
     - `toStatus`: `doing`.
   - The tool updates front-matter and moves the task into the `🏃 Doing` section of `Active_Sprint.md`.

2. **Branch & Commit Conventions**:
   - Suggest the developer create a task branch:
     ```bash
     git checkout -b task/{TASK_ID}-{short-slug}
     ```
   - Remind the developer of commit message syntax so commits can be auto-linked:
     ```text
     feat(core): brief description [{TASK_ID}]
     fix(ai): brief description [{TASK_ID}]
     ```

3. **Read Task Blueprint & Rules**:
   - Read the task file in `docs/07-Backlogs/{Module}/{NNN}-*.md`.
   - Check the `Ref` field for rule IDs (e.g. `CS.01`, `BB.01`) and fetch them via `vk_be_get_architectural_rule`.

