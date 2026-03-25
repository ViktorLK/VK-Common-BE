# ADR 001: Standardizing on Azure Blob Storage API with Advanced Cloud-Native Features

- **Date**: 2026-03-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Blob Module Optimization & Feature Expansion

## 2. Context (背景)

`VK.Blocks.Blob` モジュールは、初期の単純な CRUD 操作から、エンタープライズレベルの要件（同時実行制御、データ保護、メタデータ検索）に対応できる高度なストレージ抽象へと進化させる必要があります。また、独自の実装（Local FileSystem）を廃止し、Azurite を活用した Azure Blob API への一本化を実施しました。

## 3. Problem Statement (問題定義)

1.  **高度な機能の欠如**: コンテナ管理、リース（ロック）、タグ付け、バージョン管理、ソフトデリートなどのクラウドネイティブな機能が未実装であった。
2.  **一貫性のない削除ロジック**: 物理削除と修復可能なソフトデリートを明示的に使い分ける手段がなかった。
3.  **同時実行制御の欠如**: 複数プロセス間での Blob 競合を防ぐ排他ロック機能が必要。

## 4. Decision (決定事項)

Azure Storage SDK v12 の機能を最大限に活用し、以下のアーキテクチャ設計を決定しました：

1.  **責任の分離 (ISP Compliance)**:
    - `IBlobStorageService`: 基本的なデータ I/O、バージョン管理、削除/修復。
    - `IBlobContainerService`: コンテナのライフサイクル管理。
    - `IBlobLeaseService`: `BlobLeaseClient` を用いた排他ロック（Lease）。
    - `IBlobTagService`: Blob Index Tags (Key-Value) と Metadata の管理、およびタグによる検索。
2.  **データ保護の統合**: `BlobDeleteOptions` を導入し、SoftDelete/PermanentDelete のモード選択と、スナップショットを含めた削除制御を実現。
3.  **バージョン管理の標準化**: `WithVersion` API を内部でカプセル化し、過去のバージョンへのシームレスなアクセスを提供。
4.  **単体クライアントの継続**: すべてのサービスで `BlobServiceClient` (Singleton) を共有し、リソース効率を最適化。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: すべてを一つの巨大なインターフェースに統合**: 小規模なプロジェクトでは使いやすいが、大規模プロジェクトでの保守性（Interface Segregation Principle 違反）を考慮し、機能ごとにインターフェースを分割した。
- **Option 2: 独自形式のタグ実装**: パフォーマンスと検索性能を重視し、Azure Blob Storage API 標準の `Index Tags` を採用。これにより、ストレージレベルでの高速なフィルタリングが可能。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
    - Azurite を使うことで、開発環境でもこれらの高度な機能（Lease や Tags）を本番同様にテスト可能。
    - インターフェースが分割されたことで、依存関係の注入がより細粒度に行える。
- **Negative**:
    - バージョン管理やソフトデリートは、ストレージアカウント側の設定が必須となるため注意が必要。
- **Mitigation**: 
    - `BlobOptions` に `EnableSoftDelete` および `EnableVersioning` フラグを導入。これらが `false` の場合、ライブラリ側で早期に `FeatureDisabled` エラーを返却することで、インフラ構成との不一致による混乱を防止。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ**:
    - Azure Blob API を利用することで、パストラバーサル等のファイルシステム特有の脆弱性は物理的に排除される。
    - **Managed Identity (Azure AD) 認証**: `DefaultAzureCredential` をサポート。接続文字列を排除することで、本番環境での資格情報漏洩リスクを最小化。
- **仮想ディレクトリ (Virtual Directory)**:
    - Azure Blob のフラットな名前空間に対し、`.vk_dir_marker` という名称の 0 字节 Blob とメタデータ (`x-vk-type: Directory`) を用いてフォルダ構造をシミュレート。
    - **パフォーマンス重視**: 列挙時にメタデータを取得 (`BlobTraits.Metadata`) することで、1 回のリクエストでディレクトリ構造を判定可能。Index Tags ではなく Metadata を採用。
    - **ファサードパターン (IBlobService)**:
    - 複数の専門サービス (`IBlobFileService`, `IBlobDirectoryService` 等) を一つの入口から利用できるよう `IBlobService` Facade を提供。
    - **File / Directory モデル**: `Storage` という抽象的な名前から `File` に変更し、ディレクトリとの対比を明確化した。
- **安全性**:
        - `BlobGuard` による集中バリデーション:
            - **Path Traversal 対策**: `..` や `./` を含むパスを拒否。
            - **不正文字の排除**: Windows/Linux との互換性を考慮し、`\`, `:`, `*`, `?`, `"`, `<`, `>`, `|` を禁止。
            - **長さ制限**: Azure Blob 標準の 1024 文字を上限として検証。
        - ユーザーによるディレクトリマーカー (`.vk_dir_marker`) の直接操作を禁止。
        - **同名衝突の防止**: ファイルとディレクトリの同名共存を禁止。作成/アップロード時に双方向でチェックを実施。
        - **大小写不敏感 (Case-Insensitive)**: 衝突判定は `OrdinalIgnoreCase` で行い、OS 標準に近いユーザー体験を提供（Azure 原生の大文字小文字区別による混乱を防止）。
- **Observability (可観測性)**:
    - 外部の `Observability` 共通ライブラリとの重複を避けるため、Blob ライブラリ内へのカスタム `ActivitySource` の追加は見送り、Azure SDK 標準の OpenTelemetry 出力に委ねる設計とした。これにより、ライブラリのポータビリティと軽量性を維持。
- **Result<T> 遵守**: 全操作で例外をキャッチし、`BlobErrors` (ContainerNotFound, LeaseAlreadyHeld 等) にマッピング。

---
