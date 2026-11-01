# RFC-003: V3 Pluggable Contributors, Distributed Outbox & Multi-Provider Isolation

- **Status**: ✅ Approved (Implemented Evolution Wave 3)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-09-16
- **Target Module(s)**: `VK.Blocks.Persistence.EFCore`, `VK.Blocks.Persistence.EFCore.Cosmos`, `VK.Blocks.Persistence.EFCore.Sqlite`
- **Related ADRs**:
  - [EFCore ADR-013: Pluggable Model Configuration and Query Filter Contributor Pipeline](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-013-model-and-filter-contributor-pipeline.md)
  - [EFCore ADR-014: Transactional Outbox and Domain Events Interception Architecture](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-014-transactional-outbox-and-domain-events-interception.md)
  - [EFCore ADR-015: Deterministic Physical Column Ordering Convention](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-015-deterministic-column-ordering-convention.md)
  - [Cosmos ADR-001: Independent Cosmos Db Persistence Block](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore.Cosmos/adr-001-independent-cosmos-db-persistence-block.md)
  - [Cosmos ADR-002: Session Token Propagation for Distributed Read-Your-Writes Consistency](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore.Cosmos/adr-002-session-token-propagation-for-read-your-writes.md)
  - [Cosmos ADR-003: Atomic In-Partition Batching via Cosmos TransactionalBatch](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore.Cosmos/adr-003-in-partition-acid-batching-via-transactional-batch.md)
  - [Sqlite ADR-001: Independent Sqlite Persistence Block](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore.Sqlite/adr-001-independent-sqlite-persistence-block.md)
  - [Sqlite ADR-002: Standardized DateTimeOffset Storage via Model Convention (Abandoning EF1001 Internal APIs)](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore.Sqlite/adr-002-standardized-datetimeoffset-storage-via-model-convention.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第3波着手前の課題 (Wave 3 Pain Points)**:
  - **DbContext の一枚岩肥大化と OCP 違反 (`EFCore ADR-013`)**:
    `OnModelCreating` 内にマルチテナント、論理削除、排他制御トークン、列順序などのあらゆる関心事が集中的にハードコードされ、新しい横断的ブロック（`MultiTenancy` 等）のプラグイン登録が不可能であった。
  - **分散メッセージの Dual-Write 消失ハザード (`EFCore ADR-014`)**:
    エンティティ更新とメッセージブローカーへのイベント送信の間で通信瞬断やプロセス異常終了が発生した場合、データとイベントの不整合（片肺状態）を防止できなかった。
  - **物理スキーマレイアウトの不統一 (`EFCore ADR-015`)**:
    テーブルごとに列順序（Column Order）がばらつき、主キーや監査フィールドの物理的配置が不統一で運用性が低下していた。
  - **物理プロバイダのバイナリ汚染 (`Cosmos ADR-001`, `Sqlite ADR-001`)**:
    Cosmos DB や SQLite を使わないマイクロサービスに対しても不要な巨大 NuGet パッケージが強制取り込みされていた。
  - **クラウド分散環境での整合性問題 (`Cosmos ADR-002`, `ADR-003`)**:
    ステートレスな多ノード環境（K8s）で Cosmos DB の Session Consistency を維持できず、跨リクエストでの古いデータ読み取り（Stale Reads）が発生していた。また、同一パーティション内での複数ドキュメント更新のアトミック性が欠如していた。
  - **非公式内部 API への依存リスク (`Sqlite ADR-002`)**:
    SQLite の `DateTimeOffset` ソートのために使用していた EF Core 非公開の式ツリー変換器（`EF1001`）がバージョンアップの破壊的リスクを抱えていた。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - 全社基盤がマルチクラウド（Azure Cosmos DB、PostgreSQL、エッジ SQLite）およびイベント駆動型分散マイクロサービスへ本格移行するにあたり、高度なインフラ隔離と決定論的信頼性を両立させるため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **Contributor パイプラインによる完全な OCP 実現 (`EFCore ADR-013`)**:
    `IVKModelCreatingContributor`、`IVKModelConventionContributor`、`IVKGlobalFilterContributor` を通じて、外部ブロックから透過的にモデル定義を合成可能にする。
  - **Transactional Outbox パターン統合 (`EFCore ADR-014`)**:
    EF Core の `SavingChangesAsync` をインターセプトし、ドメインイベントとエンティティ状態変更を同一 DB トランザクション内でアトミック永続化（At-Least-Once 保証）。
  - **決定論的物理列順序規約 (`EFCore ADR-015`)**:
    `ColumnOrderingModelContributor` により、全テーブルで `Id` → `TenantId` → 業務プロパティ → 監査 → 削除 → `RowVersion` の整然たる配置を自動強制。
  - **データベースプロバイダの完全パッケージ分離 (`Cosmos ADR-001`, `Sqlite ADR-001`)**:
    Cosmos DB と SQLite を独立した Building Block に分離し、不要な依存関係をゼロにする。
  - **Cosmos DB 分散整合性とバッチアトミック性 (`Cosmos ADR-002`, `ADR-003`)**:
    `IVKCosmosSessionTokenAccessor` による HTTP 越境セッショントークン伝播と、`IVKCosmosTransactionalBatchFactory` による単一ネットワーク往復での ACID バッチ実行。
  - **SQLite 型規約の公式標準化 (`Sqlite ADR-002`)**:
    内部 API（`EF1001`）を全廃し、`DateTimeOffsetToStringConverter` による ISO 8601 文字列変換規約で安定稼働。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **Cosmos DB 跨パーティション分散トランザクション**: Cosmos DB の物理仕様上、跨パーティションは Saga パターン（`VK.Blocks.Saga`）で対処し、本ブロックでは同一パーティションキー内のアトミック性に集中する。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **近代工業団地の標準モジュラー配管網と確実配送パイプライン (The Industrial Modular Conduit & Guaranteed Delivery Pipeline)**
>
> 従来の工場（DbContext）は、内部の壁に直接ガス管、配電線、水道管（テナント、論理削除、監査）がコンクリートで塗り込められており、新しい配管を追加するには工場全体を取り壊す必要がありました。
> 第 3 波の改革により、工場内に「標準配管ラック（Contributor パイプライン）」を敷設し、各配管業者（各モジュール）が外部から規格コネクタをカチッと差し込むだけで、工場の操業を止めずに自由に設備を拡張できるようにしました。
> また、製品の出荷（DB 保存）と発送伝票の発行（イベント送信）を同一の密閉金庫（Transactional Outbox）内で同時に施錠することで、荷物だけ届いて伝票が届かない事故を根絶。
> さらに、遠隔地の支社（Cosmos DB）とは暗号化された無線認証タグ（Session Token）で交信し、現場の小さな簡易倉庫（SQLite）では危険な裏技（内部非公開 API）を捨てて頑丈な標準ラック（Model Convention）に切り替えました。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **Modular Pipe Rack (標準配管ラック)** | `IVKModelCreatingContributor`, `IVKGlobalFilterContributor` | Public Protocol (`Persistence.EFCore`) | DbContext を変更せずにモデル構成やフィルターを注入する OCP パイプライン |
| **Secure Lockbox (密閉金庫)** | `VKDomainEventsInterceptor`, `VKEFCoreOutboxStore` | `Internal/` (`Persistence.EFCore`) | SaveChanges 時にイベントを原子的に Outbox テーブルへ格納する二重書き込み防止機構 |
| **Standard Blueprint (標準設計図)** | `ColumnOrderingModelContributor` | `Internal/` (`Persistence.EFCore`) | 物理列順序を論理ゾーン順に整列させる決定論的レイアウト規約 |
| **Radio Auth Tag (無線認証タグ)** | `IVKCosmosSessionTokenAccessor`, `SessionTokenManager` | Dedicated Block (`Cosmos`) | HTTP 境界を越えて Session Token を引き継ぐ「読己所写」整合性保証 |
| **Atomic Container (アトミック密閉箱)** | `IVKCosmosTransactionalBatchFactory` | Dedicated Block (`Cosmos`) | 同一パーティション内の複数操作を単一往復で完結させる ACID バッチ |
| **Certified Standard Valve (公式認定バルブ)** | `SqliteModelConventionContributor` | Dedicated Block (`Sqlite`) | 非公開 API（EF1001）を排した公式 Model Convention による ISO 8601 日時管理 |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 プラガブル Contributor 契約 (`Persistence.EFCore`)

```csharp
// [EFCore ADR-013] モデル構築拡張パイプライン
public interface IVKModelCreatingContributor
{
    void Configure(ModelBuilder modelBuilder);
}

public interface IVKGlobalFilterContributor
{
    void ApplyFilter(ModelBuilder modelBuilder, Type entityType);
}
```

### 4.2 Transactional Outbox インターセプション

```csharp
// [EFCore ADR-014] SaveChanges での自動イベント収集とアトミックコミット
internal sealed class VKDomainEventsInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // 1. ChangeTracker からドメインイベントを収集
        // 2. VKOutboxMessage を生成し同一 DbContext の Outbox テーブルに追加
        // 3. コミット成功後にエンティティ側のイベントをクリア
    }
}
```

### 4.3 Cosmos DB 分散整合性とバッチファクトリ

```csharp
// [Cosmos ADR-002] セッショントークン伝播
public interface IVKCosmosSessionTokenAccessor
{
    string? GetSessionToken();
    void SetSessionToken(string token);
}

// [Cosmos ADR-003] 同一パーティションの ACID バッチ
public interface IVKCosmosTransactionalBatchFactory
{
    TransactionalBatch CreateBatch(string partitionKeyValue);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Outbox テーブルの肥大化リスク**:
  - 大量イベント発行に伴い、Outbox テーブルの容量が増大する。
  - *Mitigation*: 処理済み（`Published`）メッセージを定期削除するクリーンアップジョブを `BackgroundJobs` で自動稼働。
- **Cosmos DB TransactionalBatch のサイズ制限**:
  - Cosmos DB の物理仕様上、1 バッチあたりの最大ペイロードは 2MB、最大操作数は 100 件に制限される。
  - *Mitigation*: バッチファクトリ内で件数制限（Chunking）を検知し、制限超過時は Result.Failure で早期通知（`CS.01`）。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

- [x] **Phase 1 (Contributor Pipeline & Outbox)**: `IVKModelCreatingContributor`、`VKDomainEventsInterceptor`、`ColumnOrderingModelContributor` の実装。
- [x] **Phase 2 (Cosmos DB Resilience & Batching)**: `SessionTokenManager` の HTTP 伝播バインディングと `IVKCosmosTransactionalBatchFactory` の統合。
- [x] **Phase 3 (SQLite Hardening)**: `EF1001` 式木変換器の完全削除と `SqliteModelConventionContributor` への移行。

