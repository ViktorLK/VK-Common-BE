# ADR 002: Pipeline-Based Vector Search Orchestration

- **Date**: 2026-06-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/VectorSearch

## 1. Context (背景)

ハイブリッド検索（密ベクトル検索と BM25 全文検索など）を実行する際、単に複数の DB クエリを投げてマージするだけでは、実用的なエンタープライズ検索として不十分である。実運用では以下のような前処理・後処理・防衛処理が要求される：
1. **クエリの書き換え (Query Rewrite)**: ユーザーの自然な話し言葉や曖昧な入力クエリを、類似度検索に適した明確なキーワードや拡張ベクトルクエリに変換する。
2. **セマンティックキャッシュ (Semantic Cache)**: すでに類似のクエリに対して計算・返却された結果がある場合、それをインメモリ/ローカル DB から高速にキャッシュヒットさせ、LLM や重いベクトル類似度計算のコストを削減する。
3. **リランキング (Reranking)**: 一次検索（Vector Search 等）で取得した上位 N 件（例えば 50 件）のドキュメントと入力クエリの関係性を、高精度な Cross-Encoder（Reranker）を用いて再スコアリングし、最良の結果を絞り込む。
4. **検索安全性の確保 (Search Guard)**: 悪意のあるインジェクションプロンプトや、PII（個人情報）が含まれる検索クエリが実行されないよう、入力値のフィルタリングを行う。

これらをアドホックに実装すると、各機能の有効/無効の切り替えや順序制御が極めて困難になる。

## 2. Problem Statement (問題定義)

検索クエリのライフサイクル（サニタイズ -> キャッシュ確認 -> クエリ変換 -> 検索実行 -> 順位融合 -> リランキング -> キャッシュ書き込み）における多様な関心事を、モジュール化し、安全かつ順序制御可能な形で実行する検索パイプラインオーケストレーション基盤が必要であった。

## 3. Decision (決定事項)

`VK.Blocks.Core` の標準パイプラインフレームワーク（`VKPipelineExecutorBase`）をベースとし、**「パイプライン駆動のベクトル検索オーケストレーションエンジン」**を導入する。

### 1. `VKVectorSearchContext` を用いた状態共有
- 各検索リクエストの入力、書き換え後のクエリ、一次検索結果、最終リランキング結果、キャッシュ状態などをスレッドセーフに管理するコンテキストを導入する。

### 2. 独立した実行ステージ（Stage）とミドルウェアの定義
- **`DefaultSearchGuardMiddleware`**: 悪意ある入力をブロックする安全フィルタ。
- **`DefaultSemanticCacheStage`**: 検索実行前にキャッシュヒットを確認し、ヒット時はメインパイプラインをショートカットして即座に終了する。
- **`DefaultQueryRewriteStage`**: 類似度検索に適した形にクエリを変換する。
- **`DefaultRerankStage`**: 取得した候補に対して高精度な Cross-Encoder ソートをかける。
- **`SemanticCacheWriteStage`**: 新規検索結果をセマンティックキャッシュストアに非同期的に追加保存する。

```
[VectorSearch Executor]
   |
   +--> Search Guard Middleware (Validate Input)
   +--> Semantic Cache Stage (Check Cache) --> [Hit? Return Immediately]
   +--> Query Rewrite Stage (Rewrite Query Text)
   +--> Execute Underlying Vector Store & Fusion Engine (Get Candidates)
   +--> Reranking Stage (Cross-Encoder Re-sort)
   +--> Semantic Cache Write Stage (Save result to Cache)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: AI.Psyche の Weaving パイプラインに検索工程を埋め込む
- **Approach**: 提示文構築の一部として検索を行い、すべての前処理・後処理も Psyche の中で管理する。
- **Rejected Reason**: 検索（Search）は独立して使用されるケース（例：API での単純なナレッジ検索）もあり、プロンプト構築（Psyche）に密結合させてしまうと、単体での検索 API の記述やキャッシュ制御が極めて非効率になるため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **高いコンポーザビリティ**: `VKPipelineOptions` やオプションクラスを書き換えるだけで、リランカーの有無やクエリ書き換えの有無を完全にプラグイン制御可能。
- **効率的なキャッシュライフサイクル**: キャッシュの読み込みと書き込みがパイプラインのフックとして自動化され、呼び出し元の実装漏れが発生しない。

### Negative
- **実行ステップの増加**: 複数のステージとインターセプタを通るため、インメモリでのデリゲート呼び出しのオーバーヘッドがわずかに発生する。

### Mitigation
- パイプライン内の処理はほぼ非アロケーションで設計し、各ステージが無効（`Enabled = false`）の場合は即時パススルーされるため、実質的な遅延は無視できる。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- キャッシュのキー生成には、入力クエリの埋め込みベクトル同士の類似度距離（閾値：`ScoreThreshold`）を用い、完全一致でなくても高精度なセマンティックヒットを実現する。
- `SearchGuard` は、インジェクションが疑われる閾値を超えた場合に `VKVectorSearchPipelineErrors.GuardBlock` を返し、検索の実行を阻止する。

## 7. Status
✅ Accepted
