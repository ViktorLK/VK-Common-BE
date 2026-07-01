# ADR 003: Asynchronous Strategy-Based Cognitive Memory Compression

- **Date**: 2026-06-29
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Engram

## 1. Context (背景)

以前の決定（ADR 002）により、対話履歴（Echo）がトークンバジェットを超えた場合に圧縮を行う仕組みが導入された。しかし、従来の設計では `CompressionStage` がパイプライン上で同期的に実行されていたため、以下の課題が生じていた：
1. **リクエストスレッドのブロッキング**: 圧縮（要約の生成など）には LLM の呼び出しが伴うため、ネットワークレイテンシと処理時間が非常に長く、クライアントへのレスポンスが著しく遅延する。
2. **圧縮アルゴリズムの固定化**: 要約（Summarize）以外の多様な圧縮アプローチ（構造化データ抽出、セグメンテーションなど）を柔軟に切り替える拡張性が不足していた。
3. **エラーハンドリングの脆弱性**: 圧縮処理中の例外（LLM プロバイダのタイムアウトやレートリミットなど）が、メインの会話パイプライン全体の実行を巻き込んで失敗させてしまう。

## 2. Problem Statement (問題定義)

ユーザーリクエストの応答性能（Latency）と可用性を損なうことなく、多様な圧縮ロジック（Strategy）を安全かつ堅牢にバックグラウンドで実行可能な、非同期型のメモリ圧縮アーキテクチャが必要であった。

## 3. Decision (決定事項)

AI.Engram において、従来の同期式圧縮ステージを解体し、**「非同期ジョブキュー及び戦略パターンに基づく認知記憶圧縮アーキテクチャ」**を導入する。

### 1. 非同期処理への移行 (Asynchronous Background Execution)
- **`CompressionJobQueue`**: バックグラウンドで実行すべき圧縮タスクを保持するスレッドセーフなインメモリジョブキュー。
- **`DefaultCompressionBackgroundService`**: `BackgroundService` を継承したホスト型サービス。パイプラインから発行された圧縮要求を非同期にデキューし、別スレッドで安全に実行する。これにより、クライアントへの応答は圧縮の完了を待たずに即時返却される。

### 2. 戦略パターンの導入 (Strategy-Driven Polymorphism)
- **`IVKCompressionService` / `IVKCompressionStrategy`**: 圧縮ロジックをインターフェースで抽象化。
- 多様な圧縮戦略を標準提供し、`VKCompressionOptions` の `StrategyType` に応じて動的に切り替える：
  - `LlmSummary`: 标准的な LLM 要約。
  - `HierarchicalSummary`: 階層的要約（段階的要約の統合）。
  - `KeyValueExtraction`: 重要なメタデータや Key-Value の抽出。
  - `TopicSegmentation`: トピックごとの分割と要約。
  - `Null`: 圧縮を行わないパススルー。

### 3. オプション設定の分離 (`VKCompressionOptions`)
- 圧縮の実行閾値、バジェット、使用する戦略タイプ (`VKCompressionStrategyType`) などをカスタマイズ可能にする。

```
[Client Request Pipeline]
   |
   +--> Trigger Compression? 
           |
          [YES] --> Enqueue to CompressionJobQueue
           |
   (Proceed Pipeline & Return Response immediately)

--------------------------------------------------
[DefaultCompressionBackgroundService] (Background)
   |
   +--> Dequeue Job --> Resolve Strategy --> Execute LLM Call & Update L2 Memory Store
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 外部メッセージキュー (RabbitMQ / Azure Service Bus) の利用
- **Approach**: 圧縮ジョブを外部の MQ にパブリッシュし、ワーカーで処理する。
- **Rejected Reason**: インフラ依存度が高くなり、BuildingBlock（共有ライブラリ）としてのポータビリティが低下する。まずはモジュール内完結のメモリ内キュー（Channel 等）を用いた実装が適切と判断した。

### Option 2: Polly を用いた同期的リトライとタイムアウト緩和
- **Approach**: 同期実行のまま、タイムアウト値を短く設定して失敗時はフォールバックする。
- **Rejected Reason**: LLM の呼び出しが成功したとしても数秒以上のブロッキングは避けられず、UX に対する根本的な解決にならないため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **応答速度の劇的改善**: 圧縮処理がバックグラウンドに逃がされるため、クライアントリクエストの遅延が解消される。
- **プラグイン構造の確立**: 新たな圧縮アプローチ（別ベンダーの API、ローカルモデルなど）を `IVKCompressionStrategy` を実装するだけで簡単に追加可能。
- **対障害性 (Resilience)**: バックグラウンドでの圧縮処理が失敗しても、クライアントのチャット処理自体には何の影響も与えない。

### Negative
- **結果整合性 (Eventual Consistency)**: 圧縮後のメモリ（L2）が反映されるまでにわずかな時間差（バックグラウンド処理の完了待ち）が発生する。

### Mitigation
- 直近の `TargetTurns` は生データのまま L1 に残されているため、最新のコンテキストは常に維持されており、実用上のコンテキスト断絶は発生しない。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- バックグラウンドワーカー内で発生したすべての例外は、メインプロセスをクラッシュさせないようにキャッチされ、`ILogger` を通じて構造化ログとして記録される。
- ジョブキューは `System.Threading.Channels` を使用して高スループットかつ非アロケーションで実装する。

## 7. Status
✅ Accepted
