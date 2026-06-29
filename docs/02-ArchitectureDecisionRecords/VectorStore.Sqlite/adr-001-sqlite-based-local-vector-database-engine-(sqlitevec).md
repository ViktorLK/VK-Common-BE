# ADR 001: SQLite-Based Local Vector Database Engine (SqliteVec)

- **Date**: 2026-06-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/VectorStore.Sqlite

## 1. Context (背景)

ベクトルデータストア（`VectorStore`）を独立したコア層モジュールへと分離したことに伴い、ローカル開発環境やオフラインのエッジ環境、テスト環境において、外部の商用クラウドサービス（Qdrant, Pinecone, Azure AI Search 等）を使わずに、メモリ内（In-Process）で動く軽量なベクトル検索データベースエンジンの需要が高まった。
C# には `InMemoryVectorStore` が存在するが、インメモリストアは再起動時にデータが消失し、かつコサイン類似度の再計算を毎回 C# 上で行うため、数万件以上のベクトルレコードに対しては CPU パフォーマンスやメモリ効率の面で実用に耐えない。

## 2. Problem Statement (問題定義)

再起動後もデータをローカルファイルに永続化でき、かつミリ秒未満の高速なベクトル近傍検索（k-NN）を実行可能な、ゼロインフラ構成かつ軽量なローカル用ベクトルストレージエンジンが必要であった。

## 3. Decision (決定事項)

SQLite データベースおよび高速ベクトル検索拡張モジュールである `sqlite-vec` を利用したローカルベクトルプロバイダ **「`VK.Blocks.VectorStore.Sqlite`」**を導入する。

### 1. 物理パッケージ構成と依存
- `VK.Blocks.VectorStore` インフラインターフェースを実装する。
- SQLite 接続および仮想テーブル操作を制御するため、EF Core の資産を活用しつつ、直接 `sqlite-vec` 拡張モジュールを native ライブラリ経由でロードする構成を採用する。

### 2. `SqliteVectorCollection` と `SqliteVectorStore` の定義
- 仮想テーブル（`vec_f32` 等）を背後に定義し、コサイン類似度の算出を C# ではなく SQLite のネイティブ拡張である `sqlite-vec` に任せる。これにより、C# のマネージド境界を越えずに高速な C++ によるベクトル演算を実行する。
- テーブルスキーマはメタデータやフィルタ条件用のカラムも並列で管理し、フィルタ要件にも対応する。

```
[VK.Blocks.VectorStore.Sqlite]
   |
   +--> SqliteVectorStore 
           |
           +--> References native sqlite-vec extension
           +--> Execute SQL queries with vector parameters (knn_search)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Qdrant のローカルコンテナ（Testcontainers）を必須化する
- **Approach**: ローカル開発時も Docker で Qdrant を起動し、それに対して通信する。
- **Rejected Reason**: ローカル開発者に Docker Desktop のインストールを強制することになり、エッジサーバー環境や一部の制約された CI/CD パイプラインでの実行速度が低下するため却下した。

### Option 2: C# の SIMD 命令による純インメモリ類似度ソートの自作
- **Approach**: C# 上で `System.Numerics.Tensors` の SIMD 機能を用いてベクトル計算を行い、永続化は JSON でディスクに書き出す。
- **Rejected Reason**: インデックス構造の自作やファイルシリアライズのトランザクション管理（ACID 特性）をゼロから再実装する必要があり、車輪の再発明とメンテナンスコスト増大につながるため却下。SQLite を使えばそれらのインフラ機能がタダで手に入る。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **ゼロ構成と高速化**: Docker や外部接続が不要になり、SQLite ファイルが 1 つあればミリ秒単位の類似度検索（RAG 等）がローカルで完全にクローズドに動作する。
- **ACID トランザクション**: メタデータフィルタの登録とベクトルの保存が同一の SQLite トランザクションで原子性を保って実行できる。

### Negative
- **ネイティブバイナリの配布依存性**: `sqlite-vec` は C/C++ で書かれたネイティブ拡張バイナリであるため、ホストマシンの OS（Windows, Linux, macOS）および CPU アーキテクチャ（x64, ARM64）に応じた `.dll` / `.so` / `.dylib` の配布とロード制御が必要となる。

### Mitigation
- アプリケーション起動時に、現在の実行プラットフォームを検知し、該当するネイティブ拡張ファイルをランタイムディレクトリから自動検出して `sqlite3_load_extension` を呼び出す自動ローダーフックを内包する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- SQLite のファイルデータ自体がローカルに保存されるため、機密データを含むベクトルインデックスに対しては、必要に応じて SQLite 接続文字列で暗号化（SQLCipher等）を施す。

## 7. Status
✅ Accepted
