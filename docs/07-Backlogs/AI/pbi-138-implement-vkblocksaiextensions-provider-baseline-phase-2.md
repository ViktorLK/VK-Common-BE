---
id: '#138'
title: Implement VK.Blocks.AI.Extensions provider baseline (Phase 2)
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
- adc4f645
---
# Task: Implement VK.Blocks.AI.Extensions provider baseline (Phase 2)
**ID**: #138
**Target**: `src/BuildingBlocks/AI.Extensions`
**Ref**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Bootstrap new VK.Blocks.AI.Extensions module with Microsoft.Extensions.AI package references. Implement keyed DI registrations for IChatClient and IEmbeddingGenerator supporting OpenAI, Azure OpenAI, Ollama, and Google providers with typed Options (ARCH-13) and VK error mapping (BASE-05).

### 2. Proposed Changes (具体的な改修方針)
- **Target**: `src/BuildingBlocks/AI.Extensions`
- **Reference**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## ✅ DoD (Definition of Done)
- [x] Implement VK.Blocks.AI.Extensions provider baseline (Phase 2)
- [x] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [x] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認