---
name: vk-sprint-daily
description: Record structured daily standup (yesterday, today, blockers) into active sprint log.
---

# Skill: Sprint Daily Standup (`vk-sprint-daily`)

Use this skill during active development to record progress or surface blockers.
*(個人開発向けTips: 順調な日は `vk_be_sprint_log` で1行の作業メモを残すだけでも十分です。課題やブロッカーが生じた際、または週の進捗ペースを整えたい時に本スキルを活用してください。)*

## Steps

1. **Check Active Sprint Status**:
   - Inspect `docs/07-Backlogs/Active_Sprint.md`.
   - Review current tasks in `🏃 Doing` and `📝 Todo`, along with current Pace (`🟢 On Track` / `🟡 At Risk` / `🔴 Behind Pace`) and remaining days.

2. **Formulate Daily Standup**:
   - **Yesterday**: What was finished or progressed.
   - **Today**: What will be focused on.
   - **Blockers** (optional): Any obstacles, unexpected API constraints, or architectural doubts. If all previous obstacles are resolved, provide `"なし"` or `"None"` to automatically clear the active blocker banner.

3. **Invoke MCP Tool**:
   - Call `vk_be_sprint_daily` from `vk-blocks-be-manager` with:
     - `yesterday`: Summary of yesterday's work.
     - `today`: Planned focus for today.
     - `blockers`: Optional blocker message. If provided and not empty/none, it will automatically be highlighted in the `🚧 Active Blockers` section of `Active_Sprint.md`.

4. **Address Blockers & Pace Adjustments**:
   - If architectural ambiguity is identified, prompt an ADR via `vk-draft-adr` or rule consultation via `vk_be_get_architectural_rule`.
   - If behind pace or scope needs adjustment, use `vk_be_sprint_scope` to add or remove tasks with a documented reason.
