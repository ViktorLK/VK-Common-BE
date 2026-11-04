---
id: '#136'
title: 'Separation of Forgetting & Erasure: Index Archival Pruning vs. GDPR/APPI Cascade Physical Deletion'
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
  - OR.01
  - OR.02
  - FS-11
  - ARCH-02
  - BB.01
  docs: []
commits: []
---
# Task: Separation of Forgetting & Erasure: Index Archival Pruning vs. GDPR/APPI Cascade Physical Deletion
**ID**: #136
**Target**: `src/BuildingBlocks/AI.Engram/Reclamation`
**Ref**: CS.01,OR.01,OR.02,FS-11,ARCH-02,BB.01

## 📝 Description
### 1. Problem / Context
`DefaultMemoryReclamationService` currently runs `IVKDecayStrategy` to overwrite DB records and uses `IVKPruningStrategy` to permanently delete low-score memories from `IVKMemorySemanticRepository` and `IVKVectorStore` (`VKPruneAction.Delete`). Because storage is cheap and automatic deletion is irreversible (`ADR-002`), "forgetting" must be handled by L2 presentation while `Reclamation/` is restricted to index cost management (cold archival) and explicit administrative/compliance physical deletion (GDPR/APPI user erasure requests & hard retention TTL).

### 2. Proposed Changes
- **Step 1 (Restrict `IVKPruningStrategy` to Index Archival)**:
  - Remove `IVKDecayStrategy` mutation loop and automatic low-score physical deletion (`VKPruneAction.Delete`) from `DefaultMemoryReclamationService`.
  - Narrow `IVKPruningStrategy` and `DefaultPruningStrategy` to index cost governance (`VKPruneAction.ArchiveToColdIndex` / `EvictFromHotVectorIndex`), evicting stale embeddings from the hot vector index while preserving L0 and L1 records intact for future rebuilds.
- **Step 2 (Deterministic GDPR / APPI & Hard-TTL Cascade Physical Deletion)**:
  - Introduce `IVKMemoryErasureService` (or `PurgeBySubjectAsync` / `PurgeByEpisodeIdsAsync` / `PurgeExpiredHardTtlAsync` on `IVKMemoryReclamationService`) for explicit user deletion requests and statutory retention limits (`HardTtl`).
  - Traverse `SourceEpisodeIds` from target L0 episodes (or tenant/session/user scope) to deterministically cascade physical deletion across:
    1. L0 `IVKEpisodeRepository`
    2. L1 `IVKMemorySemanticRepository`, `IVKMemoryStructuredRepository`, `IVKMemorySummaryRepository`
    3. Hot/cold vector chunks in `IVKVectorStore`
  - Emit structured audit logs (`OR.01`) with PII scrubbed (`FS-11`, `OR.02`) returning a `VKErasureAuditReport`.

### 3. Acceptance Criteria
- [ ] Background reclamation never physically deletes memories due to low decay scores (`ADR-002`); it only archives/evicts from hot vector indexes.
- [ ] Explicit erasure API (`IVKMemoryErasureService`) performs complete physical cascade deletion across L0, L1 (via `SourceEpisodeIds`), and `IVKVectorStore` for GDPR/APPI and `HardTtl` expiration.
- [ ] Unit tests verifying zero data loss during routine pruning and 100% cascade removal on explicit GDPR erasure (`DL.01`).

## ✅ DoD (Definition of Done)
- [ ] Separation of Forgetting & Erasure: Index Archival Pruning vs. GDPR/APPI Cascade Physical Deletion
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み