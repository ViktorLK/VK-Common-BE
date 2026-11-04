---
id: '#126'
title: Autonomous Offline Reflection and Memory Reconsolidation Engine
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
  - OR.03
  - ARCH-12
  docs: []
commits: []
---
# Task: Autonomous Offline Reflection and Memory Reconsolidation Engine
**ID**: #126
**Target**: `src/BuildingBlocks/AI.Engram/Consolidation`
**Ref**: CS.01,OR.03,ARCH-12

## 📝 Description
### 1. Problem / Context
Current consolidation only compresses chat turns when token budget overflows. Human brains reconsolidate memories during rest/sleep, synthesizing disparate experiences into higher-level insights and updating underlying beliefs without active prompting.

### 2. Proposed Changes
- Implement `DefaultOfflineReflectionBackgroundService` running during session idle periods or scheduled maintenance windows.
- Define `IVKMemoryReflectionStrategy` for high-order abstraction: cluster recent interactions, identify behavioral trends, infer unstated user goals, and generate meta-insights.
- Introduce Memory Reconsolidation: when a previous memory is retrieved and updated by new context, adjust confidence scores and record mutation audit trails (`IVKMemoryReconsolidationManager`).
- Expose `VKReflectionOptions` for idle timeout detection, batch size, and reflection LLM prompts.

### 3. Acceptance Criteria
- [ ] Hosted background worker (`IHostedService`) safely running without blocking user requests.
- [ ] Generates higher-order synthesized insight entries into `IVKMemoryStructuredRepository` / knowledge store.
- [ ] Circuit breaker and rate limiter on reflection LLM calls (`OR.03`).

## ✅ DoD (Definition of Done)
- [ ] Autonomous Offline Reflection and Memory Reconsolidation Engine
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み

> **Status Change Note (2026-11-03)**: Superseded by Engram v1 single-pass LLM extraction & scoring (Eidos-driven) and bi-temporal contradiction invalidation (ValidFrom/ValidTo).
