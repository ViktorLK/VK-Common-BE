---
id: '#124'
title: Spreading Activation and Associative Memory Graph Retrieval
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
  - CS.03
  - ARCH-02
  docs: []
commits: []
---
# Task: Spreading Activation and Associative Memory Graph Retrieval
**ID**: #124
**Target**: `src/BuildingBlocks/AI.Engram/Retrieval`
**Ref**: CS.01,CS.03,ARCH-02

## 📝 Description
### 1. Problem / Context
Currently, memory retrieval in AI.Engram is reactive and gating-driven: it only activates when explicit keywords or length thresholds are matched. True human memory relies on associative networks where evoking concept A primes and spreads activation energy to related episodic nodes B and C before they are explicitly mentioned.

### 2. Proposed Changes
- Introduce `IVKAssociativeMemoryNetwork` protocol and in-memory/graph backing store.
- Implement Spreading Activation algorithm (`DefaultSpreadingActivationEngine`) with configurable decay factor and energy threshold.
- Hook into turn-end lifecycle to prime related memory clusters into an active working memory cache ahead of subsequent turns.
- Provide `VKSpreadingActivationOptions` for tuning activation depth, energy decay, and top-k candidate limits.

### 3. Acceptance Criteria
- [ ] `IVKAssociativeMemoryNetwork` interface defined with async activation propagation (`CS.01`, `CS.03`).
- [ ] Benchmarks and unit tests verifying sub-5ms activation spread on a 10,000-node graph.
- [ ] Integration with `AI.Psyche` context hydration pipeline.

## ✅ DoD (Definition of Done)
- [ ] Spreading Activation and Associative Memory Graph Retrieval
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み

> **Status Change Note (2026-11-03)**: Superseded by Engram v1 hybrid Recall (Vector + BM25 + Entity/Category filter + Composite Score). Associative graph spreading activation is deferred to v2+.
