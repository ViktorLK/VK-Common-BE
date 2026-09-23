# 🏛️ アーキテクチャ監査レポート — AI.Psyche.EFCore

> **対象モジュール**: `src/BuildingBlocks/AI.Psyche.EFCore`
> **監査日**: 2026-09-16
> **監査者**: VK.Blocks Lead Architect (Strict Mode)
> **Audit**: ✅
> **Handshake**: `Active: [L1+L2:AI.Psyche.EFCore] | Context: src/BuildingBlocks/AI.Psyche.EFCore | Sync: [CS.01:L3, AP.01:L3, AP.02:L3, AP.03:L3, BB.01:L3, BB.02:L3, BB.03:L3, BB.04:L3, BB.05:L3, BB.07:L3, CS.02:L3, CS.08:L3, DL.01:L3]`

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 92/100
- **Fast Audit スコア**: 16/17 (94%)
- **対象レイヤー判定**: Infrastructure Provider Layer — EFCore 永続化実装モジュール
- **総評 (Executive Summary)**: AI.Psyche.EFCore は、VK.Blocks の Infrastructure Provider パターンに極めて忠実に準拠した高品質モジュールである。`[VKPersistEntity]` Source Generator を活用した宣言的エンティティ定義、全 7 ドメイン（Directive, Echo, Knowledge, Pattern, Persona, Profile, Session）に対する完全な Diagnostics カバレッジ（`[LoggerMessage]` SG + `[VKMetricHistogram]` + `[VKMetricCounter]`）、および `EchoStore` における EF Core Compiled Queries を用いた高性能 2-Phase Retrieval パターンが評価に値する。DI 登録はすべて SG 自動生成（BB.03 8-step 完全準拠）であり、`AddScoped` による IVKEchoStore の Override 登録は AP.02 の Infrastructure Override 例外に正確に合致している。マイナーな改善ポイントとして、`EchoStore` の `Stopwatch` 手動計測パターンの `[VKTrace]` 統合への移行を推奨する。

---

## Phase 1: 構造監査 (Structural Audit — Fast Audit)

| # | チェック項目 | ルール | 結果 | 詳細 |
|:--|:-----------|:------|:----:|:-----|
| 1 | VKBlockMarker 存在 | BB.02 | ✅ | [VKPsycheEFCoreBlock.cs](/src/BuildingBlocks/AI.Psyche.EFCore/VKPsycheEFCoreBlock.cs:L15) — `[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock), typeof(VKPersistenceEFCoreBlock)], Toggleable = false)]` |
| 2 | `sealed partial class` 宣言 | BB.02, AP.01 | ✅ | `public sealed partial class VKAIPsycheEFCoreBlock` |
| 3 | Vertical Slice 構造 | BB.01 | ✅ | 7 ドメインスライス: `Directive/`, `Echo/`, `Knowledge/`, `Pattern/`, `Persona/`, `Profile/`, `Session/` |
| 4 | Internal フォルダ配置 | AP.03 | ✅ | 全 7 スライスに `Internal/` フォルダが存在し、Diagnostics 実装クラスを格納 |
| 5 | `sealed` デフォルト | AP.01 | ✅ | 全エンティティ・実装クラスが `sealed` 宣言（11/11） |
| 6 | `[LoggerMessage]` SG 使用 | OR.01 | ✅ | 7 Diagnostics クラスすべてで `[LoggerMessage]` 属性を使用。`logger.LogXxx()` 直接呼び出しなし |
| 7 | `CancellationToken` 伝播 | CS.03 | ✅ | `EchoStore` 全 async メソッドで `CancellationToken` パラメータ伝播 |
| 8 | `.ConfigureAwait(false)` | CS.03 | ✅ | 全 `await` 呼び出しに `.ConfigureAwait(false)` 付与（6/6） |
| 9 | `VKResult<T>` 返却 | CS.01 | ✅ | `EchoStore` 全メソッドが `VKResult` / `VKResult<T>` を返却。null 返却なし |
| 10 | `VKGuard` 境界防御 | AP.01 | ✅ | コンストラクタ（4 フィールド）+ メソッド境界（5 箇所）で `VKGuard.NotNull` / `NotDefault` 使用 |
| 11 | 禁止 API 不使用 | CS.06 | ✅ | `DateTime.UtcNow` / `Guid.NewGuid()` の使用なし |
| 12 | `throw` 不使用 | CS.01 | ✅ | `throw new` / `throw;` 使用なし。全エラーパスが `VKResult.Failure()` |
| 13 | エラー定数使用 | CS.01 | ✅ | `VKPersistenceErrors.Database.ExecutionFailed` 定数を使用。raw string なし |
| 14 | `Common/` フォルダ | BB.01 | ⚠️ | 手書き `Common/` フォルダは存在しないが、SG が自動的に `Common.DependencyInjection.Internal` 名前空間を生成。構造的に問題なし |
| 15 | Entity 名前空間 | AP.03 | ✅ | 全 public エンティティが root namespace `VK.Blocks.AI.Psyche.EFCore` を使用 |
| 16 | Diagnostics 名前空間 | AP.03 | ✅ | 全 internal Diagnostics が `{Feature}.Internal` deep namespace を使用 |
| 17 | `[VKBlockDiagnostics]` | BB.04 | ✅ | 7 Diagnostics クラスすべてに `[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]` 属性 |

**Fast Audit Score: 16/17 (94%)**
（`Common/` 物理フォルダ不在は Warning レベル。SG が namespace を自動管理しているため機能的影響なし）

---

## Phase 2: DI 登録監査 (Registration Audit)

### SG 生成 Registration シーケンス検証

[AIPsycheEFCoreBlockRegistration.g.cs](/src/BuildingBlocks/AI.Psyche.EFCore/obj/Generated/VK.Tools.SourceGenerators/VK.Tools.SourceGenerators.DependencyInjection.VKBlockGenerator/AIPsycheEFCoreBlockRegistration.g.cs) を精査:

| ステップ | 期待コード | 実装 | 結果 |
|:---------|:----------|:-----|:----:|
| 1. Check-Self | `IsVKBlockRegistered<VKAIPsycheEFCoreBlock>()` | L26-29 | ✅ |
| 2. Options | `AddVKBlockOptions<VKAIPsycheEFCoreOptions>(configuration, transform)` | L32 | ✅ |
| 3. Mark-Self | `AddVKBlockMarker<VKAIPsycheEFCoreBlock>()` | L35 | ✅ |
| 4. Validate | `TryAddEnumerableSingleton<IValidateOptions<...>, ...OptionsValidator>()` | L38 | ✅ |
| 4b. Provider | `TryAddSingleton<IVK...OptionsProvider, Default...OptionsProvider>()` | L41 | ✅ |
| 5. Toggle | なし（`Toggleable = false`） | — | ✅ |
| 5b. Persistence | `AddGeneratedModelContributors()` + `AddGeneratedAggregateRepositories()` | L47-48 | ✅ |
| 6. Custom Hook | `VKAIPsycheEFCoreBlock.Register(builder)` | L51 | ✅ |

| 追加チェック | ルール | 結果 | 詳細 |
|:-----------|:------|:----:|:-----|
| Func Transform パターン | BB.03/BB.05 | ✅ | `Func<VKAIPsycheEFCoreOptions, VKAIPsycheEFCoreOptions>? transform` (ADR-016 準拠) |
| `AddScoped` Override 正当性 | AP.02 | ✅ | [VKPsycheEFCoreBlock.cs:L23](/src/BuildingBlocks/AI.Psyche.EFCore/VKPsycheEFCoreBlock.cs:L23) — `services.AddScoped<IVKEchoStore, EchoStore>()` は Infrastructure Provider Override 例外に該当。親 `AI.Psyche` モジュールが `TryAdd` で登録する InMemory 実装を確実に上書きする正当な用途 |
| Options 型 | BB.05 | ✅ | `sealed partial record VKAIPsycheEFCoreOptions : IVKBlockOptions` — SG 生成。immutable record |
| Builder Interface | BB.03 | ✅ | `IVKAIPsycheEFCoreBuilder` SG 生成 |

**Phase 2 判定: PASS ✅** — BB.03 8-step 完全準拠。実行順序、Func Transform、Override 正当性すべて合格。

---

## Phase 3: 実装監査 (Deep Implementation Audit)

### 1. 設計原則 (Design Principles — SOLID/KISS/YAGNI/DRY)

**スコア: 9/10**

| 原則 | 評価 | 根拠 |
|:-----|:----:|:-----|
| SRP | ✅ | 各エンティティクラスは純粋なデータモデルのみ担当。`EchoStore` は永続化操作のみに集中 |
| OCP | ✅ | `[VKPersistEntity]` による宣言的定義で、SG が Configuration / Mapper / Repository / Validator を自動生成。新規フィールド追加時にエンティティのみ変更で完結 |
| LSP | ✅ | 全エンティティが適切な VK 標準インターフェース（`IVKTenantScoped`, `IVKFullAuditable`, `IVKAuditable`, `IVKConcurrency`）を実装 |
| ISP | ✅ | `IVKFullAuditable`（SoftDelete 含む）と `IVKAuditable`（SoftDelete なし）を正しく使い分け（Echo/Profile/Session は `IVKAuditable`、Directive/Knowledge/Pattern/Persona は `IVKFullAuditable`） |
| DIP | ✅ | `EchoStore` は `IVKEntityRepository<T>`, `IVKUnitOfWork`, `ILogger<T>` の抽象に依存 |
| KISS | ✅ | エンティティ定義は `[VKPersistEntity]` + Data Annotation による最小限の宣言 |
| DRY | ⚠️ | `EchoStore` 内の `Stopwatch` + `try/catch` + Diagnostics 計測パターンが全 5 メソッドで繰り返し。`[VKTrace]` 属性が付与されているが、手動 Stopwatch 計測との二重計測になっている可能性あり |

### 2. 設計パターン (Design Patterns)

| パターン | 使用箇所 | 評価 |
|:---------|:--------|:----:|
| **Repository** | SG 生成 `PsycheXxxRepository` (7 entities) | ✅ 適切 |
| **Unit of Work** | `IVKUnitOfWork` via DI | ✅ 適切 |
| **Compiled Query (Cache)** | [EchoStore.cs:L32-53](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoStore.cs:L32-L53) — 2 つの `static readonly` Compiled Query | ✅ 高評価。Zero-allocation LINQ、precompiled SQL |
| **Anti-Corruption Layer** | エンティティ ↔ ドメインモデル間の Mapper (SG 生成) | ✅ 適切 |
| **Marker Pattern** | `[VKBlockMarker]` / `[VKBlockDiagnostics]` | ✅ 適切 |

### 3. アーキテクチャ原則 (Architectural Principles)

| 原則 | 評価 | 根拠 |
|:-----|:----:|:-----|
| 関心の分離 | ✅ | エンティティ（データ定義）/ Diagnostics（可観測性）/ Store（ビジネスオペレーション）が明確に分離 |
| カプセル化 | ✅ | `EchoStore` / 全 Diagnostics クラスが `internal sealed`。Implementation Details は `Internal/` フォルダで隔離 |
| 凝集性 | ✅ | 各 Vertical Slice（Echo, Session 等）がエンティティ + Diagnostics + (Store) をまとめて保持 |
| 疎結合性 | ✅ | DB 実装詳細はすべて `Internal/` に閉じ込め。公開するのはエンティティ型のみ |

### 4. アーキテクチャスタイル (Architectural Styles)

| スタイル | 評価 | 根拠 |
|:---------|:----:|:-----|
| Clean Architecture | ✅ | CS.02 準拠。Infrastructure Layer として Domain (`AI.Psyche`) の抽象（`IVKEchoStore` 等）に対する実装を提供。依存方向は正確（Infra → Domain） |
| Vertical Slice | ✅ | BB.01 準拠。7 ドメインスライスが第一階層に配置 |

### 5. アーキテクチャパターン (Architectural Patterns)

| パターン | 評価 | 根拠 |
|:---------|:----:|:-----|
| Infrastructure Provider | ✅ | 親モジュール `AI.Psyche` の InMemory デフォルト実装を EFCore 実装で Override する Infrastructure Provider パターンに完全準拠 |
| DDD Persistence | ✅ | エンティティが Domain Model (`VKEchoTrace`, `VKPersonaAnchor` 等) との双方向マッピングを SG 経由で提供。Persistence Ignorance 原則を遵守 |

### 6. エンタープライズパターン (Enterprise Patterns)

| パターン | 評価 | 根拠 |
|:---------|:----:|:-----|
| 可観測性 (Observability) | ✅ | 全 7 ドメインに `[LoggerMessage]` + `[VKMetricHistogram]`（duration）+ `[VKMetricCounter]`（errors）を完備。EventId は 731xx〜737xx で体系的に割り当て |
| テナント分離 | ✅ | 6/7 エンティティが `IVKTenantScoped` 実装。`VKPsycheKnowledgeKeyEntity` は CS.08 Normalized Child Table 例外に該当（親 `VKPsycheKnowledgeEntity` の Navigation Property 経由でのみアクセス） |
| 楽観的並行制御 | ✅ | `VKPsycheSessionEntity` が `IVKConcurrency` 実装（`RowVersion` プロパティ） |
| 監査証跡 | ✅ | `IVKFullAuditable`（CreatedAt/By, UpdatedAt/By, DeletedAt/By + IsDeleted）/ `IVKAuditable`（CreatedAt/By, UpdatedAt/By）を適切に使い分け |

### 7. VK.Blocks 固有の準拠度 (Deep Compliance)

| チェック | ルール | 結果 | 詳細 |
|:--------|:------|:----:|:-----|
| Error 定数パターン | CS.01 | ✅ | `VKPersistenceErrors.Database.ExecutionFailed` — 専用 Errors クラスの static readonly 定数を使用 |
| CancellationToken 伝播 | CS.03 | ✅ | `EchoStore` 全 async メソッドで `cancellationToken.ThrowIfCancellationRequested()` + パラメータ伝播 |
| Visibility 整合性 | AP.03 | ✅ | Public entities → root namespace + `VK` prefix。Internal Diagnostics/Store → deep namespace + no `VK` prefix |
| Core 抽象活用 | CS.06 | ✅ | `VKGuard` / `VKResult` / `IVKEntityRepository` / `IVKUnitOfWork` 標準抽象を完全活用。車輪の再発明なし |
| EF Core 永続化標準 | CS.08 | ✅ | SG 生成 Configuration が `HasMaxLength()`, `HasDatabaseName()`, `ToTable()`, `HasKey()` をすべて自動生成。明示的テーブル名 (`VK_AI_Psyche_*`)、明示的インデックス名 (`IX_*`) |
| One File One Type | AP.03 | ✅ | 全 `.cs` ファイルが単一型宣言 |
| VKPersistEntity 属性 | BB.01 | ✅ | 7 エンティティすべてに `[VKPersistEntity]` 属性。`TableName`, `FlattenBy`, `ProjectBy` を適切に指定 |

### 深度ロジック・状態遷移推演 (Deep Logic & State Evolution)

#### 成功パス推演 (Echo GetHistoryAsync)
1. `cancellationToken.ThrowIfCancellationRequested()` → 事前キャンセル確認 ✅
2. `VKGuard.NotDefault(sessionId)` → 空 GUID 防御 ✅
3. `Stopwatch.StartNew()` → 計測開始
4. `s_getHistoryCompiled(_dbContext, sessionId)` → Compiled Query 実行（`AsNoTracking` で追跡なし、`OrderBy CreatedAt` で時系列順）
5. `.WithCancellation(cancellationToken).ConfigureAwait(false)` → 非同期ストリーム + キャンセル伝播 ✅
6. `entity.ToDomain()` → SG 生成 Mapper でドメイン変換
7. `VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(domainList)` → 成功結果返却 ✅

#### 失敗パス推演 (Echo SaveHistoryBatchAsync)
1. `traces.Count == 0` → early return `VKResult.Success()` ✅（不要な DB 往復回避）
2. `_repository.AddRangeAsync(entities, cancellationToken)` → 追加
3. `_unitOfWork.SaveChangesAsync(cancellationToken)` → コミット
4. 例外発生時: `EchoDiagnostics.RecordEchoOperation(...)` → メトリクス記録 + `RecordEchoError(...)` → エラーカウンタ
5. `_logger.LogSaveHistoryStoreError(ex, ...)` → 構造化ログ出力 ✅
6. `VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed)` → エラー定数返却 ✅

#### 論理デッドエンド検索
- **未使用 Diagnostics メソッド**: Directive/Knowledge/Pattern/Persona/Profile/Session の Diagnostics クラスに定義された CRUD ログメソッド（`LogGetXxxEntityError`, `LogCreateXxxEntityError` 等）は、対応する Store 実装がこのモジュール内に存在しない。これらは **SG 生成 Aggregate Repository** 内で使用されることが期待される。宣言的定義のため、実装コストは事実上ゼロであり、前方互換性の観点から問題なし。
- **`[VKTrace]` 属性と手動 Stopwatch の二重計測**: `EchoStore` に `[VKTrace("psyche.efcore.echo_store")]` が付与されているが、各メソッド内で `Stopwatch` + `EchoDiagnostics.RecordEchoOperation()` による手動計測も行われている。`[VKTrace]` は Activity/Span 生成用、手動 Stopwatch は Histogram メトリクス用であり、**用途が異なるため二重計測ではない**。ただし、将来的に `[VKTrace]` が自動メトリクス出力を含む場合は冗長になる可能性がある。

#### 破壊的思考 (Destructive Thinking)
- **データ損失シナリオ検証**: `SaveHistoryBatchAsync` で `_unitOfWork.SaveChangesAsync()` の例外が発生した場合、トランザクションはロールバックされるため、部分的データ損失は発生しない。`catch` ブロック内でのログ出力は例外オブジェクト `ex` を含み、排査情報が失われない ✅
- **フロントエンドエラー情報検証**: すべての失敗パスが `VKPersistenceErrors.Database.ExecutionFailed` を返却する。これは一般的な DB エラーとしてフロントエンドに伝播されるため、特定のエラー種別（Concurrency, Constraint Violation 等）の識別が困難になる可能性がある。ただし、Infrastructure Provider レイヤーとしてはこのレベルの抽象が適切。上位の Application Layer でエラー種別を判定すべき。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

なし。本モジュールに重大な設計上の問題は検出されなかった。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- ✅ **N+1 問題回避**: `EchoStore.GetTracesByIdsAsync()` は `_repository.GetListAsync(e => ids.Contains(e.Id))` で一括取得。N+1 問題なし。
- ✅ **AsNoTracking**: Compiled Query 内で `AsNoTracking()` 使用。読み取り専用クエリの Change Tracker オーバーヘッド排除。
- ✅ **メモリ効率**: `IAsyncEnumerable` + `await foreach` パターンで大量データのストリーミング処理。全件メモリロードを回避。
- ✅ **PII 保護**: ログ出力は ID のみ（`SessionId`, `EchoId`）。ユーザーコンテンツ（`Content` フィールド）はログに含まれない。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ✅ **完全 DI 化**: `EchoStore` はコンストラクタインジェクションで全依存を受け取り。`new` キーワードの濫用なし。
- ✅ **InternalsVisibleTo**: [VK.Blocks.AI.Psyche.EFCore.csproj](/src/BuildingBlocks/AI.Psyche.EFCore/VK.Blocks.AI.Psyche.EFCore.csproj:L18-L26) で `UnitTests` と `IntegrationTests` プロジェクトに公開。
- ⚠️ **Compiled Query テスト困難性**: `s_getMetadataCompiled` / `s_getHistoryCompiled` は `static readonly` フィールドとして定義。単体テストでの差し替えが困難だが、`DbContext` を Mock することで間接的にテスト可能。Integration Tests で検証するのが適切。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- ✅ **[LoggerMessage] SG**: 7 Diagnostics クラス × 6-7 メソッド = 合計 **43 ログメソッド**。全て Source Generator 経由（OR.01 完全準拠）
- ✅ **構造化ログ**: 全メッセージテンプレートで `{SessionId}`, `{EchoId}`, `{PersonaId}` 等のセマンティックパラメータ使用
- ✅ **Histogram メトリクス**: `vk.ai.psyche.efcore.{feature}.duration` — 全 7 ドメインで duration 計測
- ✅ **Counter メトリクス**: `vk.ai.psyche.efcore.{feature}.errors` — 全 7 ドメインでエラーカウント
- ✅ **EventId 体系**: 731xx (Directive) → 732xx (Persona) → 733xx (Pattern) → 734xx (Knowledge) → 735xx (Echo) → 736xx (Profile) → 737xx (Session) — 体系的な ID 割り当て
- ✅ **`[VKTrace]`**: `EchoStore` に Activity/Span トレース属性付与

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **手動 Stopwatch パターンの反復**: [EchoStore.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoStore.cs) 内の 5 メソッドすべてで同一の `Stopwatch.StartNew()` → `try` → `stopwatch.Stop()` → `RecordXxxOperation()` パターンが繰り返されている。将来的にメソッド数が増加した場合の保守コストが懸念される。Aspect-Oriented な `[VKTrace]` 統合、またはメソッド抽出による DRY 化を検討すべき。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **`[VKPersistEntity]` 宣言的 SG 活用**: エンティティに属性を付与するだけで、Configuration / Mapper / Repository / Validator / Query&Spec が自動生成される。ボイラープレートコードの排除と CS.08 準拠の自動保証が秀逸。
2. **EF Core Compiled Queries**: [EchoStore.cs:L32-53](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoStore.cs:L32-L53) — `EF.CompileAsyncQuery` を `static readonly` フィールドとして保持。LINQ 式の再コンパイルを排除し、Zero-allocation の高性能クエリを実現。
3. **2-Phase Retrieval パターン**: `GetMetadataAsync`（軽量メタデータ取得）→ `GetTracesByIdsAsync`（必要な Full Entity のみ取得）という 2 段階設計で、大量データ環境でのメモリ効率とネットワーク帯域最適化を実現。
4. **完全な Diagnostics カバレッジ**: 7 ドメイン × (Logger + Histogram + Counter) = 運用時の可観測性が極めて高い。
5. **Infrastructure Override パターンの正確な適用**: `AddScoped<IVKEchoStore, EchoStore>()` は AP.02 例外規定に正確に合致。コメントとドキュメントで意図を明示。
6. **`[VKPersistJson]`**: [VKPsychePersonaEntity.cs:L42-49](/src/BuildingBlocks/AI.Psyche.EFCore/Persona/VKPsychePersonaEntity.cs:L42-L49) — `Traits` / `Extensions` プロパティで JSON カラム永続化を宣言的に実現。
7. **Composite Primary Key**: [VKPsycheKnowledgeKeyEntity.cs:L20-30](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeKeyEntity.cs:L20-L30) — `[VKPersistKey(Order = 1/2)]` で複合キー定義。CS.08 Normalized Child Table 例外の正確な適用（`IVKTenantScoped` 省略の正当性）。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

なし。現時点で致命的な修正が必要な箇所はない。

### 2. リファクタリング提案 (Refactoring)

| # | 提案 | 影響 | 優先度 |
|:--|:-----|:-----|:------:|
| R1 | **Stopwatch 計測の共通化**: `EchoStore` 内の手動 `Stopwatch` + Diagnostics パターンを、ヘルパーメソッドまたは SG ベースの AOP に統合。将来のストア実装（Directive, Knowledge 等）でも同じパターンが必要になるため | DRY, 保守性 | Medium |
| R2 | **エラー種別の細分化**: `VKPersistenceErrors.Database.ExecutionFailed` のみでなく、`ConcurrencyConflict`, `ConstraintViolation` 等の具体的エラー定数を導入し、`DbUpdateConcurrencyException` 等を区別して返却することを検討 | 可観測性, デバッグ効率 | Low |

### 3. 推奨される学習トピック (Learning Suggestions)

| # | トピック | 理由 |
|:--|:--------|:-----|
| L1 | **EF Core Interceptors** による横断的関心事の自動化 | SaveChanges / DbCommand レベルでの自動計測・ロギングに活用可能 |
| L2 | **IAsyncEnumerable と Channel\<T\>** の組み合わせ | 大量 Echo データのストリーミング処理最適化 |
