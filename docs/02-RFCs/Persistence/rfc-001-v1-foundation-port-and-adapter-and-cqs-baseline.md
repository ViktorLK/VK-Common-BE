# RFC-001: V1 Foundation, Port-and-Adapter & CQS Baseline

- **Status**: ✅ Approved (Implemented Evolution Wave 1)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-02-14
- **Target Module(s)**: `VK.Blocks.Persistence`, `VK.Blocks.Persistence.EFCore`
- **Related ADRs**:
  - [Persistence ADR-001: ORM-Agnostic Persistence Abstractions and Clean Domain Decoupling](/docs/04-ArchitectureDecisionRecords/Persistence/adr-001-orm-agnostic-persistence-abstractions.md)
  - [Persistence ADR-002: Role-Segregated Repository Protocols (ISP & CQS Separation)](/docs/04-ArchitectureDecisionRecords/Persistence/adr-002-role-segregated-repository-protocols.md)
  - [EFCore ADR-001: Hybrid Auditing Strategy](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-001-hybrid-auditing.md)
  - [EFCore ADR-006: Command-Query Separation (CQS) in Repository Pattern](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-006-cqs-repository.md)
  - [EFCore ADR-007: Dynamic Global Query Filters via Reflection](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-007-dynamic-query-filters.md)
  - [EFCore ADR-010: Decoupling Multi-Tenancy from Auditing Infrastructure](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-010-decoupling-multitenancy.md)
  - [EFCore ADR-011: Null Object Pattern for IEntityLifecycleProcessor](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/adr-011-entity-lifecycle-processor-null-object.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第1波着手前の課題 (Wave 1 Pain Points)**:
  - **ORM 密結合と依存の漏洩 (`Persistence ADR-001`)**:
    エンティティやデータアクセスのインターフェースが `VK.Blocks.Persistence.EFCore` 内に混在しており、EF Core を使用しない軽量サービスや、純粋なドメイン層にまで `Microsoft.EntityFrameworkCore` のバイナリ依存が強制されていた。
  - **単一巨大リポジトリの責務混配 (`Persistence ADR-002`, `EFCore ADR-006`)**:
    従来の `IRepository<T>` では、単なる読み取り専用クエリ（No-Tracking 最適化が必要）と、変更追跡（Change Tracker）を伴う更新、さらには一括バッチ操作や特権システム操作が同一インターフェースに露出しており、インターフェース分離原則（ISP）および CQS に反していた。
  - **監査とマルチテナントの関心事汚染 (`EFCore ADR-010`)**:
    `IAuditProvider` の中にテナント情報（`TenantId`）の解決責務が混入し、単一責任の原則（SRP）を損ねていた。
  - **機能無効化時の DI 破綻 (`EFCore ADR-011`)**:
    監査や論理削除をオプトアウトした場合に DI 解決エラー（NullReferenceException）が発生し、Fail-Fast なコンストラクタ注入が困難であった。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - 全社マイクロサービス群が Clean Architecture に準拠し、ドメインロジックをインフラストラクチャ（EF Core 等）から完全に隔離した状態で単体テスト・業務開発を推進できるようにするため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **Port & Adapter 分離 (`ARCH-02`, `ARCH-03`)**: BCL と `VK.Blocks.Core` のみに依存する純粋な抽象ブロック `VK.Blocks.Persistence` を策定し、EF Core を第 1 のアダプターとして配置する。
  - **CQS / ISP 準拠のリポジトリ分離 (`Persistence ADR-002`)**:
    - `IVKEntityReadRepository<T>`: 読み取り専用（デフォルト `AsNoTracking`）。
    - `IVKEntityWriteRepository<T>`: 変更ライフサイクル保護。
    - `IVKEntityBulkRepository<T>`: 高スループット一括処理。
    - `IVKEntitySystemRepository<T>`: テナント・論理削除バイパスの特権処理。
  - **ハイブリッド監査アーキテクチャ (`EFCore ADR-001`)**: 通常の CRUD は EF Core インターセプターで自動処理し、Bulk 操作時は専用プロセッサーで 90 倍高速に監査情報を補完。
  - **Null Object Pattern による堅牢性 (`EFCore ADR-011`)**: `NoOpAuditProvider` および `NoOpEntityLifecycleProcessor` により、未構成時でも DI コンテナがクラッシュしない安全な Fallback を提供（`ARCH-05`）。

- **Non-Goals (スコープ外 — `BASE-06` 準拠`)**:
  - **特定プロバイダ固有の最適化**: Cosmos DB や SQLite の物理的差異の吸収はこの波のスコープ外とし、汎用 RDBMS（SQL Server / PostgreSQL）標準の EF Core 抽象を最優先とする。
  - **式木の動的コンパイルキャッシュ**: 第 2 波の極限最適化（ゼロアロケーション）フェーズに委ねる。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **中央物流ハブと標準輸送コンテナ規格 (The Central Logistics Hub & Standard Shipping Container)**
>
> 従来の倉庫は、特定の大型トラック（EF Core）の荷台形状に合わせて直接荷物を積み込んでいたため、別の小型バンや貨物列車（Dapper, Cosmos）に積み替えることが不可能でした。
> 第 1 波の改革により、荷物の梱包規格（`Persistence` ポート：`IVKUnitOfWork`, `IVKEntityRepository`）を「標準海上コンテナ」として制定しました。
> また、荷物の「荷受け専用ゲート（Read Repository）」と「出荷専用ゲート（Write Repository）」を物理的に完全に分離し、倉庫内の交通整理（Change Tracker の不要な起動防止）を達成。万一の税関検査員（Audit Provider）が不在の場合でも、ダミー検査ゲート（NoOp Provider）を通過させることで物流がストップしない自律性を確保しました。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **Standard Container (標準コンテナ規格)** | `IVKUnitOfWork`, `IVKTransaction` | Public Protocol (`Persistence`) | ORM に依存しないコミット境界とトランザクション制御ポート |
| **Receiving Gate (荷受け専用ゲート)** | `IVKEntityReadRepository<T>` | Public Protocol (`Persistence`) | 読み取り専用・No-Tracking なデータ射影と仕様クエリ |
| **Shipping Gate (出荷専用ゲート)** | `IVKEntityWriteRepository<T>` | Public Protocol (`Persistence`) | Unit of Work 管理下での安全なデータ追加・変更・削除 |
| **Bulk Loading Crane (一括積載クレーン)** | `IVKEntityBulkRepository<T>` | Public Protocol (`Persistence`) | チェッカーをバイパスした高速な一括挿入・更新・削除 |
| **Default Truck Fleet (標準トラック部隊)** | `Persistence.EFCore` | Infrastructure Adapter | EF Core を用いたポートの実装および DbContext マッピング |
| **Standby Inspector (待機検査員)** | `NoOpAuditProvider` | `Internal/` (`Persistence`) | 監査未構成時でも DI クラッシュを防ぐ Null Object フォールバック |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 ORM 非依存ポート (`VK.Blocks.Persistence`)

```csharp
// [Persistence ADR-001] [ARCH-02] 零 ORM 依存の Unit of Work
public interface IVKUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<VKResult<int>> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<VKResult<IVKTransaction>> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

// [Persistence ADR-002] CQS / ISP 準拠の直交リポジトリ
public interface IVKEntityReadRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default);
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
}

public interface IVKEntityWriteRepository<TEntity> where TEntity : class
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
}
```

### 4.2 EF Core アダプター (`VK.Blocks.Persistence.EFCore`)

```csharp
// [EFCore ADR-001] [EFCore ADR-006] リポジトリ実装基底
public class VKEFCoreRepository<TEntity> : VKEFCoreReadRepository<TEntity>, IVKEntityRepository<TEntity>
    where TEntity : class
{
    // 読み取りは AsNoTracking を徹底、書き込みは ChangeTracker を活用
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **プロジェクト数と契約の分割オーバーヘッド**:
  - `Persistence` と `Persistence.EFCore` に分離することで、インターフェース解決のための DI 登録ボイラープレートが増加する。
  - *Mitigation*: `VKPersistenceEFCoreBlock` 側で標準リポジトリの一括登録（`TryAddScoped(typeof(IVKEntityRepository<>), ...)`）を提供し、アプリ側の構成を 1 行（`AddVKPersistenceEFCoreBlock()`）に集約（`BB.03`）。
- **Global Query Filter のリフレクション適用**:
  - `ISoftDelete` 等の動的フィルター注入（`EFCore ADR-007`）において初期モデルビルド時にリフレクションを使用。
  - *Mitigation*: 初回 1 回のみの実行であり、リクエストごとの実行ではないため許容。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

- [x] **Phase 1 (Abstractions Extraction)**: `Persistence` パッケージの新設、`IVKUnitOfWork`、`IVKTransaction`、および `IVKAuditProvider` の定義。
- [x] **Phase 2 (Repository Role Segregation)**: `IVKEntityReadRepository`, `IVKEntityWriteRepository`, `IVKEntityBulkRepository`, `IVKEntitySystemRepository` への直交分割。
- [x] **Phase 3 (EF Core Adapter Alignment)**: `VKEFCoreRepository` の実装、`NoOpAuditProvider` / `NoOpEntityLifecycleProcessor` のフェイルセーフ整備。

