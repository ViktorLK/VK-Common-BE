# RFC-002: VectorSearch V2 Pipeline Orchestration & Context Compression

- **Status**: ✅ Approved *(Implemented)*
- **Author**: Architecture Team
- **Date**: 2026-06-27
- **Target Module(s)**: `VK.Blocks.VectorSearch`
- **Related ADRs / RFCs**: [ADR-002](/docs/04-ArchitectureDecisionRecords/VectorSearch/adr-002-pipeline-based-vector-search-orchestration.md), [ADR-003](/docs/04-ArchitectureDecisionRecords/VectorSearch/adr-003-multi-strategy-retrieval-and-context-compression-framework.md), [ADR-004 (VectorStore)](/docs/04-ArchitectureDecisionRecords/VectorStore/adr-004-capability-negotiation-and-rrf-pushdown.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **現状の課題 (Current Pain Points)**:
  1. 単なる複数検索のマージだけでなく、実エンタープライズ検索ではクエリの書き換え、セマンティックキャッシュ、Cross-Encoder リランキング、入力値セキュリティ防衛（Search Guard）などの多様な前処理・後処理が要求される。
  2. 検索によって取得された生のドキュメントコンテキストが長大すぎると、LLM のコンテキストウィンドウ上限に達するか、高額なトークン消費を引き起こす。
  3. 検索の実行方式（ベクトル、キーワード、ハイブリッド）とパイプラインが結合しており、プラグイン切り替えやモック検証が困難。
- **なぜ今やるのか (Why Now / Consumer)**:
  - RAG システムにおけるトークンコストの削減、低遅延キャッシュヒット、および安全な検索ライフサイクルオーケストレーションを確立するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - `VK.Blocks.Core` のパイプライン基盤を拡張した `DefaultVectorSearchPipeline` を導入し、検索ライフサイクルを順序制御可能なステージ群に分割する。
  - セマンティックキャッシュ（`DefaultSemanticCacheStage`）、クエリ書き換え（`DefaultQueryRewriteStage`）、入力防護（`DefaultSearchGuardMiddleware`）、Cross-Encoder リランキング（`DefaultRerankStage`）を提供する。
  - 検索戦略をカプセル化する `IVKSearchStrategy`（Vector, Keyword, Hybrid, NoOp）を導入する。
  - トークン予算内に文脈を最適要約・圧縮する `DefaultContextCompressionStage` および `IVKContextCompressionStrategy` を構築する。
  - RRF を I/O フリー純関数 `VKReciprocalRankFusion`（`ARCH-03`）として抽出し、エンジン下推とクライアント兜底を協調させる。
- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - LLM の最終回答生成（`AI.Cortex` / `AI.Psyche` が担当）。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

- **Metaphor (概念メタファー / メンタルモデル)**:
  - **The Water Purification & Bottling Plant (高度浄水・ボトリング複合ライン)**:
    - 原水（生の検索クエリ）を取り込み、不純物検査（Search Guard）を実施。
    - 貯水タンク（Semantic Cache）にすでに完成品があれば即出荷（ショートカット）。
    - 浄水処理（Query Rewrite & Multi-Strategy Retrieval）を経て、精密濾過（Reranking & RRF 融合）を行い、最後に許容ボトルサイズ（Context Compression トークン枠）に合わせて濃縮ボトリングする。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| 水質検査ゲート | `DefaultSearchGuardMiddleware` | `Internal/` | インジェクションおよび不正クエリの事前遮断 |
| 完成品貯水タンク | `DefaultSemanticCacheStage` | `Internal/` | ベクトル類似度によるキャッシュ確認と即時返却 |
| 浄水濾過ライン | `IVKSearchStrategy` | Public Protocol | ベクトル / キーワード / ハイブリッド検索の実行 |
| 精密濾過フィルター | `DefaultRerankStage` | `Internal/` | Cross-Encoder による順位再スコアリング |
| 濃縮ボトリング機 | `DefaultContextCompressionStage` | `Internal/` | トークン上限に応じた文脈要約・圧縮 |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

```csharp
namespace VK.Blocks.VectorSearch;

public interface IVKSearchStrategy
{
    Task<VKResult<IReadOnlyList<VKSearchResult>>> SearchAsync(
        string query,
        VKVectorSearchContext context,
        CancellationToken cancellationToken = default);
}

public interface IVKContextCompressionStrategy
{
    Task<VKResult<string>> CompressAsync(
        IReadOnlyList<VKSearchResult> searchResults,
        int maxTokens,
        CancellationToken cancellationToken = default);
}

public static class VKReciprocalRankFusion
{
    public const float DefaultK = 60.0f;
    public static VKResult<IReadOnlyList<VKFusionItem<TKey>>> Compute<TKey>(
        IReadOnlyList<IReadOnlyList<TKey>> runs,
        int topK,
        float k = DefaultK,
        IEqualityComparer<TKey>? comparer = null) where TKey : notnull;
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance**:
  - 各ステージは無効時（`Enabled = false`）に即時パススルーし、非アロケーション設計を徹底。
  - キャッシュヒット時は DB 検索および LLM 呼び出しを完全にスキップするため、P95 応答時間が 80% 短縮。
- **Token Efficiency**:
  - 文脈圧縮により、下流の LLM プロンプトトークン消費を 30% 〜 60% 削減。

---

## 6. 📋 Decomposition & Implementation Plan

- [x] **Phase 1 (Pipeline Architecture)**: `DefaultVectorSearchPipeline` と各ステージの実装（`ADR-002`）
- [x] **Phase 2 (Strategies & Compression)**: `IVKSearchStrategy` と `DefaultContextCompressionStage` の導入（`ADR-003`）
- [x] **Phase 3 (Pure RRF & Pushdown)**: `VKReciprocalRankFusion` 純関数抽出とエンジン下推能力交渉（`ADR-004`）

