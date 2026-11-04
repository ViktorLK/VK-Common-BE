---
id: '#133'
title: Bi-Temporal Contradiction Arbitration & Non-Destructive Fact Invalidation
module: AI.Engram
category: story
parent: '#129'
legacy_id: 
type: feature
priority: P2
status: todo
sprint: 
points: 5
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
  - AP.01
  - BB.01
  docs: []
commits: []
---
# Task: Bi-Temporal Contradiction Arbitration & Non-Destructive Fact Invalidation
**ID**: #133
**Target**: `src/BuildingBlocks/AI.Engram/Revision`
**Ref**: CS.01,CS.06,FS-05,AP.01,BB.01

## 📝 Description
### 1. Problem / Context
When a user's preference, status, or attribute changes (e.g., "I moved from Tokyo to Osaka" or "I switched from coffee to tea"), overwriting the existing record destroys historical context and prevents the agent from answering temporal questions like "What did I use to like?" or rolling back erroneous arbitrations (`ADR-003`).

### 2. Proposed Changes
- **Step 1 (Bi-Temporal Arbitrator Contract)**:
  - Update `IVKContradictionArbitrator` and `VKContradictionArbitrationResult` in `Revision/` to support bi-temporal invalidation semantics (`ADD`, `UPDATE` -> `InvalidateAndAppend`, `DELETE` -> `InvalidateOnly`, `NOOP`).
  - Include `ArbitrationRationale`, `ConfidenceScore`, `ConflictingMemoryIds`, and `EffectiveTimestamp` (`ValidTo` on the old fact, `ValidFrom` on the new fact) in `VKContradictionArbitrationResult`.
- **Step 2 (Non-Destructive Invalidation in `DefaultRevisionService`)**:
  - When a contradiction or state update (`UPDATE` / `DELETE`) is confirmed against an active `VKMemorySemantic` or `VKMemoryStructured` entry (`ValidTo == null`), never overwrite `Content` or `Value` in-place.
  - Instead, call `existingMemory.Invalidate(validTo: effectiveTime, invalidatedByEpisodeId: currentEpisodeId, supersededById: newMemoryId, timeProvider)` to close the validity window of the prior fact, and insert the new fact as a fresh L1 record with `ValidFrom = effectiveTime` and `ValidTo = null`.
- **Step 3 (Historical & Point-in-Time Query Support)**:
  - Ensure repository queries default to active records (`ValidTo == null`), while exposing `IncludeHistorical` / `AsOf(DateTimeOffset)` query options so recall can retrieve past states when temporal intent is detected.

### 3. Acceptance Criteria
- [ ] `IVKContradictionArbitrator` and `DefaultRevisionService` perform non-destructive `ValidTo` invalidation instead of in-place overwrites (`CS.01`, `CS.06`).
- [ ] Both `VKMemorySemantic` and `VKMemoryStructured` preserve complete revision lineage (`SupersededBy` / `Supersedes` + `SourceEpisodeIds`).
- [ ] Unit tests verifying temporal transitions ("used to like X, now likes Y") and rollback of false-positive arbitrations (`DL.01`).

## ✅ DoD (Definition of Done)
- [ ] Bi-Temporal Contradiction Arbitration & Non-Destructive Fact Invalidation
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み