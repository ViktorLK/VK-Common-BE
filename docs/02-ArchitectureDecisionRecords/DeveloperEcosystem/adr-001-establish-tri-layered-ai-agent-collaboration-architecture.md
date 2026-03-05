# ADR 001: Establish Tri-Layered AI Agent Collaboration Architecture

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: DeveloperEcosystem

## 1. Context (背景)

VK-Common-BEプロジェクトにおいて、スケーラブルで信頼性の高い「AI支援開発エコシステム（AI-assisted development tooling ecosystem）」の構築が急務となっていました。従来のアプローチでは、以下のような複数の課題が存在していました。

- **ルールの重複**: システム全体の絶対的なルールが `VIBE_CHECKLIST.md` と `vk-blocks-checklist.md` の間で重複・混在しており、AI（LLM）へのプロンプト指示がブレる原因となっていました。
- **不適切な役割分担**: コードレビュー（Code Normalization）やアーキテクチャ監査（Architecture Audit）など、AIの文脈理解・推論能力・自然言語生成能力を必要とするソフトタスクが、TypeScriptベースの MCP（Model Context Protocol）サーバー内にハードコードされていました。これにより、プロンプトの微調整のたびに再コンパイルが必要となり、メンテナンスオーバーヘッドが極めて高くなっていました。

## 2. Problem Statement (問題定義)

- **保守性の低下**: AIへ渡すプロンプトの調整や新しいタスクの追加が、TypeScriptコードの変更とビルドを伴うため、アジャイルな改善ループが回せない。
- **AI推論能力の制限**: MCP Toolは本来「確定的（Deterministic）なプログラム処理」に向いており、推論を要するタスクを無理に詰め込むと、エラーハンドリングや柔軟な思考プロセスが損なわれる。
- **Single Source of Truthの欠如**: 開発者が守るべきルールや、AIが参照すべきガイドラインの置き場所が散在しており、「どれが最新の正解か」が不明確になっていた。

## 3. Decision (決定事項)

AI開発エコシステムに対して、明確な「関心事の分離（Separation of Concerns）」に基づく **3層アーキテクチャ（Tri-Layered Architecture）** を採用します。

1. **Rules (Always On): 絶対的な憲法**
    - **配置**: `.agents/rules/vk-blocks-checklist.md` （単一ファイルに統合）
    - **役割**: アーキテクチャの絶対的な制約（Resultパターン、Async/Await強制など）。AIがタスクを問わず**常にバックグラウンドで遵守すべき**ルール。
2. **Workflows (Markdown): AIの思考・推論タスク**
    - **配置**: `.agents/workflows/*.md`
    - **役割**: コード正規化、アーキテクチャ監査、PRサマリ生成、ボイラープレート生成など、自然言語の指示書とAIのコード生成能力を組み合わせる領域。再コンパイル不要で即座に改善可能。
3. **MCP Tools (TypeScript): 確定的・プログラミングタスク**
    - **配置**: `mcp/src/index.ts`
    - **役割**: EF Core Migrationの破壊的変更のSQLパース、OpenAPI JSONの解析、ディレクトリの再帰的走査とJSON出力、ファイルの連番計算など、AIには不安定な**絶対的な正確性が求められる機械的処理**に限定する。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 全ての自動化をMCPに集約する (Rejected)**
    - _Approach_: 監査からファイル生成まですべてをTypeScriptで書き、AIはMCPを呼び出すだけにする。
    - _Rejected Reason_: メンテナンスコストが肥大化し、プロンプトエンジニアリングの柔軟性が失われるため。
- **Option 2: RulesをWorkflowに分散させる (Rejected)**
    - _Approach_: 各Workflowの引数やファイル内に、その都度すべてのコーディング規約を記載する。
    - _Rejected Reason_: ルールの変更時に全ファイルを書き換える必要があり、単一責任原則（DRY）に反するため。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive (メリット)**
    - ルール変更や新しいWorkflow追加がMarkdownの編集のみで完結し、開発スピードが劇的に向上する。
    - AIが「思考（Workflow）」と「作業（MCP）」を効果的に使い分けられるようになり、アウトプットの精度と安定性が向上する。
- **Negative (懸念点)**
    - MCPツールとWorkflowの境界線が開発者によってブレる可能性がある。
- **Mitigation (緩和策)**
    - 本ADRおよびエコシステムの全体像を開発チームに共有し、「推論が必要ならWorkflow、計算が必要ならMCP」という明確な判断基準を啓蒙する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装**:
    - 重複していた古い `VIBE_CHECKLIST.md` や `ADR_PROMPT.md` 等の整理・リネームを実施済み。
    - MCPサーバーから `audit_vk_blocks_code` などのAI思考タスクを削除し、純粋な `publish_adr` や `audit_ef_migrations` ツールへ純化。
- **セキュリティ考察**:
    - MCPから不要なファイル書き込み権限を持つ汎用ツールを削除したことで、過剰な権限（Least Privilege原則の違反）を抑制し、Agentの暴走リスクを低減。
