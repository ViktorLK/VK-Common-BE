---
id: '#130'
title: L0 Immutable Episode Store (IVKEpisodeRepository) & SourceEpisodeIds Provenance Tracking
module: AI.Engram
category: story
parent: '#129'
legacy_id: 
type: feature
priority: P1
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
  - FS-04
  - FS-05
  - ARCH-05
  - BB.01
  docs: []
commits: []
---
# Task: L0 Immutable Episode Store (IVKEpisodeRepository) & SourceEpisodeIds Provenance Tracking
**ID**: #130
**Target**: `src/BuildingBlocks/AI.Engram/Episodes`
**Ref**: CS.01,CS.06,FS-04,FS-05,ARCH-05,BB.01

## 📝 Description
### 1. Problem / Context
`VK.Blocks.AI.Engram` currently lacks an explicit L0 immutable raw experience repository (`IVKEpisodeRepository`). While `VK.Blocks.AI.Psyche` maintains dialogue history via `IVKEchoStore` (`VKEchoTrace`), derived memories (`VKMemorySemantic`, `VKMemoryStructured`, `VKMemorySummary`) do not maintain immutable `SourceEpisodeIds` links back to the raw turns/events that produced them. Without an append-only L0 source of truth and provenance tracking, Engram cannot rebuild L1 memories when LLM models upgrade, nor can it execute verifiable cascade deletion for GDPR/APPI compliance (`ADR-001`).

### 2. Proposed Changes
- **Step 1 (L0 Domain Models & Strongly Typed IDs)**:
  - Create `Episodes/` vertical slice (`EpisodesFeature.cs`, `VKEpisodesOptions.cs`, `VKEpisodesErrors.cs`, `VKEpisodesDiagnosticsConstants.cs`) following `BB.01` and `BB.06`.
  - Define `VKEpisodeId` (`readonly record struct` wrapping `Guid`, `FS-04`) and `VKEpisode` aggregate root (`VKAggregateRoot<VKEpisodeId>`, `IVKTenantScoped`) containing `TenantId`, `SessionId`, `PersonaId`, `TurnIndex`, `Role` / `EventType` (User/Assistant utterance, tool execution, or external event), `RawContent`, `OccurredAt` (event timestamp), `RecordedAt` (`TimeProvider` UTC timestamp, `CS.06`, `FS-05`), and `Metadata` (`IReadOnlyDictionary<string, string>`).
  - Enforce strict immutability on `VKEpisode` (no mutation methods for content or timestamps once created).
- **Step 2 (L0 Repository Contract & Psyche Bridge)**:
  - Define `IVKEpisodeRepository` in `Episodes/Protocols/` with append-only operations (`AppendAsync`, `AppendBatchAsync`), point/batch retrieval (`GetByIdAsync`, `ListByIdsAsync`, `ListBySessionAsync` with pagination `VKPagedResult<VKEpisode>`, `FS-08`), and administrative physical purge (`DeleteByIdsAsync`, `DeleteBySessionAsync`, `DeleteByTenantAsync` for GDPR/retention).
  - Provide `InMemoryEpisodeRepository` (`ARCH-05`) and an adapter/sync stage (`DefaultEpisodeCaptureStage` or bridge to `AI.Psyche`'s `IVKEchoStore`) so every dialogue turn saved in `Psyche` is deterministically recorded as an L0 `VKEpisode` without duplicating in-memory state unnecessarily.
- **Step 3 (Provenance Contract for L1 Derivatives)**:
  - Define `IVKDerivedMemory` (or base contract) requiring `IReadOnlyList<VKEpisodeId> SourceEpisodeIds { get; }` across all L1 aggregates (`VKMemorySemantic`, `VKMemoryStructured`, `VKMemorySummary`) so every derived fact or summary can trace back to its exact L0 raw episodes.

### 3. Acceptance Criteria
- [ ] `VKEpisode`, `VKEpisodeId`, and `IVKEpisodeRepository` implemented with `Result<T>` (`CS.01`), `VKGuard` (`AP.01`), and `CancellationToken` (`CS.03`).
- [ ] `InMemoryEpisodeRepository` co-located default registered via `TryAdd` (`ARCH-05`, `AP.02`).
- [ ] `SourceEpisodeIds` provenance property enforced on L1 memory creation factories.
- [ ] Unit tests covering append-only invariants, session range queries, and provenance linking (`DL.01`).

## ✅ DoD (Definition of Done)
- [ ] L0 Immutable Episode Store (IVKEpisodeRepository) & SourceEpisodeIds Provenance Tracking
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み