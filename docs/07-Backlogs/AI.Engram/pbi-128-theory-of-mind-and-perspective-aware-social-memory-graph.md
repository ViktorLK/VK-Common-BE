---
id: '#128'
title: Theory of Mind and Perspective-Aware Social Memory Graph
module: AI.Engram
category: story
parent: '#123'
legacy_id: 
type: feature
priority: P2
status: dropped
sprint: 
points: 8
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
  - FS-07
  - FS-12
  docs: []
commits: []
---
# Task: Theory of Mind and Perspective-Aware Social Memory Graph
**ID**: #128
**Target**: `src/BuildingBlocks/AI.Engram/Revision`
**Ref**: CS.01,FS-07,FS-12

## 📝 Description
### 1. Problem / Context
In multi-user conversations, group chats, or collaborative agent ecosystems, memory cannot simply be shared uniformly. True social interaction requires Theory of Mind (ToM): tracking who knows what, what entity A believes entity B knows, and enforcing epistemic access boundaries (e.g. secrets shared by user A must not leak to user B).

### 2. Proposed Changes
- Define `VKTheoryOfMindGraph` and `VKEpistemicPerspective` abstractions (`ObserverId`, `TargetEntityId`, `BeliefState`, `ConfidentialityTier`).
- Implement `IVKPerspectiveMemoryFilter` to evaluate whether a recalled engram is permissible to express in the presence of current active participants.
- Provide privacy boundary guards preventing cross-participant knowledge contamination.

### 3. Acceptance Criteria
- [ ] `VKEpistemicPerspective` metadata model integrated into all memory types.
- [ ] Strict filtering at retrieval boundary preventing disclosure of confidential third-party memories.
- [ ] Comprehensive unit tests verifying privacy preservation in simulated group dialogue scenarios.

## ✅ DoD (Definition of Done)
- [ ] Theory of Mind and Perspective-Aware Social Memory Graph
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み

> **Status Change Note (2026-11-03)**: Deferred to Engram v2 roadmap (perspective-specific views / character policies) in favor of the foundational L0/L1/L2 separation in v1.
