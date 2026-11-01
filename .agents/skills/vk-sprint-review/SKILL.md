---
name: vk-sprint-review
description: Close active Sprint, evaluate Goal achievement, generate velocity review and retrospective (KPT), and refresh health dashboard.
---

# Skill: Sprint Review & Retrospective (`vk-sprint-review`)

Use this skill at the end of the 1-week Sprint cycle to record outcomes, reflect, and reset the board.

## Steps

1. **Review Sprint Results**:
   - Inspect `docs/07-Backlogs/Active_Sprint.md`.
   - Discuss completed tasks vs. unfinished tasks and total completed Story Points.
   - Evaluate whether the **Sprint Goal** was achieved (`true` / `false`).

2. **Close Sprint via MCP Tool**:
   - Formulate concise Retrospective (KPT) notes (個人開発向け: 各1~2項目の端的なメモで十分です):
     - **Keep**: 良かったこと・継続したい習慣
     - **Problem**: 発生した課題やボトルネック
     - **Try**: 次の Sprint で試す具体的な改善アクション（※増えすぎないよう最大1〜2個を推奨）
   - Call `vk_be_sprint_close` from `vk-blocks-be-manager` with:
     - `goalMet`: `true` or `false` (Mandatory).
     - `retroNotes`: Formatted KPT text.
     - `tryItems`: Semicolon or newline-separated list of actionable Try items to automatically convert into `icebox` PBIs.
     - `tryModule`: Target module for generated Try items (optional, automatically defaults to the dominant module of the sprint).
   - The tool will:
     - Calculate velocity (`completed_points`).
     - Increment `carry_over_count` on all uncompleted tasks and return them to Product Backlog.
     - Automatically create icebox PBIs for each item in `tryItems` and record their IDs in `try_items`.
     - Archive the Sprint document and update `Active_Sprint.md`.

3. **Update Health Scorecard & Velocity Dashboard**:
   - Call `vk_be_generate_dashboard` to update `docs/07-Backlogs/Health_Scorecard.md` with latest velocity trend, goal achievement rate, and cycle time metrics.
