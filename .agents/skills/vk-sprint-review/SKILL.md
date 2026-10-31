---
name: vk-sprint-review
description: Close active Sprint, generate velocity review and retrospective (KPT), and refresh health dashboard.
---

# Skill: Sprint Review & Retrospective (`vk-sprint-review`)

Use this skill at the end of the 1-week Sprint cycle to record outcomes, reflect, and reset the board.

## Steps

1. **Review Sprint Results**:
   - Inspect `docs/07-Backlogs/Active_Sprint.md`.
   - Discuss completed tasks vs. unfinished tasks.

2. **Close Sprint via MCP Tool**:
   - Ask or formulate concise Retrospective (KPT) notes:
     - **Keep**: 良かったこと・継続したい習慣
     - **Problem**: 発生した課題やボトルネック
     - **Try**: 次の Sprint で試す具体的な改善案
   - Call `vk_be_sprint_close` from `vk-blocks-be-manager` with:
     - `retroNotes`: Formatted KPT text.
   - The tool will:
     - Mark completed tasks in the review section of `docs/07-Backlogs/Sprints/S-{Year}-{Week}.md`.
     - Automatically carry over uncompleted tasks (`status: todo`, `sprint: null`) back to the Product Backlog.
     - Set Sprint status to `closed`.
     - Regenerate `Active_Sprint.md` and `Product_Backlog.md`.

3. **Convert "Try" into Backlogs**:
   - For actionable Try items, use `vk-add-backlog` with `status: icebox` or `status: todo`.

4. **Update Health Scorecard**:
   - Call `vk_be_generate_dashboard` to update `docs/07-Backlogs/Health_Scorecard.md`.

