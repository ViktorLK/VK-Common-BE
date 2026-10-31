# ADR 001: Deprecation of Public URL and Enforcement of User Delegation SAS

- **Date**: 2026-10-23
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Storage & VK.Blocks.Storage.Azure Security Hardening

## 1. Context (背景)

エンタープライズクラウドストレージにおいて、保存されたデータ（Blob）はデフォルトで完全プライベートとし、インターネットからの直接アクセスを禁止することがセキュリティ原則（Zero Trust）の基本である。
従来の `VK.Blocks.Storage` および `Storage.Azure` の設計では、`IVKStorageFileService` および `IVKStorageUriGenerator` に `GetPublicUrl` メソッドが存在しており、Blob の生エンドポイント URL を直接公開する手段が提供されていた。

さらに、`Storage.Azure` の `GenerateSasUri` 実装は `BlobClient.GenerateSasUri`（Storage Account 共有キー）に依存しており、マネージド ID（`DefaultAzureCredential` / `IAzureCredentialProvider`）による認可環境では `CanGenerateSasUri == false` となり例外・失敗する重大な欠陥が存在していた。

## 2. Problem Statement (問題定義)

1. **ゼロトラスト原則との乖離**: `GetPublicUrl` を公開 API として提供することは、ストレージコンテナがパブリックアクセス可能であることを暗黙的に前提としており、機密漏洩リスクおよびセキュリティコンプライアンス（SOC2、ISO27001）違反の原因となる。
2. **Managed Identity 下での SAS 生成機能不全**: クラウドネイティブ環境では Storage Account Key の直接埋め込みは禁止され、Managed Identity が標準となる。現行実装では MI 接続時に SAS の生成が一切できず、大ファイル直伝やセキュアな一時 URL 発行が完全にブロックされる。
3. **用途に応じた SAS 認可粒度の不足**: アップロード専用（Write/Create のみ）、ダウンロード専用（Read のみ、Content-Disposition 強制、IP 制限）の区別が型安全に定義されていない。

## 3. Decision (決定事項)

### 3.1 `GetPublicUrl` の完全廃止（破壊的変更）
- `IVKStorageFileService` および `IVKStorageUriGenerator` から `GetPublicUrl` を完全に削除する。
- 全てのファイルアクセスは、短命（Short-lived）な SAS URI またはバックエンドプロキシストリーミング（`DownloadAsync`）を経由することを義務付ける。

### 3.2 User Delegation SAS の標準化
- `Storage.Azure` において、Managed Identity 接続時は `BlobServiceClient.GetUserDelegationKeyAsync` を用いた「User Delegation SAS（ユーザー委任 SAS）」の生成を標準とする。
- パフォーマンス維持のため、`TimeProvider` を注入したスレッドセーフな `UserDelegationKeyCache` を導入し、有効期限（最大7日間）に基づく自動リフレッシュを行う。
- 接続文字列（アカウントキー）が明示されているレガシー/ローカル開発環境では、引き続きアカウントキーベースの SAS 生成をサポートする（明示的な分岐、暗黙フォールバック禁止 / AP.06）。

### 3.3 型安全な SAS 要求モデルの整備
- `GenerateUploadSasUri`: 作成・書き込み（Create/Write）権限に限定し、単一 Blob パスおよび Content-Type 制约を付与。
- `GenerateDownloadSasUri`: 読み取り（Read）権限に限定し、`Content-Disposition`（ダウンロード時の強制ファイル名指定やインライン表示制御）および必要に応じた IP アドレス制限を付与。

## 4. Alternatives Considered (代替案の検討)

### Option 1: `GetPublicUrl` を残し、警告ログを出力する
- **Rejected Reason**: 破壊的変更を恐れて非推奨 API を残すと、開発者が安易に呼び出し続け、パブリックコンテナ構成の温床となるため。今回は破壊的変更を受け入れる方針が合意された。

### Option 2: 毎回 UserDelegationKey をリモート取得する
- **Rejected Reason**: SAS 生成のたびに Azure Storage リソースへの REST 往復が発生し、レイテンシと課金・スロットリング（HTTP 429）の重大なリスクとなるため。ローカルキャッシュが必須。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全なプライベート化**: ストレージリソースが外部から直接推測・アクセスされるリスクを根本から排除。
- **MI 準拠**: Entra ID / Managed Identity の本番環境で直伝用 SAS が安定して動作する。
- **コンプライアンス適合**: 最小権限の原則に基づくセキュアな URL 共有が可能になる。

### Negative
- **後方互換性の喪失**: 既存コードで `GetPublicUrl` を呼び出していた箇所はコンパイルエラーとなる。

### Mitigation
- アプリケーション層（`BlobMatterNebula` 等）において、短時間有効な Download SAS 生成呼び出し（またはプロキシ配信エンドポイント）への切り替えガイドを提供する。
