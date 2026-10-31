# ADR 014: Transactional Outbox and Domain Events Interception Architecture

- **Date**: 2026-08-25
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence.EFCore

---

## 1. Context (背景)

分散マイクロサービスアーキテクチャにおいて、ドメインエンティティの状態変更（DB 保存）と、関連するドメインイベント（メッセージブローカーへのメッセージ発行）の間には、典型的な **Dual-Write（二重書き込み）ハザード** が存在する。

1. **DB コミット成功、メッセージ送信失敗**: メッセージが消失し、下流マイクロサービスが不整合な状態に取り残される。
2. **メッセージ送信成功、DB コミット失敗**: 実在しないエンティティのイベントが外部に配信される（ゴーストメッセージ）。
3. **2相コミット (2PC / XA) の採用不可**: パフォーマンス劣化が極めて激しく、RabbitMQ / Kafka / Azure Service Bus と RDBMS 間の分散トランザクションはクラウドネイティブ環境では推奨されない。

## 2. Problem Statement (問題定義)

アプリケーション開発者が特別な分散トランザクション機構を意識することなく、通常の `SaveChangesAsync()` を呼び出すだけで、エンティティの更新とドメインイベントの永続化が**同一のローカルデータベーストランザクション内で確実にアトミックにコミットされる**仕組みが必要であった。

## 3. Decision (決定事項)

EF Core の `SaveChangesInterceptor` を活用した **Transactional Outbox パターン** を採用する。

### 1. `VKDomainEventsInterceptor` による自動傍受 (`ARCH-12`)
- `SaveChangesInterceptor` を継承した `VKDomainEventsInterceptor` を実装。
- `SavingChangesAsync` のタイミングで、`ChangeTracker` 内のすべてのエンティティから未発行のドメインイベント（`IVKHasDomainEvents`）を収集。
- 収集したイベントを JSON 文字列へシリアライズし、`VKOutboxMessage` エンティティとして同一 `DbContext` の `OutboxMessages` テーブルに追加。
- コミットが成功した後にエンティティ内のイベントコレクションをクリアする。

```
[ Domain Handler ] 
       | 
       v  (Modifies Entity + Raises DomainEvent)
[ DbContext.SaveChangesAsync() ]
       |
       +--> (Intercepted by VKDomainEventsInterceptor)
       |         |
       |         +--> Extracts Domain Events from ChangeTracker
       |         +--> Serializes to VKOutboxMessage
       |         +--> Adds to Outbox DbSet in SAME DbTransaction
       |
       +--> [ ATOMIC COMMIT TO DATABASE ]
```

### 2. `VKEFCoreOutboxStore` による取得と状態更新
- `Persistence` コアの `IVKOutboxStore` インターフェースを実装する `VKEFCoreOutboxStore` を提供。
- バックグラウンドワーカー（`BackgroundJobs`）が、未処理（`Pending`）の `VKOutboxMessage` をポーリング・取得し、安全に外部ブローカーへ発行した後に `Published` / `Failed` 状態へ更新する。

### 3. Outbox テーブルの自動構成 (`OutboxModelContributor`)
- `IVKModelCreatingContributor`（`ADR-013`）を実装した `OutboxModelContributor` により、`VKOutboxMessage` のテーブルスキーマ、状態インデックス（`Status` + `CreatedAt`）、およびロック制御用カラムを自動構成する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: ハンドラー内で直接メッセージブローカーに送信
- **Approach**: `await _repository.UpdateAsync(order); await _publisher.PublishAsync(new OrderCreatedEvent());`
- **Rejected Reason**: 通信瞬断やプロセス強制終了時の二重書き込み不整合を防ぐことが原理的に不可能なため却下。

### Option 2: データベースログの CDC (Change Data Capture / Debezium)
- **Approach**: DB の WAL（Write-Ahead Log）を監視し、イベントを抽出する。
- **Rejected Reason**: 外部ミドルウェア（Kafka Connect / Debezium）の運用インフラが必須となり、小〜中規模サービスでのフットプリントが過大となるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **厳密な At-Least-Once 保証**: DB への書き込みとイベント永続化が 100% 同一トランザクションで完結し、イベント消失がゼロになる。
- **透過的な開発者体験**: アプリケーション開発者は `entity.AddDomainEvent(...)` を呼んで保存するだけでよく、Outbox の存在を意識する必要がない。

### Negative
- 同一トランザクションへのレコード追加による、わずかな DB 書き込みオーバーヘッド。
- Outbox テーブルの増大に伴うストレージ管理。

### Mitigation
- 成功済みレコード（`Published`）を定期的にクリーンアップまたは別テーブルへ退避するアーカイブジョブを `BackgroundJobs` で提供する。

## 6. Implementation & Security
- **Security**: Outbox メッセージに含まれる PII（個人識別情報）は、暗号化またはマスクして格納可能（`FS-12`）。
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence.EFCore/Interceptors/VKDomainEventsInterceptor.cs`

