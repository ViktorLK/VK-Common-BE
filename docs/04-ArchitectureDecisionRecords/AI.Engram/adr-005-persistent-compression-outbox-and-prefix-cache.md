# ADR 005: Persistent Tiered Compression Outbox, Session Serialization, and Prompt-Cache Stabilization

- **Date**: 2026-11-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Engram/Compression

## 1. Context (背景)

従来の非同期圧縮ジョブキュー（ADR 003）はインメモリの `BoundedChannel`（`DropWrite`）を用いていたため、以下の重大な課題があった：
1. ホスト再起動や複数インスタンスのスケーリング時に圧縮タスクが暗黙にロストする。
2. 同一セッションに対する並行リクエスト時に順不同な要約が生成され、文脈整合性が破壊される。
3. 毎ターン頻繁に要約が更新されると、LLM プロバイダの KV/Prompt Cache が無効化され、推論レイテンシと料金が悪化する。

## 2. Decision (決定事項)

非同期圧縮エンジンに **「永続化 Outbox ジョブストア、セッション単位シリアライゼーション、およびプロンプトキャッシュ安定化」**を導入する。

1. **`IVKCompressionJobStore` 抽象化**:
   - `(TenantId, SessionId, StartTurnIndex, EndTurnIndex)` で構成される決定論的なべき等性キー（`VKCompressionJobIdempotencyKey`）を導入。
   - インメモリ fallback 実装（`InMemoryCompressionJobStore`）と永続化 Outbox パターンを提供。
2. **セッション単位の排他制御**:
   - `IVKSessionCompressionLock` により、同一セッションの圧縮は常にターン番号順に単一スレッドで直列化する。
3. **プロンプトキャッシュ安定化と縮退フォールバック**:
   - 直近ターン（`RetainRecentTurns`）を生のまま保護し、未要約トークンが閾値（`SummaryTriggerTokenThreshold`）を超えた場合のみバッチ要約を実行してプロンプト先頭を固定化する。
   - 圧縮実行中に新規リクエストが到着しコンテキスト制限を超過する場合、L0 データを破壊することなく一時的なスライディング切り詰め（Degraded Truncation）を行う。

## 3. Status
✅ Accepted

