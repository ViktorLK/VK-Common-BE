---
id: '#134'
title: 'Persistent Tiered Compression: Outbox Job State, Session Serialization, & Prompt-Cache Prefix Stabilization'
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
  - FS-06
  - ARCH-02
  - ARCH-05
  - OR.03
  docs: []
commits: []
---
# Task: Persistent Tiered Compression: Outbox Job State, Session Serialization, & Prompt-Cache Prefix Stabilization
**ID**: #134
**Target**: `src/BuildingBlocks/AI.Engram/Compression`
**Ref**: CS.01,CS.03,FS-06,ARCH-02,ARCH-05,OR.03

## 📝 Description
### 1. Problem / Context
`CompressionJobQueue` currently uses an in-memory `BoundedChannelOptions(100)` with `FullMode = DropWrite`. If the host restarts, if replicas scale out, or if burst traffic exceeds 100 queued sessions, compression jobs are silently lost (`ADR-005`). Additionally, rolling summaries on every turn invalidate LLM provider prompt caches, and incoming requests that arrive before a background compression job finishes risk exceeding the context token window.

### 2. Proposed Changes
- **Step 1 (Persistent Outbox & Job State Port)**:
  - Define `IVKCompressionJobStore` / `IVKCompressionOutboxRepository` (and optional `VK.Blocks.BackgroundJobs` adapter port) in `Compression/Protocols/`, with a co-located `InMemoryCompressionJobStore` fallback (`ARCH-05`) that explicitly documents next-trigger compensation semantics.
  - Persist job lifecycle states (`Pending`, `Running`, `Completed`, `Failed`, `DeadLettered`, `Cancelled`) along with retry counts and failure diagnostics.
- **Step 2 (Deterministic Idempotency Key & Per-Session Serialization)**:
  - Enforce a deterministic idempotency key composed of `(TenantId, SessionId, StartTurnIndex, EndTurnIndex)` (`FS-06`) so duplicate triggers or worker retries never generate duplicate summary nodes.
  - Guarantee strict per-session serialization via `IVKSessionCompressionLock` so concurrent turns in the same session are compressed in monotonic turn order.
- **Step 3 (Prompt-Cache-Friendly Batch Thresholds & Degraded Truncation Fallback)**:
  - Keep `RetainRecentTurns` as raw L0 episodes and only batch-compress turns when un-summarized tokens cross `SummaryTriggerTokenThreshold`, stabilizing the prompt prefix for LLM KV/prompt caching.
  - In the `Before` weaving stage, if an async compression job for the session is still `Pending` or `Running` and raw turns exceed `TokenBudget`, apply a deterministic temporary middle/oldest-turn truncation fallback (without mutating L0 storage) so the active turn degrades gracefully without failing.

### 3. Acceptance Criteria
- [ ] Compression jobs use deterministic idempotency key `(TenantId, SessionId, StartTurnIndex, EndTurnIndex)` (`FS-06`).
- [ ] Per-session distributed lock prevents concurrent out-of-order compression on the same session.
- [ ] Graceful temporary truncation fallback operates when compression is still in-flight at request time.
- [ ] Full cancellation, retry with backoff, and DLQ handoff (`IVKCompressionDlqHandler`) verified via unit tests (`DL.01`, `CS.03`).

## ✅ DoD (Definition of Done)
- [ ] Persistent Tiered Compression: Outbox Job State, Session Serialization, & Prompt-Cache Prefix Stabilization
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み