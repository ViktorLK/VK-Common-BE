# ADR 002: Tiered Chat History Compression and Eviction

- **Date**: 2026-06-15
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Engram

## 1. Context (背景)

LLM（大規模言語モデル）のコンテキストウィンドウは有限であり、API 呼び出しのコストやレイテンシは送信する入力トークン数に直接比例する。長期にわたるエージェント会話（Echo / L1 メモリ）をすべてそのまま Prompt に引き継ぐと、以下の問題が発生する：
1. 急激なトークン消費とプロバイダ料金の増加。
2. 重要なコンテキスト（ペルソナ、直近の指示）が過去ログのノイズに埋もれ、LLM の応答精度が低下する（Lost in the Middle 現象）。
3. トークン上限オーバーによる接続遮断。

## 2. Problem Statement (問題定義)

過去の対話履歴をコンテキスト内で維持しつつ、システム負荷およびコストを制御するための、エグザム（Engram）ライフサイクル内での効率的かつ自動的な歴史圧縮（Summarization）および不要情報の安全な駆逐（Eviction）アーキテクチャが必要であった。

## 3. Decision (決定事項)

AI.Engram に **「多階層型の会話圧縮・駆逐機構 (Tiered Compression and Eviction)」**を導入する。

1. **二段階圧縮モデル（L1 & L2 Summary）**:
   - **L1 圧縮基準 (L1 Token Budget)**: 対話履歴（Echo データの合計トークン数）が設定された `L1TokenBudget`（デフォルト 2000）を超えた場合、自動的に圧縮をトリガーする。
   - **保護ターン設定 (Target Turns)**: 直近の `TargetTurns`（デフォルト 10）は文脈維持のために生データのまま保護し、それ以前の過去会話のみを圧縮対象（Summarize）にする。
   - **L2 圧縮上限 (L2 Max Summary Tokens)**: 合成された累積要約が大きくなりすぎた場合、再度全体の要約圧縮を行いサイズを一定に抑える。
2. **履歴の駆逐 (Eviction)**:
   - 圧縮対象となった古い `VKPromptFragment` (Echo) はアクティブな Fragment リストから除外され、呼び出しコンテキストの `EvictedFragments` へ移動する。これによりコンテキスト内の無駄なトークンを即時削減する。
3. **抽象化インターフェースによる疎結合設計**:
   - 実際の圧縮ロジックは `IVKCompressionStrategy` を通して実行され、初期実装として `NullCompressionStrategy` (透過) および `SummarizeCompressionStrategy` を用意する。
   - チャットセッションの情報は `IVKChatSessionStore` （インメモリ実装：`InMemoryChatSessionStore`）を介して管理され、インフラに依存しないテストが保証される。

```
[Echo History (L1)] ----> Exceeds L1 Budget? ----> YES ----> Summarize via Strategy (L2)
       |                                                               |
  Protect TargetTurns (10)                                       Inject as Knowledge Entry
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 単純なスライディングウィンドウによる古い対話の切捨て
- **Approach**: トークン上限が近づいたら、古いメッセージから単に削除していく。
- **Rejected Reason**: 長い文脈の中で、過去に決定したトピックやユーザーの発言意図が完全に喪失されるため、インテリジェントなエージェントの挙動としては不適であると判断した。

### Option 2: 毎回全ての歴史をベクトル DB に退避し、RAG で検索する
- **Approach**: 過去履歴をすべてベクトル DB にインデックスし、コサイン類似度で動的に引っ張る。
- **Rejected Reason**: 対話履歴は時間順（Chronological）の流れが重要であり、ベクトル類似度による断片的な検索（RAG）だけでは全体の歴史的文脈を追うのが困難である。要約と併用するのが望ましい。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **トークンとコストの最適化**: 過去履歴を圧縮してナレッジ（Knowledge）ステージにまとめ直すことで、Prompt 内の専有トークン数を劇的に削減できる。
- **プラグイン構造**: `IVKCompressionStrategy` にローカル LLM やファインチューン済みのサマライザ、あるいはプロバイダの API を任意にバインドできる。

### Negative
- **要約用 LLM 呼び出しコスト**: 圧縮フェーズ発生時に、別途サマリー作成のための LLM 呼び出しが 1 回非同期的に発生する。

### Mitigation
- 圧縮ステージはバジェット制限の計算に基づき、本当に必要なタイミングでのみトリガーされるよう、トグル設定および閾値チューニングを可能にする。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 圧縮によって生成された要約は、`RelativeDepth.AfterPersona` の深度にて `VKKnowledgeEntry` として再インジェクションされる。
- 個人情報（PII）などが含まれる場合、要約処理のプロンプト側でマスキングを行うなどのセキュリティ対策を考慮する。

## 7. Status
✅ Accepted
