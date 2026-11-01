# RFC-002: VectorStore V2 Ecosystem Modernization & Enterprise Governance

- **Status**: ✅ Approved *(Implemented)*
- **Author**: Architecture Team
- **Date**: 2026-10-30
- **Target Module(s)**: `VK.Blocks.VectorStore`, `VK.Blocks.VectorSearch`, `VK.Blocks.VectorIngest`
- **Related ADRs / RFCs**: [ADR-003](/docs/04-ArchitectureDecisionRecords/VectorStore/adr-003-vectordata-abstraction-boundary-and-backend-roles.md), [ADR-004](/docs/04-ArchitectureDecisionRecords/VectorStore/adr-004-capability-negotiation-and-rrf-pushdown.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **現状の課題 (Current Pain Points)**:
  1. **孤立した独自抽象の再発明**: `VK.Blocks.VectorStore` の独自抽象は、.NET 9+ 公式標準の `Microsoft.Extensions.VectorData.Abstractions` と乖離し、ベンダー公式コネクタ（Qdrant, Redis, Azure AI Search, PostgreSQL）を活用しにくい。
  2. **バックエンド責務の脳裂（Brain-Split）**: 専用ベクトルエンジン（Qdrant）とリレーショナルDBのベクトル列（pgvector, Azure SQL）のライフサイクルが未分離。リレーショナル側で `VectorStore` が動的テーブル作成を行うと、`VK.Blocks.Persistence.EFCore` と責務が衝突する。
  3. **クライアント側 RRF による I/O 浪費**: ハイブリッド検索において常にクライアント側で複数クエリを実行して自前マージしており、エンジン側のネイティブ RRF/ハイブリッド検索を活用できていない。
  4. **ColBERT / 疎ベクトルの表現力不足**: 単一の稠密ベクトルに限定されており、BM25/SPLADE 疎ベクトルや ColBERT 形式のトークン単位マルチベクトルが扱えない。
- **なぜ今やるのか (Why Now / Consumer)**:
  - RAG パイプライン、高精度セマンティック検索、マルチテナント AI サービスにおいて、PostgreSQL (pgvector) 主力構成および Qdrant 高速エンジンへの移行需要が急増しているため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - `Microsoft.Extensions.VectorData.Abstractions` を公式基盤として取り込み、VK 側は `VKResult<T>`、強制テナント分離（`VKTenantId`）、診断（`BB.04`）、耐障害性（`OR.03`）を付与するエンタープライズ・ガバナンス層に特化する。
  - バックエンドを 3 分類（`DedicatedEngine`, `RelationalColumn`, `Embedded`）し、リレーショナル DB においては「起動時検証＋LINQ 拡張」に徹し、スキーマ管理を EF Core へ委譲する。
  - RRF をエンジン非依存の I/O フリー純関数（`ARCH-03`）として公開し、Provider 能力に応じた「エンジン下推 (Pushdown) vs クライアント側 RRF 兜底 (Fallback)」の能力交渉メカニズムを確立する。
  - 多ベクトル（`VKMultiVector`）および疎ベクトル（`VKSparseVector`）モデルを新設し、ColBERT およびハイブリッド検索へ拡張可能にする。
- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - `HnswLite` の即時導入: 組み込み InMemory は当面ゼロ依存のコサイン類似度線形探索を維持する。
  - ColBERT の全量デフォルト永続化: トークン数倍率のストレージ膨張を避けるため、全自動有効化は行わず、Rerank パイプラインの選択的機能とする。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

- **Metaphor (概念メタファー / メンタルモデル)**:
  - **The Airport Baggage & Security Screening (空港手荷物検査と仕分け)**:
    - 旅客（クエリ/ドキュメント）は必ず厳格な保安検査場（VK ガバナンス・テナント検証）を通過する。
    - 荷物が専用コンテナ便（Dedicated Engine: Qdrant）ならコンテナごと丸ごと管理するが、旅客定期便（Relational: pgvector）なら既存の機体貨物室（EF Core テーブル列）に相乗りし、点検（Schema Validator）のみを行う。
    - 仕分けアルゴリズム（RRF）は完全に独立した計算理論（純関数）であり、自動仕分け機（ネイティブ下推）があれば任せ、なければ手作業仕分け（クライアント側計算）にフォールバックする。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| 保安検査・テナント防壁 | `VectorDataCollectionAdapter` | `VectorStore.VecEngine.Internal` | `VKGuard`、`VKTenantId` 注入、`VKResult` 例外マッピング |
| 機体貨物室の安全点検員 | `IVKVectorSchemaValidator` | `VectorStore.Common.Protocols` | 起動時に pgvector 拡張や HNSW/DiskANN 索引の存在と次元を検証 |
| 自動仕分け機（理論計算） | `VKReciprocalRankFusion` | `VectorSearch.Fusion.Contracts` | I/O フリーの純粋な数学的ランク融合計算 $\sum \frac{1}{k+\text{rank}}$ |
| 荷物便の仕様申告書 | `VKVectorStoreCapabilities` | `VectorStore.Common.Models` | ネイティブ RRF、ハイブリッド、多ベクトル対応可否の能力宣言 |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

```csharp
namespace VK.Blocks.VectorStore;

public enum VKVectorStoreBackendRole : byte
{
    DedicatedEngine = 0,
    RelationalColumn = 1,
    Embedded = 2
}

public sealed record VKVectorStoreCapabilities
{
    public required VKVectorStoreBackendRole BackendRole { get; init; }
    public bool SupportsNativeHybridSearch { get; init; }
    public bool SupportsNativeRrf { get; init; }
    public bool SupportsMultiVector { get; init; }
    public bool SupportsSparseVector { get; init; }
    public bool SupportsDiskIndex { get; init; }
    public bool SupportsFilterPushdown { get; init; }
}

public interface IVKVectorSchemaValidator
{
    Task<VKResult> ValidateSchemaAsync(string collectionOrTableName, int expectedDimensions, CancellationToken cancellationToken = default);
}

public interface IVKVectorCollectionLifecycle
{
    Task<VKResult<bool>> CollectionExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<VKResult> CreateCollectionAsync(string name, int dimensions, string distanceMetric = "Cosine", CancellationToken cancellationToken = default);
    Task<VKResult> DropCollectionAsync(string name, CancellationToken cancellationToken = default);
    Task<VKResult> CreateAliasAsync(string aliasName, string targetCollectionName, CancellationToken cancellationToken = default);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance / Allocation**:
  - `VKVector`、`VKSparseVector`、`VKMultiVector` は `ReadOnlyMemory<float>` を基礎とし、ヒープ上の配列複製を徹底的に抑止（`CS.04`）。
- **Resilience & Security**:
  - `VectorData` 接続先の障害はすべて Polly (`OR.03`) により遮断・リトライし、テナント境界 (`OR.02`) はアダプター境界で強制注入。

---

## 6. 📋 Decomposition & Implementation Plan

- [x] **Phase 1 (VectorStore Core Modernization)**: `Microsoft.Extensions.VectorData.Abstractions` 参照追加、3 バックエンドロール、能力モデル、スキーマ検証インターフェース、疎/多ベクトル定義、エラー定数（`ADR-003`）
- [x] **Phase 2 (VectorSearch RRF & Pushdown)**: 純関数 `VKReciprocalRankFusion` 抽出と能力交渉ハイブリッド検索ステージの実装（`ADR-004`）
- [x] **Phase 3 (PostgreSQL & Qdrant Packages)**: `VK.Blocks.VectorStore.Pgvector`（極薄検証）および `VK.Blocks.VectorStore.Qdrant`（フル機能）の構築
- [x] **Phase 4 (VectorIngest & Rerank Integration)**: 多ベクトル/疎ベクトルのインジェスト対応と ColBERT 選択的精排の整備

