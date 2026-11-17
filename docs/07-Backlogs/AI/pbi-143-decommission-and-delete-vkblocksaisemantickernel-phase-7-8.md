---
id: '#143'
title: Decommission and delete VK.Blocks.AI.SemanticKernel (Phase 7 & 8)
module: AI
category: debt
parent: RFC-001
legacy_id: 
type: debt
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
# Task: Decommission and delete VK.Blocks.AI.SemanticKernel (Phase 7 & 8)
**ID**: #143
**Target**: `src/BuildingBlocks/AI.SemanticKernel`
**Ref**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Migrate all consumers (AI.Engram, Samples) from VK.Blocks.AI.SemanticKernel to AI.Extensions and AI.Agents. Completely delete src/BuildingBlocks/AI.SemanticKernel project directory and clean up Directory.Packages.props. Update manifests and READMEs.

### 2. Proposed Changes (具体的な改修方針)
- **Target**: `src/BuildingBlocks/AI.SemanticKernel`
- **Reference**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## ✅ DoD (Definition of Done)
- [x] Decommission and delete VK.Blocks.AI.SemanticKernel (Phase 7 & 8)
- [x] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [x] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認