---
id: '#131'
title: 'L1 Derived Memory Stores: Bi-Temporal Schema (ValidFrom/ValidTo), Entity Links, & Reminder Workflow Delegation'
module: AI.Engram
category: story
parent: '#129'
legacy_id: 
type: feature
priority: P1
status: todo
sprint: 
points: 8
rank: 
started: 
carry_over_count: 0
ready: true
estimate: 
created: 2026-11-03
closed: 
refs:
  rules:
  - CS.01
  - CS.06
  - FS-05
  - ARCH-02
  - ARCH-05
  - BB.01
  docs: []
commits: []
---
# Task: L1 Derived Memory Stores: Bi-Temporal Schema (ValidFrom/ValidTo), Entity Links, & Reminder Workflow Delegation
**ID**: #131
**Target**: `src/BuildingBlocks/AI.Engram/MemorySemantic,src/BuildingBlocks/AI.Engram/MemoryStructured,src/BuildingBlocks/AI.Engram/MemorySummary,src/BuildingBlocks/AI.Engram/MemoryReminder`
**Ref**: CS.01,CS.06,FS-05,ARCH-02,ARCH-05,BB.01

## 📝 Description
### 1. Problem / Context
In the current L1 memory stores (`MemorySemantic`, `MemoryStructured`, `MemorySummary`, `MemoryReminder`):
1. `VKMemoryStructured` overwrites values in-place via `UpdateValue()`, destroying historical state, and lacks entity-relationship links.
2. `VKMemorySemantic` has `IsSuperseded` / `SupersededBy`, but lacks bi-temporal validity (`ValidFrom` / `ValidTo` distinguishing when a fact was true in the real world vs. when it was recorded in DB), making point-in-time queries ("what did the user prefer last month?") impossible (`ADR-003`).
3. `MemorySummary` lacks explicit `SourceEpisodeIds` links for incremental/hierarchical traceability.
4. `MemoryReminder` (`IVKMemoryReminderRepository`, `MemoryReminderScanBackgroundService`) is implemented inside `Engram`, violating module boundaries because reminders are scheduled future tasks rather than cognitive memories.
5. Procedural rules learned from conversation (e.g., "don't use honorifics with me", "prefer concise code examples") had an ambiguous boundary between `Psyche` and `Engram`.

### 2. Proposed Changes
- **Step 1 (Bi-Temporal & Provenance Upgrade on `VKMemorySemantic` & `VKMemoryStructured`)**:
  - Add `SourceEpisodeIds` (`IReadOnlyList<VKEpisodeId>`), `Category` (including learned procedural/behavioral preferences such as `BehavioralRule` / `Preference`), `ValidFrom` (`DateTimeOffset`), `ValidTo` (`DateTimeOffset?`, `null` = currently active), and `InvalidatedByEpisodeId` / `SupersededById` to both `VKMemorySemantic` and `VKMemoryStructured`.
  - Distinguish event validity time (`ValidFrom` / `ValidTo`) from system record time (`CreatedAt` / `InvalidatedAt`).
  - Add entity linking (`SubjectEntityId`, `Predicate`, `ObjectEntityId` or `IReadOnlyList<VKEntityLink> EntityLinks`) to `VKMemoryStructured` while keeping static initial persona/directives in `AI.Psyche` and storing dialogue-learned, time-invalidatable behavioral rules/preferences in `Engram` L1.
- **Step 2 (Hierarchical & Incremental `VKMemorySummary` with Episode Links)**:
  - Extend `VKMemorySummary` with `SourceEpisodeIds` (`IReadOnlyList<VKEpisodeId>`) and `ChildSummaryIds` (`IReadOnlyList<VKMemorySummaryId>`) to support full L0-to-L1 rebuildability and lossless provenance across leaf (`Level = 0`) and consolidated (`Level >= 1`) summaries.
- **Step 3 (Deprecate `MemoryReminder` & Delegate to `VK.Blocks.Workflow` Port)**:
  - Remove `IVKMemoryReminderRepository`, `InMemoryMemoryReminderRepository`, and `MemoryReminderScanBackgroundService` from `AI.Engram`.
  - Introduce a lightweight outbound port `IVKReminderWorkflowPort` (e.g., `ScheduleReminderTaskAsync(VKReminderDispatchRequest, CancellationToken)`) with a `NoOpReminderWorkflowPort` default (`ARCH-05`) so extracted reminder intents are immediately handed off to `VK.Blocks.Workflow` without storing scheduled jobs inside `Engram`.

### 3. Acceptance Criteria
- [ ] `VKMemorySemantic` and `VKMemoryStructured` support bi-temporal queries (`AsOf(DateTimeOffset)` and active-only `ValidTo == null`) in their repositories (`IVKMemorySemanticRepository`, `IVKMemoryStructuredRepository`).
- [ ] `VKMemoryStructured` supports entity link metadata and learned procedural/preference categories.
- [ ] `VKMemorySummary` tracks `SourceEpisodeIds` and hierarchical parent/child lineage.
- [ ] `MemoryReminder` storage slice is replaced by the `IVKReminderWorkflowPort` abstraction.

## ✅ DoD (Definition of Done)
- [ ] L1 Derived Memory Stores: Bi-Temporal Schema (ValidFrom/ValidTo), Entity Links, & Reminder Workflow Delegation
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み