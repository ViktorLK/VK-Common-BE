# VK.Tools.McpServer

VK.Blocks エコシステムにおける **AI 連携の「メカニクス（機械的処理）」** を担当するツール群です。
[ADR-001](/docs/02-ArchitectureDecisionRecords/DeveloperEcosystem/adr-001-establish-tri-layered-ai-agent-collaboration-architecture.md) に基づく三層アーキテクチャのうち、**MCP Tools レイヤー** を実装しています。

## 🚀 概要

このプロジェクトは、Model Context Protocol (MCP) を使用して、AI エージェントがプロジェクトの構造、ルール、および管理タスクに対して確定的な操作を行えるようにするためのサーバーです。AI の推論では不安定になりがちな「正確なファイル走査」や「構造化データの抽出」を、C# による堅牢なロジックで提供します。

## 🛠️ 主な機能

### 🧱 BuildingBlock 管理 (`BuildingBlocks`)
- モジュールのコンテキスト（依存関係、オプション、診断情報）の取得。
- 新しい BuildingBlock のボイラープレート生成支援。

### 📜 ADR & ルール管理 (`Adr`, `Rules`)
- アーキテクチャ決定記録 (ADR) のドラフト作成とテンプレート管理。
- [アーキテクチャル・ルール](/.agents/rules/)のメタデータ取得。

### 📋 バックログ & ガバナンス (`Backlogs`, `Lifecycle`)
- `docs/Backlogs/` へのタスク項目の自動追加と同期。
- プロジェクトの正常性スコアカード（ダッシュボード）の生成。

### 🔍 監査 & 分析 (`Migrations`, `Coverage`, `Codebase`)
- EF Core Migration における破壊的変更（Drop Column 等）の自動検出。
- `dotnet test` と `reportgenerator` を連携させたコードカバレッジ分析の実行。
- 大規模なコードベースを AI が読みやすい形式でエクスポートする機能。

### 🧪 テスト支援 (`Testing`)
- OpenAPI (Swagger) 定義を解析し、xUnit と WebApplicationFactory による統合テストのボイラープレートを生成。

### ⚙️ サーバー管理 (`Lifecycle`)
- 開発環境におけるサーバーの安全な停止（再ビルド用）。

## 📂 ディレクトリ構造

- `Program.cs`: MCP サーバーの起動、JSON-RPC プロトコルの制御、およびツール登録。
- `Internal/`: 各ツールの具体的なドメインロジック。
  - `McpTools.BuildingBlocks.cs`: モジュール・依存関係解析。
  - `McpTools.Adr.cs`: ADR ドキュメント管理。
  - `McpTools.Backlogs.cs`: Markdown ベースのバックログ操作。
  - ...その他、各ドメインごとのメカニクス実装。

## ⚙️ セットアップ & 開発

本サーバーは、AI エージェントの設定（`antigravity-config.json` 等）に登録されることで機能します。

### ローカルでの実行・デバッグ
```bash
dotnet run --project src/Tools/McpServer/VK.Tools.McpServer.csproj
```

## 🏗️ 設計原則

1.  **決定論的 (Deterministic)**: AI の推論に頼らず、コードによって一意で正確な結果を返す。
2.  **ステートレス**: サーバー自体は状態を持たず、リポジトリのファイルシステムを Single Source of Truth とする。
3.  **安全なゲートウェイ**: AI が直接ファイルシステムを操作する際の「ガードレール」として機能し、フォーマット違反や破壊的操作を防止する。

---
詳細は [AI ガバナンス・エコシステム (/.agents/README.md)](/.agents/README.md) を参照してください。
