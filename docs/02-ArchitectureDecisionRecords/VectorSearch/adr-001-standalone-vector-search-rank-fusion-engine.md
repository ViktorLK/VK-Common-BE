# ADR 001: Standalone Vector Search Rank Fusion Engine

- **Date**: 2026-06-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/VectorSearch

## 1. Context (背景)

検索システムにおいて、密ベクトル検索（Semantic Search）と疎ベクトル・全文テキスト検索（BM25 等）を組み合わせるハイブリッド検索は、検索の再現率（Recall）と適合率（Precision）を同時に高める業界標準のアプローチである。
しかし、これら異なる情報源から返される検索スコア（密ベクトルの距離値と、BM25 の頻度スコア）は、単位やスケールが全く異なるため、そのまま単純に合計することはできない。
これらを適切に融合（Fusion）し、最終的な並び順（Rank）を再計算する処理ロジックが各呼び出し元（RAG パイプライン等）で個別に実装されると、以下の問題が発生する：
1. **数式ロジックの重複**: 相互ランク融合（RRF）や加重スコア融合（Weighted Score Fusion）などの数学的な計算コードが分散・重複する。
2. **パフォーマンス効率の低下**: ソートやインデックス結合の処理で不要なメモリ割り当て（Allocation）が多発する。

## 2. Problem Statement (問題定義)

ハイブリッド検索やマルチインデックス検索において、異なるスケールのスコアやランク順位を一貫したアルゴリズムでマージし、高速かつコンポーザブルに結果を結合できる、共通のスコア融合・リランキングエンジンの構築が必要であった。

## 3. Decision (決定事項)

検索結果のスコア融合に特化した独立アセンブリ **「`VK.Blocks.VectorSearch`」**を新規追加し、RRF や Weighted などの融合エンジンを提供する。

### 1. 統一候補モデル (`VKFusionCandidate`)
- 各情報源のドキュメント識別子、元スコア、元順位などをカプセル化した軽量なモデル `VKFusionCandidate` を定義する。

### 2. 抽象インターフェース `IVKScoreFusion`
- スコア融合アルゴリズムを抽象化し、異なるフュージョン戦略をプラグイン可能にする。
  - `ReciprocalRankFusion` (相互ランク融合 - RRF): スコア値そのものではなく、順位（Rank）の逆数を用いて順位を再評価する。スケールの異なる結果の融合に極めて頑健。
  - `WeightedScoreFusion` (加重スコア融合): 各検索エンジンのスコアを重み付けパラメータ（Weight）に基づき結合する。

```
[Dense Vector Results]  --->  [IVKScoreFusion]
                                   |
[Sparse Text Results]   --->  (Executes RRF / Weighted)  ---> [Final Hybrid Ranked List]
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 各データベースプロバイダ側（Qdrant 等）のハイブリッド検索機能に依存する
- **Approach**: アプリケーション側でマージせず、Qdrant が持っている RRF/Hybrid Query API を直接叩く。
- **Rejected Reason**: システム全体で SQLite などのローカル DB とクラウド DB を混用している場合、SQLite 側で RRF がネイティブ実行できないため、DB 非依存の共通結合ランタイムが中間層（メモリ内）に必要となる。したがってアセンブリ側での実装を正とした。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **アルゴリズムの一貫性**: RAG パイプラインやエンタープライズ検索において、ハイブリッド検索の結果マージ結果の精度が完全に統一される。
- **高パフォーマンス**: スコア融合時のソート処理を最適化し、不必要なオブジェクト生成を抑制して GC 負荷を抑える。

### Negative
- **クライアント側でのパラメータ調整の難しさ**: RRF の定数（k=60等）や加重比率のチューニングを誤ると、類似度検索の精度が著しく悪化する可能性がある。

### Mitigation
- オプション（`VKVectorSearchOptions`）に業界推奨のデフォルト値（例：RRF 定数 `k = 60`）をあらかじめ設定し、無設定でも最適に動作するよう保証する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 融合処理においては、ドメインオブジェクト（実ドキュメントの本文等）のロードは行わず、ID とスコアのみでマージを完了させ、メモリフットプリントを最小限に抑える。

## 7. Status
✅ Accepted
