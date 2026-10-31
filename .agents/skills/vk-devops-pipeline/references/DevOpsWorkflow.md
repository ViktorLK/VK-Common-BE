# タスク：エンタープライズ級 Azure DevOps Pipeline 生成 (Enterprise YAML Pipeline)

# ロール設定

あなたは Azure Solution Architect および Senior DevOps Engineer です。Azure DevOps の高度な機能（Environments, Variable Groups, Service Connections など）を活用し、セキュアかつ高効率な CI/CD パイプラインを構築することを専門とします。

# コア環境

- プラットフォーム：Azure Pipelines (YAML)
- ターゲットフレームワーク：.NET 9 / .NET 8
- エージェントプール：ubuntu-latest

# パイプラインステージ要件 (Stages)

## 1. Build ステージ (Stage: Build)

    - パッケージの Restore および Build には DotNetCoreCLI@2 タスクを使用する。
    - キャッシュ最適化：NuGet 依存関係をキャッシュするために Cache@2 タスクを設定する。
    - マルチプロジェクト構成に対応できるよう projects パスパターンを指定する。

## 2. Test & Coverage ステージ (Stage: Test)

    - テスト実行：coverlet.collector を統合して dotnet test を実行する。
    - カバレッジ収集：Cobertura 形式の XML カバレッジ結果を出力する。
    - レポート生成 (HTML)：ReportGenerator ツール（またはスクリプトによるツールインストールおよび実行）を使用して XML を HTML レポートに変換する。
    - PublishCodeCoverageResults@1 タスクを使用して、Azure DevOps の Code Coverage タブに結果を発行する。
    - テスト結果発行：PublishTestResults@2 を使用して、Azure DevOps の Tests タブに詳細テスト結果を表示させる。
    - 閾値チェック：カバレッジが [80%] を下回る場合、パイプラインの実行を失敗させる。

## 3. Deploy ステージ (Stage: Deploy)

    - 依存関係：Build / Test ステージが正常に完了した後にのみ実行する。
    - 環境バインディング：承認（Approvals）フローをサポートするため、Azure DevOps の Environment 機能を活用する。
    - デプロイ先ターゲット：[選択：App Service / Azure Kubernetes Service (AKS) / Docker Container]。
    - 変数管理：Variable Groups や Library を通じてシークレットをセキュアに参照する。

# 高度な仕様

    - Artifacts：ビルド出力および HTML カバレッジレポートを Pipeline Artifacts としてパッケージ化する。
    - 命名規則：明快かつ一貫性のある Stage および Job の命名規則を採用する。
    - エラーハンドリング：テスト失敗時やビルドエラー時にパイプラインが確実に中断・失敗ステータスとなるよう制御する。

# パフォーマンスとセキュリティの最適化

    - Caching：ビルド時間を短縮するための NuGet パッケージキャッシュを構成する。
    - Permissions：最小権限の原則（Least Privilege）に基づきアクセストークンや接続権限を構成する。
    - Concurrency：同一ブランチに対する新たな Push があった場合、進行中の古いパイプライン実行を自動キャンセル（Auto-cancel）するよう構成する。

# 出力要件

    - 完全な azure-pipelines.yml コードを出力すること。
    - カバレッジレポート出力に必要な NuGet パッケージ（例：coverlet.collector など）を簡潔にリストアップすること。
    - Azure DevOps ポータル上で HTML カバレッジレポートを確認・参照する手順を簡潔に説明すること。

# 入力コードフォルダパス (Input Code)

# 出力コードフォルダパス (Output Code)
