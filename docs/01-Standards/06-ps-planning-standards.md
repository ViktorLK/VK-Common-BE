# 06. Planning & Decision Standards (`PS.01`〜`PS.05`)

> **対応ルール**: [`.agents/rules/l3-vk-be-ps-planning-standards.md`](/.agents/rules/l3-vk-be-ps-planning-standards.md) (`PS.01`〜`PS.05`)
> **連携テンプレート集**: **[`docs/00-Blueprints/README.md`](/docs/00-Blueprints/README.md)**

本ドキュメントは、新機能の設計・実装計画（Implementation Plan）、RFC 先行ポリシー、意思決定のトレーサビリティ検証、およびモジュール固有コンテキストの優先順位ルールを定義します。

---

## `PS.01` — Mandatory Architecture Decision Audit
すべての実装計画（[`Plan-ContractAndImplementationBlueprint.md`](/docs/00-Blueprints/Plan-ContractAndImplementationBlueprint.md) に基づく `implementation_plan.md`）には、具体的な変更内容の前に必ず **"Architectural Decision Audit"** セクションを設け、以下を明記します：

1. **Contract Impact**: Level 1 Public API（`IVK...`, `VK...Options`, `Result<T>`）またはコアパターンに影響を与えるか。
2. **ADR Necessity**: `DL.03` に照らして ADR の起票が必要かどうかの明確な `Yes / No` 判定。
   - **ADR が必須となる条件**:
     - 新しいモジュール横断インターフェースの導入
     - `Result<T>` または共通 `VKXxxOptions` のシグネチャ変更
     - 標準 DI 登録シーケンス（`BB.03`）の変更
     - BuildingBlock の Industrial DNA に影響する新規外部依存パッケージの採用
3. **Traceability**: ADR が必要な場合は、タスクリストに `[ ] Generate ADR` を必須項目として追加します。

---

## `PS.02` — Closed-Loop Walkthrough & PR Gate
実装完了後の検証レポート（Walkthrough）および Pull Request（[`.github/PULL_REQUEST_TEMPLATE.md`](/.github/PULL_REQUEST_TEMPLATE.md)）では、**意思決定の閉ループ（Decision Traceability）** を検証します：

- 計画段階で ADR が必要と判定された場合、Walkthrough および PR には [`docs/04-ArchitectureDecisionRecords/`](/docs/04-ArchitectureDecisionRecords) 内の新規作成された ADR ファイルへのリンクを必ず記載します。
- パッケージ公開や破壊的変更を伴うリリース時には、[`08-ReleaseNoteBlueprint.md`](/docs/00-Blueprints/08-ReleaseNoteBlueprint.md) に従って移行ガイド（Before / After コード例）を作成します。

---

## `PS.03` — RFC-First Policy (Top-Down Design)
AI 認知モデル、複雑なステートマシン、分散制御、または新規 BuildingBlock の立ち上げといった **複雑・実験的な機能** については、バックログにタスクを分解する前に必ず [`docs/02-RFCs/`](/docs/02-RFCs) に RFC（提案書）を作成します（テンプレート：[`02-RfcBlueprint.md`](/docs/00-Blueprints/02-RfcBlueprint.md)）。

### RFC の必須3要素
1. **Metaphor (概念メタファー)**: 「なぜ・どのように動くか」を直感的に理解できるメンタルモデル。
2. **Mapping (構造マッピング)**: メタファーの各要素がどのクラス・インターフェースに対応するかの対応表または図。
3. **Code Blueprint (契約ブループリント)**: コアとなる公開インターフェース（`IVK...`）と Options / Models の定義案。

**ワークフロー**:
`RFC ドラフト作成` ➡️ `レビュー・承認 (✅ Approved)` ➡️ `03-Architecture 境界定義` ➡️ `07-Backlogs へのアトミックタスク分解`

---

## `PS.04` & `PS.05` — Contextual Instruction Priority & Evidence-Based Design 🔴

### 1. モジュール固有コンテキストの発見 (`PS.04`)
特定のモジュール（`src/BuildingBlocks/{Module}/`）や実験プロジェクト（`src/Labs/`）の設計・改修を行う際は、グローバル規約だけでなくモジュール固有の境界定義（`.prompts/module-manifest.md` や [`docs/03-Architecture/`](/docs/03-Architecture)）を確認します（AI Agent は MCP ツール `vk_be_get_module_context` を実行）。

### 2. ルール優先順位と競合解決ヒエラルキー
ローカルな実験マニフェストとグローバル標準が競合した場合の優先順位は以下の通りです：

```text
Layer 3: Local Module Prompt (.prompts/)  >  Layer 2: Lab Manifesto (src/Labs/)  >  Layer 1: Global Standards
```

- **Type A ルール（🔴 Logic Bottom Line）の絶対保護**:
  `CS.01`（Result）、`CS.03`（Async）、`CS.06`（決定論的抽象）、`CS.07`（GetRequiredService）、`CS.08`（DB 制約）、`OR.01`（LoggerMessage）、`AP.01`（sealed/VKGuard）などの **Type A ルールは Labs であっても免除されません**。万が一ローカル要件と衝突した場合は、無断で逸脱せず実装計画上で明示的にフラグを立ててユーザーの承認を仰ぎます（`BASE-08`）。
- **Type B ルール（🟡 Industrial Habits）の特例免除**:
  `src/Labs/` 等でローカルマニフェストが `VK` プレフィックスや `Internal/` 構造の免除を明示的に許可している場合に限り、監査時に `🚩 [AP.03] Bypassed per Lab Manifesto` と明記した上で特例に従います。
- **根拠に基づく構造変更 (`PS.05`)**:
  記憶や推測だけでフォルダ構造やクラス命名を変更してはならず、必ず対応する L3 ルール仕様（`01`〜`06` および `.agents/rules/l3-*`）の根拠に基づいて設計・リファクタリングを行います。

