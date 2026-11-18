---
id: '#140'
title: Implement MEAI Adapter Engines for Chat, Embedding, Speech, Transcription, and Image (Phase 4)
module: AI
category: task
parent: RFC-001
legacy_id: 
type: task
priority: P1
status: done
sprint: 
points: 
rank: 
started: 2026-11-10
carry_over_count: 0
ready: false
estimate: 
created: 2026-11-10
closed: 2026-11-10
refs:
  rules: []
  docs:
  - /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md
commits: []
---
# Task: Implement MEAI Adapter Engines for Chat, Embedding, Speech, Transcription, and Image (Phase 4)
**ID**: #140
**Target**: `src/BuildingBlocks/AI.Extensions`
**Ref**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Implement concrete adapter engines in VK.Blocks.AI.Extensions mapping MEAI / provider clients to IVKChatEngine, IVKEmbeddingEngine, IVKSpeechEngine, IVKTranscriptionEngine, and IVKImageGenerationEngine with zero-allocation mappers and RFC 7807/VKResult error handling. Deprecate Handlebars/Liquid template packages.

### 2. Proposed Changes (具体的な改修方針)
- **Target**: `src/BuildingBlocks/AI.Extensions`
- **Reference**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## ✅ DoD (Definition of Done)
- [x] Implement MEAI Adapter Engines for Chat, Embedding, Speech, Transcription, and Image (Phase 4)
- [x] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [x] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認