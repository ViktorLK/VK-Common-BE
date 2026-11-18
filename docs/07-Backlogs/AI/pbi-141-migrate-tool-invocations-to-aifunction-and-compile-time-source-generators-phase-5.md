---
id: '#141'
title: Migrate tool invocations to AIFunction and compile-time Source Generators (Phase 5)
module: AI
category: task
parent: RFC-001
legacy_id: 
type: task
priority: P2
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
# Task: Migrate tool invocations to AIFunction and compile-time Source Generators (Phase 5)
**ID**: #141
**Target**: `src/BuildingBlocks/AI.Extensions`
**Ref**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Migrate tool and plugin execution from reflection-based KernelFunction / PluginScanner to AIFunction and Source Generators. Eliminate Scrutor and reflection from tool calling per AP.01.

### 2. Proposed Changes (具体的な改修方針)
- **Target**: `src/BuildingBlocks/AI.Extensions`
- **Reference**: /docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md

## ✅ DoD (Definition of Done)
- [x] Migrate tool invocations to AIFunction and compile-time Source Generators (Phase 5)
- [x] 子 Story / Task に分解完了（各子 PBI の parent に本 ID を指定）
- [x] 承認済み RFC ドキュメント（`docs/02-RFCs/`）の紐付け確認