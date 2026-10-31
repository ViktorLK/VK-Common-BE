# ADR 005: Asynchronous Corpus Ingestion Service and Job Status Engine

- **Date**: 2026-06-27
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Corpus

## 1. Context (背景)

ナレッジ（コーパス）をライフサイクルストアにインポートする処理（`DefaultRecallKnowledgeLifecycleStore` 等での読み込み・保存）には、ファイルのダウンロードやパース、チャンキング等の時間のかかる I/O 処理が伴う。
これらがメインのクライアントリクエスト（Web サーバー等）の実行スレッド上で同期的に呼び出されると、以下の課題が発生する：
1. **リクエストのタイムアウト**: インポートするファイルの容量が大きい場合、ネットワーク通信制限やスレッドの応答限界を超え、要求元の接続が遮断（Timeout）される。
2. **進行状況の追跡不可**: バックグラウンドでの進捗（処理中、完了、失敗、エラー原因など）を、実行中の非同期タスクに対して外部からポーリングや状態監視する手段が存在しなかった。

## 2. Problem Statement (問題定義)

システム全体の可用性とユーザー体験を損なうことなく、時間のかかる大量のナレッジインポート処理をバックグラウンドに逃がし、その進捗状況を外部から安全にクエリ・管理できるインジェスション管理エンジンが必要であった。

## 3. Decision (決定事項)

AI.Corpus において、**「非同期コーパスインジェストサービスとインメモリジョブ状態管理ストアの導入」**を決定する。

### 1. 抽象インジェストサービス (`IVKCorpusIngestingService`)
- インポートジョブの投入およびステータス管理を行うための `IVKCorpusIngestingService` を定義する。
- 具象実装として `DefaultCorpusIngestingService` を提供し、受け取ったインポート要求を非同期スレッド（バックグラウンド）に安全にスケジューリングする。

### 2. 進捗状態管理抽象 (`IVKIngestingStatusStore`)
- ジョブのライフサイクル状態を永続化・クエリするための `IVKIngestingStatusStore` を導入。
- 開発時および単一ホスト用に、スレッドセーフな `InMemoryIngestingStatusStore` を標準同梱する。
- 管理するステータスモデルとして以下を定義：
  - `VKIngestingJobStatus`: 進捗（`Pending`, `Processing`, `Completed`, `Failed`）やエラー文字列。
  - `VKIngestingStatus`: 総チャンク数、処理済みチャンク数などの進行統計データ。

```
[Import request]
       |
       +--> IVKCorpusIngestingService.StartIngestingAsync(jobId)
                   |
                   +--> Write status: IVKIngestingStatusStore (State = Processing)
                   +--> Fire & Forget: Background Worker Task (Runs parser/chunker/indexing)
                   +--> Return success (JobId) immediately to Client

-------------------------------------------------------------------
[Background Worker Task] (Async)
       |
       +--> Processing... Update IVKIngestingStatusStore (Processed count)
       +--> Success/Fail -> Update state (Completed / Failed)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Hangfire 等の本格的な分散ジョブキューを前提とする
- **Approach**: インジェストジョブを Hangfire を用いて分散キューイングする。
- **Rejected Reason**: `AI.Corpus` はライブラリ（Building Block）であり、外部の特定データベース（Hangfire 用 SQL Server等）を強要すると、モジュールのポータビリティが著しく損なわれる。そのため、メモリ内のスレッドプールまたはチャネルで動作する軽量エンジンをデフォルトとし、オプションで差し替え可能に設計した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **API レスポンスの高速化**: 重いインポート処理が即時にバックグラウンドへ投げられるため、呼出元はミリ秒単位で応答を受け取れる。
- **進捗追跡のサポート**: フロントエンドやクライアントが JobId を使って進捗率（何％処理完了）をポーリングし、進捗バーを UI に表示できるようになった。

### Negative
- **プロセスクラッシュ時のジョブの消失**: デフォルトのインメモリストア（`InMemoryIngestingStatusStore`）を使用している場合、アプリケーションサーバーがクラッシュすると実行中の進捗やジョブ情報が完全に消失する。

### Mitigation
- 本番運用の場合は、`IVKIngestingStatusStore` の DB 永続化プロバイダ（Redis、SQL Server 等）を別途 DI コンテナ側で `Replace` してバインドできるように設計し、状態の揮発を防ぐ。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- バックグラウンドタスクが多発してスレッドプールやメモリが枯渇するのを防ぐため、同時にアクティブとなるインジェストジョブ数を制限（またはキューイング）するガードロジックを実装する。

## 7. Status
✅ Accepted
