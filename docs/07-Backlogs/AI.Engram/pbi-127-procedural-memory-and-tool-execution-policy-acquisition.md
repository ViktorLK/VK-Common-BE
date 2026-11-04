---
id: '#127'
title: Procedural Memory and Tool Execution Policy Acquisition
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
  - ARCH-01
  docs: []
commits: []
---
# Task: Procedural Memory and Tool Execution Policy Acquisition
**ID**: #127
**Target**: `src/BuildingBlocks/AI.Engram/MemoryStructured`
**Ref**: CS.01,ARCH-01

## 📝 Description
### 1. Problem / Context
When an agent attempts a tool call or workflow that fails, or learns an effective technique from user feedback, this procedural knowledge is lost once the session closes or turns are evicted. Conversational memory must feed directly into action decision-making.

### 2. Proposed Changes
- Define `VKProceduralRule` and `IVKProceduralMemoryRepository` to store tool-calling heuristics, execution preferences, and recovery playbooks.
- Implement `IVKProceduralPolicyExtractor` to analyze completed turns and extract action-outcome pairs ("Tool X failed with Parameter Y; succeeded with Parameter Z").
- Expose an injection hook into `AI.Psyche` tool orchestrator / action planner to constrain or guide future tool selections.

### 3. Acceptance Criteria
- [ ] `VKProceduralRule` contract with validation and confidence scoring.
- [ ] Automatic learning pipeline on user correction turns.
- [ ] Prompt/System instructions augmentation for action planning in `AI.Psyche`.

## ✅ DoD (Definition of Done)
- [ ] Procedural Memory and Tool Execution Policy Acquisition
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み

> **Status Change Note (2026-11-03)**: Superseded by the Engram v1 boundary decision: static persona/directives remain in AI.Psyche while dialogue-learned behavioral preferences/rules are stored as bi-temporal L1 MemoryStructured/MemorySemantic entries in AI.Engram.
