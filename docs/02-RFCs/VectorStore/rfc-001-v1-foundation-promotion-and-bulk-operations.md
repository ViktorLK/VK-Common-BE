# RFC-001: VectorStore V1 Foundation Promotion, Bulk Operations & In-Process SQLite Engine

- **Status**: ❌ Superseded *(Historic)*
- **Author**: Architecture Team
- **Date**: 2026-06-22
- **Target Module(s)**: `VK.Blocks.VectorStore`, `VK.Blocks.VectorStore.Sqlite`
- **Related ADRs / RFCs**: 
  - [VectorStore ADR-001](/docs/04-ArchitectureDecisionRecords/VectorStore/adr-001-vectorstore-core-abstractions-and-bulk-operations.md)
  - [VectorStore ADR-002](/docs/04-ArchitectureDecisionRecords/VectorStore/adr-002-vector-similarity-and-indexing-strategy.md)
  - [VectorStore.Sqlite ADR-001](/docs/04-ArchitectureDecisionRecords/VectorStore.Sqlite/adr-001-sqlite-based-in-process-vector-storage-engine.md)
  - [VectorStore.Sqlite ADR-002](/docs/04-ArchitectureDecisionRecords/VectorStore.Sqlite/adr-002-sqlite-vec-native-extension-loader-and-virtual-table-architecture.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **現状の課題 (Current Pain Points)**:
  1. **AI ドメインからの結合**: ベクトルデータベース機能が `VK.Blocks.AI` 名前空間（`AI.VectorStore`）に内包されていたため、LLM 生成推論を伴わない一般的な類似度検索、セマンティックキャッシュ、重複排除などの用途においても、不要な AI ランタイムへの依存を強制されていた。
  2. **1件 Upsert の性能ボトルネック**: ベクトル格納において 1 件ごとの順次 Upsert しか提供されておらず、数百〜数千件のナレッジドキュメント登録時にネットワーク往復および HNSW インデックス再計算のオーバーヘッドが爆発していた。
  3. **開発・CI 環境での重厚インフラ依存**: 開発やテストにおいて外部の Qdrant や クラウド DB を立ち上げる必要があり、ゼロインフラで即時動作する永続化ベクトルストレージが存在しなかった（インメモリは再起動で全消失）。
- **なぜ今やるのか (Why Now / Consumer)**:
  - 汎用検索基盤およびセマンティックキャッシュ基盤として、AI ドメインから自立したルートレベルの BuildingBlock を提供し、大量データの一括登録スループットを確保すると同時に、SQLite ベースの組込実行環境（`VectorStore.Sqlite`）を確立するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - ベクトルストアを独立したルートレベルの `VK.Blocks.VectorStore` へ昇格・再編成する。
  - コレクション単位の CRUD を司る `IVKVectorStore` および `IVKVectorCollection<T>` を抽象化する。
  - 大量データの一括登録を可能にする `IVKBulkCapableVectorStore` および `UpsertBatchAsync` 契約を確立する。
  - 外部 Docker 不要でローカルディスクに ACID 永続化可能な `VK.Blocks.VectorStore.Sqlite`（`sqlite-vec` ネイティブ拡張内包）を提供する。
  - ゼロ外部依存のテスト用インメモリ実装（`InMemoryVectorStore`）を同梱する。
- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - 高度な多段リランキングやクエリ書き換え（`VectorSearch` が管轄）。
  - 生ドキュメント（PDF/Word 等）の解析とチャンキング（`VectorIngest` が管轄）。
  - 十億件級の大規模分散インデックス（Qdrant / Azure SQL DiskANN が担当）。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

- **Metaphor (概念メタファー / メンタルモデル)**:
  - **The Specialized High-Dimensional Warehouse & Pocket Safe (高次元専用倉庫と携帯用小型耐火金庫)**:
    - 従来の「AI 研究室の片隅の保管棚」から「全社独立の高次元物流倉庫」へと昇格。
    - パレット単位の一括コンテナ（Bulk Ingestion）で搬入し、倉庫内の仕分け棚（Named Collection）に高速に一括格納する。
    - また、現場のエッジ端末や実験室向けには、机の引き出しに収まる「小型耐火金庫（SQLite + `sqlite-vec`）」を配備し、同一の操作体系で利用可能にする。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| 高次元物流倉庫 | `IVKVectorStore` | Public Protocol | 名前付きコレクションの取得・管理 |
| 仕分け棚（コレクション） | `IVKVectorCollection<T>` | Public Protocol | ドキュメントとベクトルの CRUD・類似度検索 |
| パレット一括搬入ゲート | `IVKBulkCapableVectorStore` | Public Protocol | 単一トランザクション内でのバッチ書き込み |
| 携帯用小型耐火金庫 | `SqliteVectorStore` | L3 Adapter | ローカルファイルへの組込 ACID 永続化と KNN 検索 |
| 金庫の超高速ダイヤル | `sqlite-vec` ネイティブ拡張 | Native Extension | C/C++ SIMD 加速によるコサイン類似度計算 |
| 簡易保管棚 | `InMemoryVectorStore` | `Internal/` | 開発・テスト用ゼロ依存インメモリフォールバック |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

```csharp
namespace VK.Blocks.VectorStore;

public interface IVKVectorStore
{
    ValueTask<IVKVectorCollection<TRecord>> GetCollectionAsync<TRecord>(
        string collectionName,
        CancellationToken cancellationToken = default);
}

public interface IVKVectorCollection<TRecord>
{
    ValueTask<VKResult<bool>> UpsertBatchAsync(
        IEnumerable<TRecord> records,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<VKVectorSearchResult<TRecord>> SearchStreamAsync(
        ReadOnlyMemory<float> vector,
        VKVectorSearchFilter? filter = null,
        int topK = 10,
        CancellationToken cancellationToken = default);
}
```

---

## 5. ⚖️ Trade-offs & Alternatives (トレードオフと代替案検討)

- **採用案**: `VectorStore` コアを純粋抽象とし、インプロセス永続化として `sqlite-vec` 拡張（C-ABI）を採用。
- **却下された代替案**:
  - *C# 完全自前 HNSW インデックスの実装*: マネージドヒープでのグラフ構造アロケーションが巨大化し、GC 停止（STW）を引き起こすため却下。
  - *DuckDB vss 拡張*: バイナリサイズが大きく（50MB+）、SQLite の軽量性とエコシステム成熟度に及ばないため見送り。

---

## 6. 🔗 包含された意思決定 (Consolidated ADRs)

- [VectorStore ADR-001: ベクトルストアコア抽象とバルク操作プロトコル](/docs/04-ArchitectureDecisionRecords/VectorStore/adr-001-vectorstore-core-abstractions-and-bulk-operations.md)
- [VectorStore ADR-002: ベクトル類似度計算とインデックス戦略](/docs/04-ArchitectureDecisionRecords/VectorStore/adr-002-vector-similarity-and-indexing-strategy.md)
- [VectorStore.Sqlite ADR-001: SQLite によるインプロセスベクトルストレージエンジン選定](/docs/04-ArchitectureDecisionRecords/VectorStore.Sqlite/adr-001-sqlite-based-in-process-vector-storage-engine.md)
- [VectorStore.Sqlite ADR-002: sqlite-vec ネイティブ拡張ローダーと仮想テーブルアーキテクチャ](/docs/04-ArchitectureDecisionRecords/VectorStore.Sqlite/adr-002-sqlite-vec-native-extension-loader-and-virtual-table-architecture.md)
