# ADR 003: Multi-Strategy Retrieval and Context Compression Framework

- **Date**: 2026-06-27
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/VectorSearch

## 1. Context (背景)

ベクトル検索エンジン（`VectorSearch`）は、ハイブリッド検索、密ベクトル検索、およびキーワード検索など、複数のデータソースと検索アルゴリズムを協調して実行する。
しかし、検索を実行する下位の技術仕様（SQL クエリ、HTTP クエリなど）と検索の意図が結合していると、ストレージ側の変更やチューニングのたびにオーケストレーターが影響を受ける。
また、検索によって取得されたドキュメント群（コンテキスト）は、生のままだと全体のトークンサイズが肥大化しやすく、LLM（大規模言語モデル）のコンテキスト上限に達してリクエストエラーを引き起こすか、あるいは不要なトークンコストを増加させる。

## 2. Problem Statement (問題定義)

1. **検索方法の強結合**: クエリ検索を実行するインターフェースにおいて、キーワード検索・ベクトル検索・ハイブリッド検索の呼び出し仕様が整理されておらず、動的な切り替えやモックが困難であった。
2. **取得テキストの肥大化**: 検索でヒットした生の文脈データを、トークンバジェットに応じて自動で要約・圧縮（Context Compression）してプロンプトに引き渡す最適化インフラが不足していた。

## 3. Decision (決定事項)

VK.Blocks.VectorSearch において、**「検索戦略の抽象化（`IVKSearchStrategy`）および文脈圧縮ステージ（Context Compression）の導入」**を決定する。

### 1. 多様な検索戦略の抽象化
- `IVKSearchStrategy` を定義し、具象検索プロトコルを完全に隠蔽する。
- 以下の標準戦略を提供する：
  - `DefaultVectorSearchStrategy`: ベクトルデータのみを用いた類似度検索。
  - `DefaultKeywordSearchStrategy`: キーワードインデックス（BM25等）を用いたキーワード検索。
  - `DefaultHybridSearchStrategy`: 双方を実行し、`IVKScoreFusion` を用いてランク融合するハイブリッド検索。
  - `NoOpSearchStrategy`: 検索を行わず空結果を返す。

### 2. 文脈圧縮ステージ（`DefaultContextExpansionStage` / `DefaultContextCompressionStage`）の導入
- 検索によって収集された `VKSearchResult` 一覧に対し、LLM や圧縮アルゴリズムを用いて情報密度を最大化しトークンを減らす `DefaultContextCompressionStage` を実装する。
- 圧縮ロジックは `IVKContextCompressionStrategy` （例：`DefaultContextCompressionStrategy`）に委譲し、設定された `VKContextCompressionOptions` の上限トークン数（`MaxCompressionTokens` 等）に収まるよう自動調整する。

```
[Search Query]
       |
       +--> Resolve IVKSearchStrategy (Hybrid / Vector / Keyword)
       +--> Get Raw Search Results
       +--> Enter: DefaultContextCompressionStage 
                   |
                   +--> Apply IVKContextCompressionStrategy
                   +--> Output: Compact, Token-optimized Context
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 圧縮処理を呼び出し元のアプリケーションサービスで行う
- **Approach**: 各 API コントローラやオーケストレータ側で、検索結果リストに対して手動で LLM 要約リクエストを投げる。
- **Rejected Reason**: 要約プロンプトの作成や、トークン制限計算などの定型コードが各所に散乱し、実装漏れによるトークンバースト（API エラー）を多発させるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **インフラの透過性**: 検索アルゴリズム（RRF 等）や DB 操作が完全に `IVKSearchStrategy` 配下にカプセル化され、呼び出し側はただ `SearchAsync` を呼ぶだけで最適な結果が得られる。
- **大幅なトークン節約**: 重複情報や無関係なノイズテキストが事前に圧縮されるため、LLM 呼び出し時の入力トークンコストが 30% 〜 60% 削減される。

### Negative
- **追加の遅延**: 前処理の圧縮ステージで追加の要約処理（LLM 等）が走る場合、検索全体の応答時間が長くなる。

### Mitigation
- 圧縮が必要な閾値（文字数やトークン数）を超えた場合のみ要約を走らせるか、軽量なルールベース（先頭切り出し、文末削除など）の超高速圧縮ストラテジを準備して選択できるようにする。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 圧縮処理中に個人情報や機密データ（PII）の流出を防ぐため、マスク処理や機密表現の置換を圧縮の前に適用するセキュリティ設計を推奨する。

## 7. Status
✅ Accepted
