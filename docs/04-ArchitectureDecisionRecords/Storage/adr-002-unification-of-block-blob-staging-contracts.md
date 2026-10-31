# ADR 002: Unification of Block Blob Staging Contracts

- **Date**: 2026-10-23
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Storage Multipart & Resumable Upload Modernization

## 1. Context (背景)

大容量ファイル（数十MB〜数GB）のクラウドアップロードにおいて、ネットワーク断線への耐性とメモリ効率を担保するため、チャンク分割アップロード（Multipart / Staging）および再開（Resumable）機能が不可欠である。
従来の `VK.Blocks.Storage` には、`IVKMultipartUploadService` と `IVKResumableUploadManager` という2つの類似した抽象が並存していた。しかし、これらはインターフェース宣言のみで `Storage.Azure` 側に具象実装が存在せず、DI コンテナへの登録も行われていなかった（BASE-06 Radical YAGNI 違反・形骸化）。

また、Azure Blob Storage の大容量ファイル処理モデルは **Block Blob Staging（`StageBlock`）およびコミット（`CommitBlockList`）** という明確なプリミティブを提供している。

## 2. Problem Statement (問題定义)

1. **二重定義による認知負荷と未実装**: `MultipartUpload` と `ResumableUpload` でセッション管理やインターフェースが重複し、利用者がどちらを使うべきか判断できない上、実際にはどちらも Azure で動作しない。
2. **直伝アップロードプロトコルとの不整合**: ブラウザやモバイルクライアントがバックエンドを経由せず、直接 Azure Blob に Block を並行 Put（StageBlock）し、最後にバックエンドが原子的に Commit して検証を行う「直伝 Staging」モデルに対応したプリミティブが欠落している。
3. **トランザクション的検証の欠如**: チャンクがすべてアップロードされた後、コミット時に全体の SHA-256、Content-Type、魔数、およびサイズ上限を検証し、違反時に即時ロールバック（破棄）するライフサイクルが存在しない。

## 3. Decision (決定事項)

### 3.1 冗長インターフェースの統合と廃止（破壊的変更）
- `IVKResumableUploadManager` を廃止・削除する。
- `IVKMultipartUploadService` を、Azure Block Blob の Staging セマンティクスを忠実に抽象化した **`IVKBlockUploadService`** に統合・改名する。

### 3.2 `IVKBlockUploadService` の中核契約
1. **`StageBlockAsync`**:
   - 指定された BlockId（Base64 エンコード文字列）と Stream チャンクを Azure にステージングする。
2. **`CommitBlockListAsync`**:
   - ステージングされた BlockId リストを順序通りに結合・コミットし、単一の Blob を生成する。
   - `VKStorageUploadCommitOptions` を通じて、Content-Type、インデックスタグ、カスタムメタデータ、および `IfNoneMatch` 等の条件を原子的に適用する。
3. **`GetStagedBlockListAsync`**:
   - クライアントが切断・再開時に「既にどのブロックがステージング完了しているか」を照会できるようにし、確実なレジューム（断点続行）を可能にする。

### 3.3 コミット時検証パイプライン
- コミット処理時に `IVKStorageContentValidator` および `IVKStorageScanHook`（ウイルススキャン）を実行可能とする。
- SHA-256 チェックサムが要求されている場合、コミット後の Blob プロパティまたはメタデータに `vk-sha256` として付与する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: `IVKResumableUploadManager` を独自の状態DB（Redis/RDBMS）を用いて実装する
- **Rejected Reason**: Azure Block Blob 自体が未コミットブロック一覧（Uncommitted Block List）を最大7日間保持する機能を標準で持っている。外部状態ストアを持つと分散トランザクションや孤児レコード問題が発生し、アーキテクチャが過度に複雑化するため。

### Option 2: 既存の `IVKMultipartUploadService` のシグネチャをそのまま維持する
- **Rejected Reason**: 従来のシグネチャは AWS S3 の MultipartUploadSession の流儀を模しており、Azure の柔軟な Block Staging 直伝モデルとミスマッチであったため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **API の一元化**: 分割・大容量アップロードの契約が `IVKBlockUploadService` 1つに集約され、迷いがなくなる。
- **高スループット & 直伝親和性**: Azure の Block Blob 特性を最大限に生かし、クライアントからの並行直伝 StageBlock とバックエンドでの安全な Commit が実現可能になる。
- **完全な実装保証**: 空虚なインターフェースではなく、`Storage.Azure` で完全に動作する実体を供給する。

### Negative
- **破壊的変更**: 既存の `IVKMultipartUploadService` / `IVKResumableUploadManager` を参照していたコードは移行が必要となる。

### Mitigation
- アプリケーション層向けに、1つのファイルを自動的にチャンク分割してアップロードする高レベルのユーティリティ／拡張メソッド（Facade）を整備する。
