---
id: '#132'
title: Single-Pass LLM Extraction & Scoring Integration with Eidos Structured Output
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
  - FS-12
  - ARCH-02
  - BB.01
  docs: []
commits: []
---
# Task: Single-Pass LLM Extraction & Scoring Integration with Eidos Structured Output
**ID**: #132
**Target**: `src/BuildingBlocks/AI.Engram/Consolidation,src/BuildingBlocks/AI.Engram/Scoring`
**Ref**: CS.01,CS.03,FS-12,ARCH-02,BB.01

## 📝 Description
### 1. Problem / Context
Currently, `AI.Engram` invokes the LLM multiple times for a single turn's memory processing: once in `FactExtractionConsolidationStrategy` (producing raw unvalidated bullet text), once in `LlmHeuristicScoringTask` (evaluating importance score), and again during contradiction arbitration. This multiplies latency, token cost, and failure surfaces (`ADR-004`). Furthermore, parsing raw bullet strings without schema enforcement is brittle (`FS-12`).

### 2. Proposed Changes
- **Step 1 (Unified Structured Extraction Contract via `VK.Blocks.AI.Eidos`)**:
  - Reference or integrate with `VK.Blocks.AI.Eidos` (`IVKSchemaFactory` / `VKContractSpecFactory`) to define a strict, schema-validated output contract `VKMemoryExtractionEnvelope` containing a list of `VKExtractedMemoryCandidate` items.
  - Each `VKExtractedMemoryCandidate` outputs in a single LLM call:
    - `FactContent` (`string`) and optional `SummaryAbstract` (coarse grain for L2 detail-scaling)
    - `Category` (`SemanticFact`, `StructuredProperty`, `BehavioralRule`, `UserPreference`, `EpisodicHighlight`, `ReminderIntent`)
    - `EntityLinks` (`Subject`, `Relation`, `Object`) and optional structured `Key` / `Value`
    - `ImportanceScore` (`float`, `0.0` to `1.0`)
    - `EmotionalSignal` (`Valence`, `Arousal`, `DominantEmotion`, `Intensity` `0.0` to `1.0`)
    - `Operation` (`ADD`, `UPDATE`, `DELETE` / `INVALIDATE`, `NOOP`) and `TargetExistingMemoryId` (when matching candidate memories passed into context)
    - `ValidFrom` (`DateTimeOffset?` if explicitly stated in dialogue)
- **Step 2 (Scoring Pipeline Streamlining)**:
  - Retain `RuleBasedScoringTask` (for fast security rejection like `SensitiveCredentialRule` and deterministic regex routing) and `EmotionalImpactScoringTask` (as a cheap pre-filter / post-adjustment weight).
  - Remove standalone `LlmHeuristicScoringTask` so scoring never triggers a separate LLM round-trip.
  - Repurpose `IVKScoringStrategy` as the write-time composite importance calculator that blends the pre-extraction rule/emotion signals with the LLM-extracted importance score.
- **Step 3 (`IVKConsolidationStrategy` L1 Rebuild Support)**:
  - Update `IVKConsolidationService` and `IVKConsolidationStrategy` to accept L0 `IReadOnlyList<VKEpisode>` batches (carrying `VKEpisodeId`s) and support full or range-based L1 reconstruction from L0 episodes.

### 3. Acceptance Criteria
- [ ] Single LLM call extracts facts, category, importance, emotional intensity, and `ADD/UPDATE/DELETE/NOOP` operation using structured JSON schema validation (`FS-12`, `CS.01`).
- [ ] `LlmHeuristicScoringTask` is removed; `RuleBasedScoringTask` and `EmotionalImpactScoringTask` remain operational as fast pre/post tasks.
- [ ] L1 memories can be deterministically re-extracted from a sequence of L0 `VKEpisode` records.

## ✅ DoD (Definition of Done)
- [ ] Single-Pass LLM Extraction & Scoring Integration with Eidos Structured Output
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み