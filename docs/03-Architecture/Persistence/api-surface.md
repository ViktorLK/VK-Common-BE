# 📜 Public API Surface & Compatibility Policy: VK.Blocks.Persistence

- **System ID / Package ID**: `VK.Blocks.Persistence`
- **Domain / Area**: `Persistence`
- **Target Layer**: L1 Foundation / L2 Domain BuildingBlock / L3 Infrastructure Adapters
- **Status**: 🏛️ Accepted
- **Last Verified**: 2026-10-30
- **Governing Rules**: `ARCH-07`, `AP.03`, `FS-09`, `BASE-01`, `CS.01`

---

## 1. 🎯 目的とスコープ (Purpose & Scope)

本文書は、`VK.Blocks.Persistence` サブシステム（`Persistence`, `Persistence.EFCore`, `Persistence.EFCore.Cosmos`, `Persistence.EFCore.Sqlite`）が下流マイクロサービス、ドメイン層、および AI コーディングエージェントに対してコミットする **Level 1 公開契約（Public API Surface）** の境界線と、**SemVer 互換性・非推奨化ガバナンス** を明文化する SSOT（単一の真実の源）です。

> [!IMPORTANT]
> `ARCH-07` に基づき、明示的に Level 1 公開契約として承認された型・メンバ以外はすべて `internal` でカプセル化されます。AI エージェントおよび開発者は、本ドキュメントに記載のない内部型（`Internal/` 配下のクラスやキャッシュ）を下流アセンブリから直接参照したり、不用意に `public` 化したりすることを固く禁じます。

---

## 2. 🏛️ Level 1 公開契約型一覧 (Public Exports Baseline — `AP.03`, `ARCH-07`)

### 2.1 コア永続化ポート (`VK.Blocks.Persistence`)

| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `IVKUnitOfWork` | `interface` | トランザクション・コミット境界を一元管理する中核ポート（`SaveChangesAsync`）。 |
| `IVKUnitOfWork<TContext>` | `interface` | 特定のコンテキスト型にバインドされた型安全な Unit of Work。 |
| `IVKTransaction` | `interface` | 明示的トランザクション（Commit / Rollback）制御ポート。 |
| `IVKEntityReadRepository<T>` | `interface` | 読み取り専用リポジトリ（No-Tracking、仕様パターン、カーソルページネーション）。 |
| `IVKEntityWriteRepository<T>` | `interface` | 変更追跡・更新専用リポジトリ（Add, Update, Delete）。 |
| `IVKEntityBulkRepository<T>` | `interface` | 高スループット一括バッチ処理専用リポジトリ（BulkInsert, BulkUpdate, BulkDelete）。 |
| `IVKEntitySystemRepository<T>` | `interface` | 特権システムワーカー専用リポジトリ（テナント・論理削除フィルターバイパス）。 |
| `IVKEntityRepository<T>` | `interface` | Read と Write を合成した標準集約ルート向け CRUD リポジトリ。 |
| `IVKAuditProvider` | `interface` | 監査情報（実行ユーザー・日時）の取得ポート。 |
| `IVKOutboxStore` | `interface` | Transactional Outbox の未処理メッセージ取得・状態更新ポート。 |
| `VKPersistenceErrors` | `static class` | 永続化層の標準エラーコード定数群（`CS.01`）。 |
| `VKPersistenceOptions` | `sealed partial record` | 永続化共通オプション。 |

### 2.2 EF Core アダプター (`VK.Blocks.Persistence.EFCore`)

| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKPersistenceEFCoreBlock` | `sealed partial class` | ブロックマーカー型（Source Generator 連携）。 |
| `VKPersistenceEFCoreOptions` | `sealed partial record` | EF Core 共通構成オプション（接続文字列、タイムアウト、スキーマ設定）。 |
| `VKBaseDbContext` | `abstract class` | 監査・Outbox・Contributor パイプラインを内包する基底 DbContext。 |
| `IVKModelCreatingContributor` | `interface` | モデル生成（エンティティ構成）拡張ポート（`ADR-013`）。 |
| `IVKModelConventionContributor` | `interface` | データ型規約拡張ポート（`ADR-013`）。 |
| `IVKGlobalFilterContributor` | `interface` | 動的グローバルクエリフィルター注入ポート（`ADR-013`）。 |
| `ICursorSerializer` | `interface` | カーソル文字列のシリアライズ・暗号署名検証ポート（`ADR-009`）。 |
| `VKCursorPagedResult<T>` | `sealed record` | 定数時間カーソルページネーション結果モデル（`FS-08`）。 |

### 2.3 物理プロバイダ拡張 (`Cosmos`, `Sqlite`)

| モジュール | 公開シンボル (Symbol) | 責務 |
| :--- | :--- | :--- |
| **`Persistence.EFCore.Cosmos`** | `VKPersistenceEFCoreCosmosBlock` | Cosmos DB ブロックマーカー。 |
| | `VKPersistenceEFCoreCosmosOptions` | Cosmos DB 接続・スループット・整合性オプション。 |
| | `IVKCosmosSessionTokenAccessor` | 分散ノード越境の Session Token 伝播ポート（`Cosmos ADR-002`）。 |
| | `IVKCosmosTransactionalBatchFactory` | 同一パーティション内 ACID バッチ生成ポート（`Cosmos ADR-003`）。 |
| **`Persistence.EFCore.Sqlite`** | `VKPersistenceEFCoreSqliteBlock` | SQLite ブロックマーカー。 |
| | `VKPersistenceEFCoreSqliteOptions` | SQLite 接続・ファイルパス・プーリングオプション。 |

---

## 3. 🔒 内部実装のカプセル化 (Internal Boundaries)

以下のコンポーネントは **`internal sealed`** としてカプセル化されており、下流マイクロサービスからの直接参照は禁止されています：

1. **メタデータキャッシュ**: `EfCoreTypeCache<T>`, `EfCoreMethodInfoCache`, `EfCoreExpressionCache`
2. **インターセプター具象**: `AuditingInterceptor`, `VKDomainEventsInterceptor`, `SoftDeleteInterceptor`
3. **プロバイダ内部マネージャー**: `SessionTokenManager`, `SqliteModelConventionContributor`
4. **内部実装リポジトリ**: `CosmosBaseRepository<T>`, `VKEFCoreSystemRepository<T>`

---

## 4. ⚖️ SemVer 運用と破壊的変更 (Breaking Changes) 基準 (`FS-09`)

### 4.1 破壊的変更（Major バージョン昇格必須・要 ADR）
- `IVKUnitOfWork` や `IVKEntityReadRepository<T>` などの公開ポートのメソッドシグネチャ変更・削除。
- `VKPersistenceErrors` に定義された公開エラーコード文字列の変更（`FS-02`）。
- `VKCursorPagedResult<T>` のワイヤ契約プロパティの削除・名前変更。
- 物理列順序（`ADR-015`）のゾーン規則の変更。

### 4.2 後方互換変更（Minor / Patch）
- 新しい Contributor（例: 新規インデックス規約）のオプトイン追加。
- 既存リポジトリへのオプショナル引数（`VKRequestOverride` / `CancellationToken`）の追加。
- 新規物理データベースアダプター（例: PostgreSQL 専用アダプター）の独立パッケージ追加。

---

## 5. 🔄 非推奨化（Deprecation）プロセス
1. 非推奨メソッド・プロパティに `[Obsolete("Use NewApi instead. Scheduled for removal in v{NextMajor}.", false)]` を付与。
2. 移行理由および代替手段を ADR に記録し、警告メッセージ内に ADR 参照リンクを含める。
3. 少なくとも 1 つの Minor バージョンサイクルで猶予期間を設けた後、次の Major バージョンで物理削除。

