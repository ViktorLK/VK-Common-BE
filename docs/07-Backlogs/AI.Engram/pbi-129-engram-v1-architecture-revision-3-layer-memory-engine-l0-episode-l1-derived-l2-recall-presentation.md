---
id: '#129'
title: 'Engram v1 Architecture Revision: 3-Layer Memory Engine (L0 Episode / L1 Derived / L2 Recall & Presentation)'
module: AI.Engram
category: feature
parent: 
legacy_id: 
type: feature
priority: P1
status: todo
sprint: 
points: 
rank: 
started: 
carry_over_count: 0
ready: false
estimate: 
created: 2026-11-03
closed: 
refs:
  rules:
  - ARCH-01
  - ARCH-02
  - BASE-01
  - CS.01
  docs:
  - /src/BuildingBlocks/AI.Engram/README.md
commits: []
---
# Task: Engram v1 Architecture Revision: 3-Layer Memory Engine (L0 Episode / L1 Derived / L2 Recall & Presentation)
**ID**: #129
**Target**: `src/BuildingBlocks/AI.Engram`
**Ref**: ARCH-01,ARCH-02,BASE-01,CS.01,/src/BuildingBlocks/AI.Engram/README.md

## 📝 Description
### 1. Problem / Context
Current `VK.Blocks.AI.Engram` treats memory management primarily as a storage-mutating token budgeting engine:
- Decay (`IVKDecayStrategy`) directly mutates and overwrites `Importance` scores in the database (`VKMemorySemantic`) during background reclamation cycles, and `IVKPruningStrategy` permanently deletes decayed memories (`ADR-002` violation risk).
- Summary (`VKMemorySummary`) and structured facts (`VKMemoryStructured`) lack immutable provenance links (`SourceEpisodeIds`) back to raw dialogue/events, making full re-extraction on model upgrades or deterministic GDPR cascade deletion impossible.
- Contradiction arbitration overwrites or coarsely marks facts without bi-temporal validity (`ValidFrom` / `ValidTo`), preventing historical queries ("what did the user like previously?").
- Extraction, heuristic LLM scoring (`LlmHeuristicScoringTask`), and contradiction checks require multiple sequential LLM invocations per turn.
- Compression jobs use a volatile in-memory `BoundedChannel` (`FullMode = DropWrite`), risking job loss on restart.
- Reminders (`MemoryReminder`) are mixed into memory stores rather than delegated to `VK.Blocks.Workflow`.

### 2. Proposed Changes
Re-architect `VK.Blocks.AI.Engram` into a 3-Layer Context Engineering & Memory Architecture that decides "what to show the model in this single turn":
1. **L0 Episode Log (`Episodes/`)**: Append-only, immutable, lossless event/dialogue store (`IVKEpisodeRepository`, `VKEpisode`) bridged with `AI.Psyche` (`IVKEchoStore`). Serves as the single source of truth.
2. **L1 Derived Memory (`MemorySemantic/`, `MemoryStructured/`, `MemorySummary/`)**: Fully reconstructible from L0 via `SourceEpisodeIds`. Bi-temporal validity (`ValidFrom`, `ValidTo`, `CreatedAt`), entity-relationship links on structured KV, and learned behavioral rules/preferences (procedural rules learned from dialogue live in L1; static persona stays in `AI.Psyche`). Remove `MemoryReminder` in favor of a `VK.Blocks.Workflow` outbound port.
3. **Single-Pass Extraction & Scoring (`Consolidation/`, `Scoring/`)**: Unify fact extraction, category, importance, emotional intensity, and `ADD/UPDATE/DELETE(Invalidate)/NOOP` arbitration into a single `VK.Blocks.AI.Eidos` schema-driven LLM call. Retain `RuleBasedScoringTask` and `EmotionalImpactScoringTask` as cheap pre/post-processors; retire standalone `LlmHeuristicScoringTask`.
4. **Bi-Temporal Contradiction Arbitration (`Revision/`)**: `IVKContradictionArbitrator` invalidates superseded facts by setting `ValidTo` rather than overwriting records.
5. **Persistent Tiered Compression (`Compression/`)**: Outbox + persistent job state (`IVKSessionCompressionStateRepository` / `VK.Blocks.BackgroundJobs` port), per-session serialization, idempotency key `(SessionId, StartTurnIndex, EndTurnIndex)`, prefix stabilization for prompt caching, and temporary truncation fallback when compression is in-flight.
6. **L2 Recall & Presentation Policy (`Recall/`)**: Hybrid search directly over `VK.Blocks.VectorSearch` (Vector + BM25 + Entity/Category + Bi-Temporal filters, independent of `AI.Corpus`), composite scoring (`Relevance * Recency * Importance`), spaced-repetition reinforcement on recall, token-budget packing, and `IVKRecallPresentationPolicy` (`HumanLikePresentationPolicy` with `IVKDecayStrategy` relocated to L2, deterministic hedging/detail-scaling without extra LLM calls, and `VKCharacterMemoryProfile` configuration).
7. **Erasure vs. Forgetting Separation (`Reclamation/`)**: Restrict `IVKPruningStrategy` to index cost management (archival/cold tiering); implement deterministic physical cascade deletion across L0 and all L1/vector derivatives strictly for GDPR/APPI user requests and hard retention policies.

### 3. Acceptance Criteria
- [ ] All 7 child stories covering L0, L1, Single-Pass Extraction, Bi-Temporal Revision, Persistent Compression, L2 Recall/Presentation, and GDPR/Archival Reclamation are defined, refined (`ready: true`), and linked to this Epic.
- [ ] ADRs (`ADR-001` through `ADR-006`), `README.md`, and `.prompts/module-manifest.md` are aligned with the 3-layer architecture.

## ✅ DoD (Definition of Done)
- [ ] Engram v1 Architecture Revision: 3-Layer Memory Engine (L0 Episode / L1 Derived / L2 Recall & Presentation)
- [ ] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [ ] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認