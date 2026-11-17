---
id: '#137'
title: Align Core Ports with pure zero-dependency contract (Phase 1)
module: AI
category: feature
parent: RFC-001
legacy_id: 
type: feature
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
commits:
- cc71f11f
---
# Task: Align Core Ports with pure zero-dependency contract (Phase 1)
**ID**: #137
**Target**: `src/BuildingBlocks/AI`
**Ref**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Audit and align VK.Blocks.AI Core Ports (IVKChatEngine, IVKEmbeddingEngine, IVKSpeechEngine, IVKTranscriptionEngine, IVKImageGenerationEngine, IVKAgent) to guarantee zero external AI dependencies per ADR-020 (Option B). Ensure strict VKResult<T> error returns, Args patterns, and no reference to Microsoft.Extensions.AI.Abstractions in VK.Blocks.AI.

### 2. Proposed Changes (具体的な改修方針)
- **Target**: `src/BuildingBlocks/AI`
- **Reference**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## ✅ DoD (Definition of Done)
- [x] Align Core Ports with pure zero-dependency contract (Phase 1)
- [x] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [x] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認