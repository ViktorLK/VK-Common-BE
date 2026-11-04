# RFC-001: Standalone Vector Search Rank Fusion Engine (RRF & Weighted)

- **Status**: ❌ Superseded *(Historic)*
- **Author**: Architecture Team
- **Date**: 2026-06-22
- **Target Module(s)**: `VK.Blocks.VectorSearch`
- **Related ADRs / RFCs**: [ADR-001](/docs/04-ArchitectureDecisionRecords/VectorSearch/adr-001-standalone-vector-search-rank-fusion-engine.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **現状の課題 (Current Pain Points)**:
  1. 密ベクトル検索（Semantic Search）と疎ベクトル・全文検索（BM25）を組み合わせるハイブリッド検索において、スコアのスケールや単位が異なるため、単純合算が不可能であった。
  2. 相互ランク融合（RRF: Reciprocal Rank Fusion）や加重スコア融合（Weighted Score Fusion）の計算ロジックが各呼び出し元でアドホックに重複実装され、メモリの不要な再割り当てが発生していた。
- **なぜ今やるのか (Why Now / Consumer)**:
  - 検索の再現率（Recall）と適合率（Precision）を両立させるハイブリッド検索のスコア融合ロジックを、独立した BuildingBlock として統一標準化するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - 検索結果のスコア融合・リランキングに特化した独立モジュール `VK.Blocks.VectorSearch` を導入する。
  - 統一候補モデル `VKFusionCandidate` を定義し、情報源ごとのドキュメント識別子、元スコア、元順位をカプセル化する。
  - スコア融合アルゴリズムを抽象化する `IVKScoreFusion` を策定し、`ReciprocalRankFusion`（RRF）および `WeightedScoreFusion`（加重スコア）を提供する。
- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - 物理的なデータベースへのクエリ発行（`VectorStore` が担当）。
  - 自然言語クエリのプロンプト書き換えやセマンティックキャッシュ（後の V2 パイプラインで導入）。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

- **Metaphor (概念メタファー / メンタルモデル)**:
  - **The Olympic Decathlon Scoring Table (十種競技の総合得点算出台)**:
    - 走る競技（秒数）と投擲競技（メートル）のように、単位もスケールも全く異なる複数の競技結果（ベクトル距離とキーワード出現頻度）。
    - これらを生の数字ではなく「順位点換算表（RRF 式: $1 / (k + \text{rank})$）」にかけることで、公平かつ頑健に総合優勝者（最上位ドキュメント）を決定する。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| 各種競技の記録用紙 | `VKFusionCandidate` | Public Model | 各検索系統の ID、スコア、順位の保持 |
| 総合得点算出ルール | `IVKScoreFusion` | Public Protocol | スコア融合戦略の抽象化 |
| 順位点換算機（RRF） | `ReciprocalRankFusion` | `Internal/` | スケール差に頑健な順位逆数加算ロジック |
| 加重得点換算機 | `WeightedScoreFusion` | `Internal/` | 比率重み付けによる線形結合スコア算出 |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

```csharp
namespace VK.Blocks.VectorSearch;

public sealed record VKFusionCandidate
{
    public required VKDocumentChunk Chunk { get; init; }
    public float Score { get; init; }
    public int Rank { get; init; }
}

public interface IVKScoreFusion
{
    VKResult<IReadOnlyList<VKSearchResult>> Fuse(
        IReadOnlyList<IReadOnlyList<VKFusionCandidate>> runs,
        int topK);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance**:
  - ドキュメント本文ではなく ID とスコアのみでマージを完了させ、メモリフットプリントと GC プレッシャーを抑制。
- **Tuning Risk**:
  - RRF の定数 $k$ の設定値（デフォルト $k=60$）により順位感度が変動するため、業界推奨デフォルト値を設定。

---

## 6. 📋 Decomposition & Implementation Plan

- [x] **Phase 1 (Models & Protocol)**: `VKFusionCandidate` と `IVKScoreFusion` の定義（`ADR-001`）
- [x] **Phase 2 (RRF & Weighted)**: `ReciprocalRankFusion` と `WeightedScoreFusion` の実装

