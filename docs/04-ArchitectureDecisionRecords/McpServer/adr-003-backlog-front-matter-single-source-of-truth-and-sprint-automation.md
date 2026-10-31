# ADR 003: Backlog Front-Matter Single Source of Truth and Deterministic Sprint Automation

- **Date**: 2026-10-31
- **Status**: ✅ Accepted
- **Deciders**: Engineering Team
- **Technical Story**: src/Tools/McpServer, docs/07-Backlogs, .agents/skills

## 1. Context (背景)

VK.Blocks エコシステムでは、技術的負債やアーキテクチャ監査の指摘事項、RFC のタスク分解を `docs/07-Backlogs/{Module}/` 配下に Markdown ファイルとして記録・管理してきた。
これまでバックログの管理は MCP ツール `vk_be_add_backlog_item` による新規作成と、`docs/07-Backlogs/Active_Sprint.md` への手動/簡易追記に留まっていた。

個人開発において Sprint 運用を模倣するにあたり、以下の課題が顕在化していた：
1. **状態管理の多重化と不整合**: タスク本文のマークダウン（`**Status**: 🔴 High`）、`Active_Sprint.md` のチェックボックス（`- [x]`）、および `.Archive/` 物理ディレクトリへの移動状態が手動運用により乖離していた。
2. **Sprint 概念の欠落**: `Active_Sprint.md` が全量バックログのリストと化しており、タイムボックス（1週間）、コミットメント（Sprint Goal）、Review/Retro（振り返り）の記録が存在しなかった。
3. **機械的追跡性（Traceability）の不足**: Git コミットや DoD の達成判定、ルール違反参照が非構造化テキストに埋没しており、MCP やエージェントから一貫して検証・遷移できなかった。

## 2. Problem Statement (問題定義)

- Markdown 本文の特定文字列（絵文字含む）を正規表現や部分一致で判定していたため、フォーマットの揺らぎに極めて脆弱であった。
- 新規タスクの採番ロジックがアクティブフォルダのみを走査していたため、`.Archive/` に存在する過去 ID と重複する危険性があった。
- Sprint の開始・完了（Review/Retro）・タスクの着手/完了のライフサイクルを安全に自動化する仕組みがなく、個人開発における認知負荷を高めていた。

## 3. Decision (決定事項)

1. **YAML Front-Matter を唯一の真実の源泉（Single Source of Truth: SSoT）とする**:
   - 各バックログファイルの先頭に厳密な YAML front-matter（`id`, `title`, `module`, `type`, `priority`, `status`, `sprint`, `created`, `closed`, `refs`, `commits`）を付与する。
   - `Product_Backlog.md`、`Active_Sprint.md`、`Health_Scorecard.md` などの集計ドキュメントはすべてこの front-matter から **完全自動生成** し、手動編集を禁止する。

2. **状態遷移マシン（State Machine）の MCP ツール化**:
   - `vk_be_backlog_transition`: `icebox` → `todo` → `doing` → `done` / `dropped` の遷移を厳密に制御。
   - `done` への遷移時に DoD のチェック完了を確認し、該当ファイルを `.Archive/{Module}/` へ自動移動。関連コミットハッシュの自動記録をサポート。

3. **軽量な個人用 Sprint ライフサイクルの導入**:
   - デフォルト期間を「1週間」とし、`S-{Year}-{ISOWeek}` 形式の識別子を採用。
   - `vk_be_sprint_start`: Sprint 目標とコミットされたタスク ID 群を束ねた Sprint ドキュメントを `docs/07-Backlogs/Sprints/` に生成。
   - `vk_be_sprint_close`: 未完了タスクを carry-over し、完了実績、ベロシティ、および Keep/Problem/Try の振り返りテンプレートを自動集約。

4. **YamlDotNet の採用**:
   - 信頼性の高いオープンソースライブラリ `YamlDotNet`（MIT License）を `VK.Tools.McpServer` に導入し、型安全かつ確実な YAML パース・シリアライズを実現する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: 独自正規表現パーサーによる Markdown 本文の管理継続
- **Pros**: 外部依存ゼロ。
- **Cons**: リスト項目（refs, commits）のパースが脆弱で、将来の拡張に耐えられない。YAML front-matter という業界標準から外れる。

### Option 2: 外部の課題管理 SaaS（GitHub Issues / Jira / Linear）への全面委譲
- **Pros**: 豊富な UI。
- **Cons**: リポジトリローカルで完結せず、オフラインや AI エージェントの自律アクセスに外部 API トークンやネットワーク境界の制約が生じる。

## 5. Consequences (影響とトレードオフ)

- **Positive**:
  - バックログの二重管理が解消され、ダッシュボードや Sprint ボードがワンクリックで正確に再生成可能になる。
  - 採番が `.Archive` を含む全領域を走査するため ID 重複が根絶される。
  - エージェント（Skill）との連携により、Sprint Planning から Retro までの記録が最小限の手間で整備される。
- **Trade-off**:
  - 既存の全バックログファイル（約100件）に対する一括移行（Front-Matter 付与）が必要（移行ツール `vk_be_backlog_migrate` で安全に実施）。

