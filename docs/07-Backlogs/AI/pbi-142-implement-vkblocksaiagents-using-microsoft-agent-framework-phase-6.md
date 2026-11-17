---
id: '#142'
title: Implement VK.Blocks.AI.Agents using Microsoft Agent Framework (Phase 6)
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
commits: []
---
# Task: Implement VK.Blocks.AI.Agents using Microsoft Agent Framework (Phase 6)
**ID**: #142
**Target**: `src/BuildingBlocks/AI.Agents`
**Ref**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Bootstrap new VK.Blocks.AI.Agents module using Microsoft Agent Framework (Microsoft.Agents.AI). Integrate ChatClientAgent wrapping IVKAgent, implement workflows, checkpoint persistence abstractions, and multi-agent group execution.

### 2. Proposed Changes (具体的な改修方針)
- **Target**: `src/BuildingBlocks/AI.Agents`
- **Reference**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## ✅ DoD (Definition of Done)
- [x] Implement VK.Blocks.AI.Agents using Microsoft Agent Framework (Phase 6)
- [x] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [x] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認