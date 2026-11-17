---
id: '#144'
title: Audit AI.Guardrails slice and reconcile overlapping Governance contracts
module: AI
category: debt
parent: 
legacy_id: 
type: debt
priority: P3
status: icebox
sprint: 
points: 2
rank: 
started: 
carry_over_count: 0
ready: true
estimate: 
created: 2026-11-13
closed: 
refs:
  rules: []
  docs: []
commits: []
---
# Task: Audit AI.Guardrails slice and reconcile overlapping Governance contracts
**ID**: #144
**Target**: `src/BuildingBlocks/AI/Guardrails`
**Ref**: N/A

## 📝 Description
### 1. Problem / Context (現状の課題と背景)
Perform a full architecture and semantic audit on src/BuildingBlocks/AI/Guardrails (Content, Injection, Privacy). Reconcile overlapping guardrail protocols between AI/Governance (IVKAIPromptInjectionDefender, IVKAIPIIMasker, IVKAIGuardrail) and AI/Guardrails (IVKInjectionDetector, IVKPrivacyFilter, IVKModerationEngine).

### 2. Proposed Changes (具体的な改修方針)
- **Target**: `src/BuildingBlocks/AI/Guardrails`
- **Reference**: N/A

## ✅ DoD (Definition of Done)
- [ ] Audit AI.Guardrails slice and reconcile overlapping Governance contracts
- [ ] `dotnet build -warnaserror` がエラー・警告ゼロで通る (`verifyBuild: true`)
- [ ] 振る舞いの変更に対するテストの追加・更新、全パス通過 (`DL.01`, `BASE-07`)
- [ ] 新規の Type A ルール違反がないこと (`CS.01`, `CS.06`, `OR.01`, `AP.01`)
- [ ] 未追跡の裸の `// TODO` がないこと（残存負債は PBI 化済み） (`DL.02`, `DL.04`)
- [ ] 公開契約またはアーキテクチャパターン変更時の ADR 作成・更新 (`DL.03`)
- [ ] Commit メッセージに `[#NNN]` が含まれ、main にマージ済み