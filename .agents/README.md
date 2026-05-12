# VK.Blocks AI ガバナンス・エコシステム

このディレクトリには、人間と AI エージェントのシームレスな協調を実現するための「Industrial DNA（工業製品レベルの設計思想）」が格納されています。すべてのコードがエンタープライズ品質の基準を満たすよう、確定的（Deterministic）なガバナンスモデルを実装しています。

## 🏛️ 三層アーキテクチャ (ADR-001)

[ADR-001](/docs/02-ArchitectureDecisionRecords/DeveloperEcosystem/adr-001-establish-tri-layered-ai-agent-collaboration-architecture.md) に基づき、AI エコシステムは以下の 3 つの責任レイヤーに分離されています：

### 1. [Rules](/.agents/rules/) - 「憲法」

- **目的**: タスクの内容に関わらず遵守すべき、絶対的なアーキテクチャ上の制約。
- **主要ファイル**: [vk-blocks-checklist.md](/.agents/rules/vk-blocks-checklist.md) (Lead Architect 用マスターチェックリスト)。
- **戦略**: 階層制ルール強制 (L1 意識, L2 禁令, L3 動的ロード)。

### 2. [Workflows](/.agents/workflows/) - 「手順書」

- **目的**: アーキテクチャ監査、正規化、ボイラープレート生成など、複雑な推論を必要とするタスクのステップ・バイ・ステップの実行手順。
- **使用方法**: スラッシュコマンド（例：`/vk-audit-architecture`）を通じて呼び出し。
- **設計思想**: AI の自然言語推論能力を最大限に活用し、ツールの再コンパイルなしで柔軟な改善を可能にする。

### 3. [MCP Tools](/src/Tools/McpServer/) - 「メカニクス（機械的処理）」

- **目的**: SQL パース、ディレクトリ走査、JSON 出力など、高い精度と確定性が求められる処理。
- **サーバー**: [`vk-blocks-manager`](/src/Tools/McpServer/) (Model Context Protocol)。
- **設計思想**: AI の推論が不安定になる領域や、数学的・構造的な正確性が不可欠な領域に限定して決定論的なコードを使用する。

---

## 🛰️ 階層制ルール強制プロトコル (Tiered Enforcement)

すべての AI との対話は、マスターチェックリストで定義された **Tiered Strategy** に従います：

| 階層   | 名称                  | 強制レベル                   | 説明                                                                                                                   |
| :----- | :-------------------- | :--------------------------- | :--------------------------------------------------------------------------------------------------------------------- |
| **L1** | **Rule Index**        | **Awareness (意識化)**       | 全ルールの 1 行サマリ。常に AI の視界に入れる。                                                                        |
| **L2** | **Core Prohibitions** | **Hard Constraints (制約)**  | **Type A (🔴)**: 論理/安全ルール。ゼロトレランス。 <br> **Type B (🟡)**: 工業慣習ルール。`src/Labs` 以外では原則厳守。 |
| **L3** | **Dynamic Loading**   | **Context Discovery (発見)** | `vk_get_module_context` を通じたモジュール固有ルールの取得。                                                           |

### ハンドシェイク・プロトコル (The Handshake)

AI エージェントは、すべての回答の冒頭に以下のステータス行を出力することが義務付けられています：
`Active: [L1+L2:{Module}] | Context: {Path} | Sync: Ready`

### 監査とトレーサビリティ

- **成功時**: `Audit: ✅ All constraints satisfied.`
- **違反/例外時**: `Audit: 🚩 [RuleID] {Rationale}` (L2 ルールをバイパスする場合の必須項目)。

---

## 🛠️ 利用ガイド

### 開発者（人間）向け:

- **ワークフローの活用**: `.agents/workflows/` にあるワークフローを使用して、アーキテクチャに準拠した監査やコード生成を実行してください。
- **ルールの参照**: `.agents/rules/` にある Markdown ファイルを、コーディング規約の Single Source of Truth（唯一の正解）として扱ってください。

### AI エージェント向け（指示事項）:

1. **初期化 (Bootstrap)**: 新しいモジュールに入る際は、まず `vk_get_module_context` を呼び出し、コンテキストを初期化してください。
2. **差異の補完 (Differential Loading)**: `vk-blocks-checklist.md` の L3 要件を確認し、現在のコンテキストに不足しているルールのみを `vk_get_architectural_rule` で「トップアップ（追記ロード）」してください。
3. **実行**: 「Lead Architect（首席アーキテクト）」のペルソナを維持しながらタスクを遂行してください。
4. **監査**: 自己修正を行い、最終出力に Handshake 和 Audit ステータスを含めてください。
5. **リフレッシュ**: 対話が長くなり、ルールの注意力が低下した（Lost-in-the-Middle）と感じた場合は、躊躇なく L3 ツールを再実行して法典を上下文の「新鮮領域」に引き戻してください。

---

## 🧠 AI コンテキスト・メモリ管理 (AI Context & Entropy Governance)

長時間のペアプログラミングにおいて、AI の精度を維持するための高度なガバナンス理論を採用しています：

### 1. 「中間での紛失（Lost-in-the-Middle）」への対策
LLM はコンテキストの中間にある情報を失念しやすい性質があります。重要なアーキテクチャ上の決定を行う前（特に `/vk-audit-*` の実行時）に **Step 0** としてコンテキストを Bootstrap することで、法典を常に AI の「最新の注意力（Fresh Attention）」の範囲内に保持します。

### 2. コンテキスト・エントロピーの制御
対話が重なり、コードの差分や説明が増えるほど、コンテキスト内の「ノイズ（エントロピー）」が増大します。
- **解決策**: **Differential Loading（差分ロード）** 戦略を採用します。まず `vk_get_module_context` で「工業 DNA」を一括ロードし、監査に必要な特定の L3 ルールのみを最小限に追記することで、コンテキスト・ウィンドウの有効活用と、決定論的な出力を両立させます。

### 3. 確定的な位置合わせ (Deterministic Alignment)
L3 ツールの出力は、通常の会話履歴よりも高い優先度で AI に処理されます。ルールを「思い出す」のではなく「再取得」することで、ヒューリスティックな推論による「ルールの漂移（Rule Drift）」を完全に排除します。

---

> [!TIP]
> このエコシステムは **自己文書化 (Self-Documenting)** されるよう設計されています。新しいアーキテクチャルールを追加した場合は、`vk-blocks-checklist.md` の L1 インデックスと `.agents/rules/` 内の該当ファイルを更新してください。
