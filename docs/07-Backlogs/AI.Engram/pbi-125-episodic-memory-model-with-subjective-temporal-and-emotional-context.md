---
id: '#125'
title: Episodic Memory Model with Subjective Temporal and Emotional Context
module: AI.Engram
category: story
parent: '#123'
legacy_id: 
type: feature
priority: P2
status: dropped
sprint: 
points: 5
rank: 
started: 
carry_over_count: 0
ready: true
estimate: 
created: 2026-11-02
closed: 2026-11-03
refs:
  rules:
  - CS.01
  - CS.06
  - FS-05
  docs: []
commits: []
---
# Task: Episodic Memory Model with Subjective Temporal and Emotional Context
**ID**: #125
**Target**: `src/BuildingBlocks/AI.Engram/Models`
**Ref**: CS.01,CS.06,FS-05

## 📝 Description
### 1. Problem / Context
Engram extracts semantic summaries and structured KV pairs, but loses episodic consciousness: the subjective, first-person experience of past events (where, when, with whom, and under what emotional atmosphere). The agent knows facts ("user likes coffee") but cannot recall experiential moments ("last rainy Tuesday when we discussed this project over coffee").

### 2. Proposed Changes
- Define `VKEpisodicEngram` aggregate model incorporating `SubjectiveTimestamp`, `EmotionalValence`, `ArousalLevel`, `InteractionAtmosphere`, and `SensoryContext`.
- Implement `IVKEpisodicMemoryStore` and repository abstractions.
- Add an episodic prompt weaver that formats memories with human-like subjective time cues ("a few weeks ago when we started...", "just earlier today...").
- Integrate with `TimeProvider` (`CS.06`) to compute subjective temporal distance rather than raw timestamps.

### 3. Acceptance Criteria
- [ ] `VKEpisodicEngram` record struct / aggregate implemented following `AP.01`.
- [ ] Relative time descriptor renderer (e.g. `HumanizedRelativeTimeFormatter`) supporting multiple locales.
- [ ] Emotional tagging extractor embedded in Consolidation stage.

## ✅ DoD (Definition of Done)
- [ ] Episodic Memory Model with Subjective Temporal and Emotional Context
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み

> **Status Change Note (2026-11-03)**: Superseded by Engram v1 L0 Immutable Episode Log (IVKEpisodeRepository) and L2 HumanLikePresentationPolicy (IVKDecayStrategy + deterministic hedging/detail-scaling).
