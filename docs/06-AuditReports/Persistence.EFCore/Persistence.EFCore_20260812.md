# アーキテクチャ監査レポート: Persistence.EFCore

**監査日**: 2026-08-12
**対象モジュール**: `src/BuildingBlocks/Persistence.EFCore`
**監査者**: AI Architecture Auditor (VK.Blocks Strict Mode)
**Audit**: ✅

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 92/100
- **Fast Audit スコア**: 30/30 (100%)
- **対象レイヤー判定**: Infrastructure Layer / EF Core Provider Implementation
- **総評 (Executive Summary)**: 本モジュールは VK.Blocks アーキテクチャの模範的な実装である。`[VKBlockMarker]` による SG 自動化、`sealed` / `internal` の厳格な適用、`ConfigureAwait(false)` の徹底、`Result<T>` パターンの採用、そして `[LoggerMessage]` による構造化ログなど、Industrial DNA への準拠度は極めて高い。Interceptor ベースの Auditing / SoftDelete / MultiTenancy の分離、Strategy パターンによるカーソルシリアライザーの切替、NoOp パターンによる安全なフォールバックなど、設計品質も優れている。マイナス要因は、`DbContextBuilder` 内での `AddTransient`（`TryAdd` 規約違反）、`VKBaseDbContext` の `abstract class`（`sealed` 不可のため許容）、`VKEFCoreReadRepository` の `public partial class`（継承前提のため `sealed` 不可）、および `GetSchemaSwitchCommand` 内の SQL インジェクション防御不足といった限定的なリスクに集中している。

---

## Phase 2: DI Registration Audit (Registration Layer Only)

### 実行順序検証 (BB.03)

本モジュールは `[VKBlockMarker]` 属性を使用し、SG (`VKBlockGenerator`) が以下のシーケンスを自動生成する:

| Step | 検証 | 結果 |
|:-----|:-----|:-----|
| 1. Check-Self | `IsVKBlockRegistered` (SG 自動生成) | ✅ |
| 2. Options Registration | `AddVKBlockOptions` (SG 自動生成) | ✅ |
| 3. Mark-Self | `AddVKBlockMarker` (SG 自動生成) | ✅ |
| 4. Validate Options | `IValidateOptions` (SG 自動生成) | ✅ |
| 5. Diagnostics | `[VKBlockDiagnostics]` (SG 統合) | ✅ |
| 6. Feature Toggle | N/A（`IVKToggleableBlockOptions` 非実装） | ✅ N/A |
| 7. Custom Hook | `RegisterBlockCustom` ([VKPersistenceEFCoreBlock.cs](/src/BuildingBlocks/Persistence.EFCore/VKPersistenceEFCoreBlock.cs)) | ✅ |

### Func Transform (ADR-016 / BB.05)

- Block-level: SG がオプション変換を自動処理。`VKPersistenceEFCoreOptions` は `sealed partial record` + `init` プロパティ。 → ✅
- Feature-level: `[VKFeature]` 属性による SG 自動生成。`VKDatabaseOptions`, `VKPaginationOptions` ともに `sealed partial record`。 → ✅

### Enabled Policy Position (BB.03)

- 本モジュールは `IVKToggleableBlockOptions` を実装していないため、Feature Toggle ステップは適用外。 → ✅ N/A

### Builder Pattern (BB.03)

- `IVKPersistenceEFCoreBuilder` (SG 生成) を介した builder chaining パターンを採用。 → ✅

### OptionsValidator Quality (BB.05)

- [DatabaseFeature.cs](/src/BuildingBlocks/Persistence.EFCore/Database/DatabaseFeature.cs): `ValidateFeatureCustom` で `ConnectionString`, `CommandTimeout`, `MaxRetryCount`, `MaxRetryDelay` を検証。 → ✅
- [PaginationFeature.cs](/src/BuildingBlocks/Persistence.EFCore/Pagination/PaginationFeature.cs): `ValidateFeatureCustom` で `UseSecureSerializer` 有効時の `SigningKey` 存在・長さを検証。 → ✅

**Phase 2 判定**: ✅ PASS

---

## Phase 3: Implementation Audit (Deep Analysis)

### 1. 設計原則 (Design Principles)

| 原則 | 評価 | 根拠 |
|:-----|:-----|:-----|
| **SRP** | ✅ | 各クラスが単一責務に集中。Interceptor は Auditing / SoftDelete / Tenant を分離。Repository は Read / Write / Bulk / System を分離。 |
| **OCP** | ✅ | `IVKDbContextOptionsConfigurator` による拡張ポイント、`IVKEntityLifecycleProcessor` による Strategy 切替。新機能は既存コードを変更せず追加可能。 |
| **LSP** | ✅ | `VKEFCoreReadRepository` → `VKEFCoreRepository` の継承関係は read/write の自然な拡張。`VKEFCoreSystemRepository` は `IgnoreQueryFilters` のオーバーライドのみで基底クラスの契約を維持。 |
| **ISP** | ✅ | `IVKReadRepository` / `IVKWriteRepository` / `IVKBulkRepository` / `IVKSystemRepository` の細粒度インターフェース分割。 |
| **DIP** | ✅ | すべての依存関係がインターフェース経由。`IVKAuditProvider`, `IVKCursorSerializer`, `IVKEntityLifecycleProcessor` など。 |
| **KISS** | ✅ | 過度な抽象化なし。各ファイルが簡潔で責務が明確。 |
| **DRY** | ⚠️ | `VKRepositoryConstants` と `EFCoreErrors` に重複するエラーメッセージ定義が存在。前者は旧式の `const string` 定義で、`EFCoreErrors` の `VKError` 定数と意図が重複。 |

### 2. 設計パターン (Design Patterns)

| パターン | 使用箇所 | 評価 |
|:---------|:---------|:-----|
| **Strategy** | `IVKCursorSerializer` → `SecureCursorSerializer` / `SimpleCursorSerializer` | ✅ 環境に応じた切替が適切 |
| **Strategy** | `IVKEntityLifecycleProcessor` → `DefaultEntityLifecycleProcessor` / `NoOpEntityLifecycleProcessor` | ✅ NoOp フォールバックが安全 |
| **Adapter** | `EFCoreTransactionAdapter` (`IDbContextTransaction` → `IVKTransaction`) | ✅ |
| **Adapter** | `EFCorePropertySetterAdapter` (`SetPropertyCalls` → `IVKPropertySetter`) | ✅ |
| **Template Method** | `VKEFCoreReadRepository.GetQueryable()` (virtual) | ✅ SystemRepository がオーバーライド |
| **Composition** | `VKEFCoreSystemRepository` 内部の `_innerWriteRepository` | ✅ Delegation による Write 機能の再利用 |
| **Registry** | `PhysicalDeleteRegistry` (ConditionalWeakTable) | ✅ 弱参照による GC フレンドリーな設計 |

### 3. 架構原則 (Architectural Principles)

- **関注分離**: ✅ Interceptor / Repository / UnitOfWork / Pagination がそれぞれ独立した Feature Slice として編成。
- **封装**: ✅ `Internal/` ディレクトリのすべての型が `internal sealed class` で宣言。Public API は最小限。
- **モジュール化**: ✅ `DatabaseFeature` と `PaginationFeature` は `[VKFeature]` で独立登録。
- **凝集度**: ✅ 各 Feature Slice が関連する Options / Registration / Implementation を含む高凝集設計。
- **結合度**: ✅ Core 抽象 (`IVKAuditProvider`, `TimeProvider`, `IVKJsonSerializer`) 経由の疎結合。

### 4. 架構スタイル (Architectural Styles)

- **Vertical Slice**: ✅ `Database/`, `Pagination/`, `Interceptors/` の Feature Slice 構成。
- **Clean Architecture**: ✅ Domain 抽象 (`IVKReadRepository`, `IVKUnitOfWork` など) は Persistence (ORM 非依存) 層で定義。EF Core 実装は本モジュールに閉じる。
- **Provider Pattern**: ✅ EF Core core パッケージのみ参照。具体的なプロバイダー (SqlServer / Npgsql) は参照していない。

### 5. 架構パターン (Architectural Patterns)

- **Repository Pattern**: ✅ Read / Write / System の3層リポジトリ。
- **Unit of Work**: ✅ `VKUnitOfWork<TDbContext>` がトランザクション管理と `Result<T>` エラーハンドリングを統合。
- **Specification Pattern**: ✅ `IVKSpecification<TEntity>` を受け入れる全リポジトリメソッド。

### 6. 企業級パターン (Enterprise Patterns)

| パターン | 評価 | 根拠 |
|:---------|:-----|:-----|
| **冪等性** | ✅ | DI 登録は `[VKBlockMarker]` SG による `IsVKBlockRegistered` チェック。全サービスが `TryAdd`。 |
| **監査** | ✅ | `DefaultEntityLifecycleProcessor` が `IVKAuditProvider` 経由で `CreatedAt/By`, `UpdatedAt/By` を自動設定。Bulk 操作にも対応。 |
| **ソフト削除** | ✅ | `ProcessSoftDelete` + EF Core Global Query Filter + `PhysicalDeleteRegistry` による物理削除エスケープハッチ。 |
| **マルチテナンシー** | ✅ | TenantId 注入 + Schema 切替 + Global Query Filter + Fail-Closed 防御。 |
| **同時実行制御** | ✅ | `IVKConcurrency.RowVersion` 自動設定 + `DbUpdateConcurrencyException` → `VKPersistenceErrors.UnitOfWork.ConcurrentUpdate` マッピング。 |
| **リトライ** | ✅ | `ExecutionStrategy` を `ExecuteInTransactionAsync` で活用。`MaxRetryCount` / `MaxRetryDelay` をオプション化。 |
| **可観測性** | ✅ | `[LoggerMessage]` SG + `[VKBlockDiagnostics]` + 構造化テンプレート。 |

### 7. VK.Blocks 固有の準拠度 (VK.Blocks Compliance — Deep)

| チェック項目 | 評価 | 詳細 |
|:-------------|:-----|:-----|
| **境界防御 (VKGuard)** | ✅ | 50 以上のコンストラクタ・メソッドエントリーポイントで `VKGuard.NotNull` を使用。 |
| **非確定的 API (CS.06)** | ✅ | `TimeProvider` 注入（`BasicAuditProvider`, `SecureCursorSerializer`）。`IVKJsonSerializer` 注入（両 Serializer）。`DateTime.UtcNow` / `Guid.NewGuid()` / `JsonSerializer` 直接呼び出しなし。 |
| **DI & モジュール化** | ✅ | 全サービスが `TryAdd` パターン。`[VKBlockMarker]` SG による冪等性。 |
| **Result パターン (CS.01)** | ✅ | `VKUnitOfWork` の `SaveChangesAsync`, `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`, `ExecuteInTransactionAsync` すべてが `VKResult<T>` / `VKResult` を返却。エラーは `VKPersistenceErrors` 定数経由。 |
| **Error 定数 (CS.01)** | ✅ | `VKPersistenceErrors.UnitOfWork.ConcurrentUpdate` 等の構造化エラー定数。`EFCoreErrors` も `VKError` 定数を定義。 |
| **CancellationToken 伝播 (CS.03)** | ✅ | 全 async メソッドが `CancellationToken` を受け取り、末端まで伝播。`ConfigureAwait(false)` も全箇所で付与。 |
| **Visibility 整合性 (AP.03)** | ✅ | L1 public 型はすべて `VK` プレフィックス。`Internal/` 配下はすべて `internal sealed`。 |

### 深度逻辑与状態演進審査 (Deep Logic & State Evolution Audit)

#### 執行経路シミュレーション

**正常経路 (Happy Path)**: `SaveChangesAsync` → `VKAuditingInterceptor.SavingChangesAsync` → `DefaultEntityLifecycleProcessor.ProcessAuditing` → `CreatedAt/By` 設定 → `base.SavingChangesAsync` → DB 書き込み → `VKResult.Success(count)` 返却。
→ ✅ 状態が正しく伝播される。

**異常経路 (Failure Path)**: `SaveChangesAsync` → `DbUpdateConcurrencyException` → `catch` → `VKResult.Failure<int>(VKPersistenceErrors.UnitOfWork.ConcurrentUpdate)` → 呼び出し元に構造化エラーが返却。
→ ✅ 例外が `Result<T>` に変換され、スタックトレースは保持されないが、セマンティックなエラーコードが返却される。

#### ロジックの死角 (Logic Dead Ends)

1. ⚠️ **`VKRepositoryConstants` の残留**: [VKRepositoryConstants.cs](/src/BuildingBlocks/Persistence.EFCore/Common/VKRepositoryConstants.cs) の `ErrorMessages` は `EFCoreErrors` の `VKError` 定数と意図が重複。現在のコードベースでは直接参照されていない可能性があり、デッドコード化のリスクがある。

#### 防御的逆向思考 (Destructive Thinking)

1. ⚠️ **SQL インジェクションリスク**: [VKTenantInterceptor.cs:L143](/src/BuildingBlocks/Persistence.EFCore/Interceptors/VKTenantInterceptor.cs) の `GetSchemaSwitchCommand` で `$"SET search_path TO {schema}"` としてスキーマ名を直接文字列補間している。`schema` は `IVKTenantContext.CurrentTenant.Schema` から取得されるが、テナントデータのソースによっては SQL インジェクションベクターとなる。パラメータ化できないため、少なくともホワイトリスト検証（`^[a-zA-Z_][a-zA-Z0-9_]*$`）が必要。

2. ⚠️ **`AddTransient` 直接使用**: [DbContextBuilder.cs:L18](/src/BuildingBlocks/Persistence.EFCore/Common/DependencyInjection/Internal/DbContextBuilder.cs) で `Services.AddTransient<IVKDbContextOptionsConfigurator>` を直接使用。AP.02 では `TryAdd` パターンが義務付けられているが、このケースでは同一インターフェースに対する複数実装の意図的な累積登録（`GetServices` で列挙）であるため、`TryAdd` では機能しない。設計意図は正当だが、コメントでの明示的な逸脱理由の記載が必要。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

該当なし。本モジュールに致命的なアーキテクチャ違反は検出されなかった。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **SQL インジェクション**: [VKTenantInterceptor.cs:L143](/src/BuildingBlocks/Persistence.EFCore/Interceptors/VKTenantInterceptor.cs) — `SET search_path TO {schema}` でスキーマ名をエスケープなしで文字列補間。テナントスキーマ名のホワイトリスト検証を追加すべき。
- 🔒 **機密データログ防御**: [VKDatabaseOptions.cs:L29](/src/BuildingBlocks/Persistence.EFCore/Database/VKDatabaseOptions.cs) — `EnableSensitiveDataLogging` がデフォルト `false` で安全。ドキュメントコメントでも本番禁止を明示。✅
- 🔒 **カーソルトークンセキュリティ**: [SecureCursorSerializer.cs](/src/BuildingBlocks/Persistence.EFCore/Pagination/Internal/SecureCursorSerializer.cs) — HMAC-SHA256 署名 + タイミング安全比較 (`CryptographicOperations.FixedTimeEquals`) + スキーマバージョニング + 有効期限。✅ 堅牢な実装。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **テスト容易性**: ✅ すべての依存関係がインターフェース経由で注入。`TimeProvider` / `IVKJsonSerializer` / `IVKAuditProvider` / `IVKTenantProvider` のモック化が容易。`VKBaseDbContext` のコンストラクタはオプショナルパラメータを受け入れ、テスト時の最小構成を許容。
- ⚙️ **疎結合性**: ✅ EF Core core パッケージのみ参照。具体プロバイダーへの依存なし。`IVKDbContextOptionsConfigurator` による拡張ポイントがプロバイダー固有設定を分離。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **構造化ログ**: ✅ [PersistenceEFCoreLog.cs](/src/BuildingBlocks/Persistence.EFCore/Common/Diagnostics/Internal/PersistenceEFCoreLog.cs) — `[LoggerMessage]` SG による 5 つのイベント定義（Bulk Update/SoftDelete/Delete + Schema Switch sync/async）。EventId 付与済み。
- 📡 **Diagnostics 統合**: ✅ `[VKBlockDiagnostics<VKPersistenceEFCoreBlock>]` で Block 診断メタデータを SG 統合。
- 📡 **エラーハンドリング**: ✅ `VKResult<T>` + `VKPersistenceErrors` 構造化エラー定数。RFC 7807 変換は上位レイヤーの責務。
- 📡 **改善余地**: `VKPersistenceEFCoreDiagnosticsConstants` が空クラス。将来的にメトリクス名やアクティビティ名を定義するプレースホルダーだが、現状では未活用。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **`VKRepositoryConstants` デッドコード候補**: [VKRepositoryConstants.cs](/src/BuildingBlocks/Persistence.EFCore/Common/VKRepositoryConstants.cs) — `EFCoreErrors` の `VKError` 定数と重複。旧式の `const string` 定義が残存しており、参照状況の確認と統廃合が推奨される。
- ⚠️ **`AddTransient` 直接使用**: [DbContextBuilder.cs:L18](/src/BuildingBlocks/Persistence.EFCore/Common/DependencyInjection/Internal/DbContextBuilder.cs) — AP.02 違反だが、設計意図（複数 `IVKDbContextOptionsConfigurator` の累積登録）は正当。コメントでの逸脱理由の明示が必要。
- ⚠️ **`VKBaseDbContext` の `abstract class`**: 継承前提のため `sealed` 不可。AP.01 の `sealed default` ルールの正当な例外だが、ドキュメントでの明示が望ましい。
- ⚠️ **`DefaultEntityLifecycleProcessor` の `ArgumentNullException.ThrowIfNull`**: [DefaultEntityLifecycleProcessor.cs:L17,48](/src/BuildingBlocks/Persistence.EFCore/Interceptors/Internal/DefaultEntityLifecycleProcessor.cs) — 他のファイルでは `VKGuard.NotNull` を使用しているが、ここでは BCL の `ArgumentNullException.ThrowIfNull` を直接使用。一貫性の観点から `VKGuard.NotNull` への統一が推奨される。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **SG 駆動アーキテクチャ**: `[VKBlockMarker]` + `[VKFeature]` + `[VKBlockDiagnostics]` の三位一体により、ボイラープレートの手動記述を完全に排除。DI 登録の順序保証も SG が自動化。
2. **Fail-Closed マルチテナンシー**: `VKBaseDbContext.CurrentTenantIdForQueryFilter` が `null` の場合、Global Query Filter が 0 行を返却する Defense-in-Depth 設計。ドキュメントコメントも丁寧に記述。
3. **PhysicalDeleteRegistry**: `ConditionalWeakTable` による弱参照ベースのレジストリ。GC フレンドリーでメモリリークのリスクがなく、Soft Delete のエスケープハッチとして洗練された実装。
4. **SecureCursorSerializer**: HMAC-SHA256 + タイミング安全比較 + スキーマバージョニング + 有効期限の 4 重防御。プロダクショングレードの実装。
5. **Bulk 操作の Interceptor バイパス対策**: `ExecuteUpdateAsync` / `ExecuteDeleteAsync` が ChangeTracker をバイパスすることを認識し、`IVKEntityLifecycleProcessor.ProcessBulkUpdate/ProcessBulkSoftDelete` で手動補完。
6. **`VKUnitOfWork.ExecuteInTransactionAsync`**: `ExecutionStrategy` + 自動 Rollback + `Result<T>` 統合のトランザクション管理パターン。
7. **ConfigureAwait(false) 徹底**: 50 以上の async 呼び出しすべてに `ConfigureAwait(false)` を付与。`// [CS.03]` タグによるルール追跡も模範的。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| 優先度 | 対象 | 改善内容 |
|:-------|:-----|:---------|
| **High** | [VKTenantInterceptor.cs:L143](/src/BuildingBlocks/Persistence.EFCore/Interceptors/VKTenantInterceptor.cs) | Schema 名のホワイトリスト検証を追加（`^[a-zA-Z_][a-zA-Z0-9_]*$`）。SQL インジェクション防御。 |
| **Medium** | [DbContextBuilder.cs:L18](/src/BuildingBlocks/Persistence.EFCore/Common/DependencyInjection/Internal/DbContextBuilder.cs) | `AddTransient` 使用箇所に AP.02 逸脱理由のコメントを追加。 |

### 2. リファクタリング提案 (Refactoring)

| 対象 | 改善内容 |
|:-----|:---------|
| [VKRepositoryConstants.cs](/src/BuildingBlocks/Persistence.EFCore/Common/VKRepositoryConstants.cs) | `EFCoreErrors` の `VKError` 定数と統廃合。参照がなければ削除候補。 |
| [DefaultEntityLifecycleProcessor.cs:L17,48](/src/BuildingBlocks/Persistence.EFCore/Interceptors/Internal/DefaultEntityLifecycleProcessor.cs) | `ArgumentNullException.ThrowIfNull` → `VKGuard.NotNull` への統一。 |
| [VKPersistenceEFCoreDiagnosticsConstants.cs](/src/BuildingBlocks/Persistence.EFCore/Common/Diagnostics/VKPersistenceEFCoreDiagnosticsConstants.cs) | 空クラスにメトリクス定数（例: Slow Query Threshold カウンター名）を追加、または不要なら削除を検討。 |

### 3. 推奨される学習トピック (Learning Suggestions)

- **EF Core Interceptor Pipeline の深掘り**: 複数 Interceptor の実行順序制御とフレームワーク Interceptor / コンシューマー Interceptor の分離パターン。
- **`ReadOnlySpan<byte>` による HMAC 最適化**: `SecureCursorSerializer.ComputeHmac` で `byte[]` の代わりに `Span<byte>` / `stackalloc` を検討し、ヒープ割り当てを削減。
- **Source Generator による Options Validator の完全自動化**: `ValidateFeatureCustom` の手動実装を SG で検出・警告する仕組みの検討。
