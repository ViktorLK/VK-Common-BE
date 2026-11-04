---
id: '#135'
title: L2 Recall Engine & HumanLikePresentationPolicy (Hybrid Search, L2 Decay, Spaced Repetition, & Character Parameters)
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
  - CS.03
  - CS.06
  - ARCH-01
  - ARCH-11
  - BB.01
  docs: []
commits: []
---
# Task: L2 Recall Engine & HumanLikePresentationPolicy (Hybrid Search, L2 Decay, Spaced Repetition, & Character Parameters)
**ID**: #135
**Target**: `src/BuildingBlocks/AI.Engram/Retrieval,src/BuildingBlocks/AI.Engram/Reclamation`
**Ref**: CS.01,CS.03,CS.06,ARCH-01,ARCH-11,BB.01

## 📝 Description
### 1. Problem / Context
Engram's core mission is to decide "what to show the model in this single turn" (`L2 Recall & Presentation`). Currently, `Retrieval/` only provides a speculative prefetcher (`DefaultPredictiveMemoryPrefetcher`), while `IVKDecayStrategy` lives in `Reclamation/` and mutates DB storage. Furthermore, three architectural questions were resolved for v1:
1. Search implementation belongs in `AI.Engram` (`Recall/` slice) directly utilizing `VK.Blocks.VectorSearch` (not in `AI.Corpus`, which manages static/lorebook knowledge rules).
2. The 4 strategies remain separate interfaces by lifecycle stage, while character-specific parameters are unified into a configuration model (`VKCharacterMemoryProfile`).
3. Human-like presentation (`ADR-006`) must avoid extra per-turn LLM calls by using deterministic metadata/hedging annotations and multi-grain detail selection.

### 2. Proposed Changes
- **Step 1 (Hybrid Recall Engine & Spaced-Repetition Reinforcement in `Recall/`)**:
  - Evolve `Retrieval/` into the L2 `Recall/` slice (`RecallFeature.cs`, `VKRecallOptions.cs`, `IVKMemoryRecallService`).
  - Implement hybrid retrieval over `VK.Blocks.VectorSearch` combining dense vector similarity + BM25 lexical search + structured filters (`TenantId`, `PersonaId`, `EntityId`, `Category`, and bi-temporal `ValidTo == null`).
  - Compute composite recall score: `CompositeScore = Relevance * RecencyDecay * Importance * EmotionalBoost`.
  - Pack recalled memories strictly within `TokenBudget` and trigger spaced-repetition reinforcement (`RecordAccess` / decay reset) on recalled L1 entries.
- **Step 2 (Relocate `IVKDecayStrategy` to L2 & Define `VKCharacterMemoryProfile`)**:
  - Move `IVKDecayStrategy` from `Reclamation/` to `Recall/Protocols/` as a pure, side-effect-free L2 calculation (`EvaluateRetentionFactor(memory, currentTime, characterProfile)`), never deleting or overwriting stored L1 importance.
  - Define `VKCharacterMemoryProfile` (configurable per persona: `ForgettingRateHalfLife`, `EmotionalSensitivityWeight`, `DetailRetentionRate`, `LowConfidenceHedgingThreshold`, `RecallFailureThreshold`).
- **Step 3 (`IVKRecallPresentationPolicy` & v1 `HumanLikePresentationPolicy`)**:
  - Define `IVKRecallPresentationPolicy` to transform raw recalled L1 items into prompt-ready segments (`VKRecalledMemorySegment`).
  - Implement `HumanLikePresentationPolicy`:
    - **Emotional Priority**: Slower decay and higher recall priority for high emotional intensity memories.
    - **Ambiguity / Hedging**: When retention confidence falls into the hedging band, attach deterministic hedging cues/attributes (e.g., `confidence="low" delivery_hint="uncertain_hedging"`) so the response model naturally phrases it as "たしか〜だったかも" without an extra LLM call.
    - **Recall Failure**: Memories scoring below `RecallFailureThreshold` can be surfaced as "unable to recall details" or omitted while remaining intact in L1 storage.
    - **Uncanny Valley Mitigation**: Avoid verbatim quoting of old timestamps/minutiae; select `SummaryAbstract` or mask fine-grained literals based on elapsed time and `DetailRetentionRate`.

### 3. Acceptance Criteria
- [ ] `IVKMemoryRecallService` executes hybrid search (Vector + BM25 + Entity/Category/Bi-Temporal filters) via `VK.Blocks.VectorSearch` without coupling to `AI.Corpus` (`ARCH-01`).
- [ ] `IVKDecayStrategy` operates in L2 `Recall/` with zero destructive DB writes; recalled memories receive spaced-repetition reinforcement.
- [ ] `IVKRecallPresentationPolicy` and `HumanLikePresentationPolicy` format recalled memories using `VKCharacterMemoryProfile` with zero extra LLM calls.

## ✅ DoD (Definition of Done)
- [ ] L2 Recall Engine & HumanLikePresentationPolicy (Hybrid Search, L2 Decay, Spaced Repetition, & Character Parameters)
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み