# 🏛️ アーキテクチャ監査レポート — AI.Psyche.EFCore

> **モジュール**: `AI.Psyche.EFCore`
> **パス**: `/src/BuildingBlocks/AI.Psyche.EFCore`
> **監査日**: 2026-08-25
> **監査者**: Antigravity Architect Agent
> **Audit**: ✅

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **88 / 100**
- **Fast Audit スコア**: 22/22 (100%) — 全チェック項目 PASS
- **対象レイヤー判定**: Infrastructure Layer / EFCore Persistence Provider
- **総評 (Executive Summary)**: AI.Psyche.EFCore は、AI Psyche ドメイン（Directive, Echo, Knowledge, Pattern, Persona, Profile, Session）の永続化を EF Core 経由で実現するインフラストラクチャ層モジュールである。Vertical Slice ベースのフォルダ構成、`sealed` デフォルト、`VKResult<T>` パターン、`[LoggerMessage]` SG ベースのダイアグノスティクス、`TryAdd` 冪等DI登録など、VK.Blocks の工業品質基準を高水準で遵守している。主な改善領域は、一部エンティティの `Content` プロパティにおける `MaxLength` 未指定（CS.08）と、`Common/` フォルダの標準 DI 構造の省略（BB.01 デビエーション）に限定される。

---

## Phase 1: 構造監査 (Fast Audit)

| # | チェック項目 | ルール | 結果 | 備考 |
|:--|:------------|:------|:----:|:-----|
| 1 | `VKBlockMarker` 存在 | BB.02 | ✅ | `sealed partial class VKAIPsycheEFCoreBlock` + `[VKBlockMarker]` |
| 2 | Dependencies 明示 | BB.02 | ✅ | `[typeof(VKAIPsycheBlock), typeof(VKPersistenceEFCoreBlock)]` |
| 3 | マーカー名前空間 | BB.02 | ✅ | `VK.Blocks.AI.Psyche.EFCore` (ルートNS) |
| 4 | Vertical Slice 構造 | BB.01 | ✅ | 7 ドメインフォルダ (`Directive/`, `Echo/`, `Knowledge/`, `Pattern/`, `Persona/`, `Profile/`, `Session/`) |
| 5 | Internal スコーピング | AP.03 | ✅ | Store / Diagnostics は全て `Internal/` 配下、`internal` 宣言 |
| 6 | Public エンティティ名前空間 | AP.03 | ✅ | 全エンティティが `VK.Blocks.AI.Psyche.EFCore` (ルートNS) |
| 7 | Internal 名前空間 | AP.03 | ✅ | `{Feature}.Internal` 名前空間使用 |
| 8 | `sealed` デフォルト | AP.01 | ✅ | 全クラス `sealed` / `sealed partial` |
| 9 | VKGuard 境界防御 | AP.01 | ✅ | 全 Store コンストラクタ・メソッド入口で VKGuard 使用 |
| 10 | `VKResult<T>` 戻り値 | CS.01 | ✅ | 全 Store メソッドが `VKResult<T>` 返却 |
| 11 | Error 定数使用 | CS.01 | ✅ | `VKPersistenceErrors.Database.ExecutionFailed` / `Repository.EntityNotFound` |
| 12 | `throw` 不使用 | CS.01 | ✅ | 0 件 (Store 内で例外スローなし) |
| 13 | `CancellationToken` 伝播 | CS.03 | ✅ | 全 async メソッドで受け取り・伝播 |
| 14 | `.ConfigureAwait(false)` | CS.03 | ✅ | 全 await に付与 (12 箇所) |
| 15 | `[LoggerMessage]` SG | OR.01 | ✅ | 全 Diagnostics クラスで使用 (42 メソッド) |
| 16 | `logger.LogXxx()` 不使用 | OR.01 | ✅ | 直接呼び出しなし、SG 拡張メソッドのみ |
| 17 | `[VKBlockDiagnostics]` | BB.04 | ✅ | 全7フィーチャーに配置 |
| 18 | `TryAdd` DI 登録 | AP.02 | ✅ | 全7サービスが `TryAddScoped` |
| 19 | `DateTime.UtcNow` 不使用 | CS.06 | ✅ | 0 件 |
| 20 | `Guid.NewGuid()` 不使用 | CS.06 | ✅ | 0 件 |
| 21 | `[VKPersistEntity]` 使用 | CS.08 | ✅ | 全8エンティティに付与 |
| 22 | テナントスコープ | CS.08 | ✅ | 主要エンティティ全て `IVKTenantScoped` 実装 |

**Fast Audit 総評**: 22/22 (100%)

---

## Phase 2: DI 登録監査 (Registration Audit)

### BB.03 実行順序

本モジュールは `Toggleable = false` で SG 生成されたブロックであり、DI 登録の主要シーケンスは Source Generator が自動出力する。開発者が手動で記述しているのは `RegisterBlockCustom` のカスタムフック部分のみ。

| ステップ | アクション | 状態 |
|:---------|:----------|:----:|
| 1. Check-Self | SG 自動生成 (`IsVKBlockRegistered`) | ✅ |
| 2. Options | SG 自動生成 (Toggleable=false のため省略可) | ✅ |
| 3. Mark-Self | SG 自動生成 (`AddVKBlockMarker`) | ✅ |
| 4. Validate | SG 自動生成 | ✅ |
| 5. Feature Toggle | `Toggleable = false` → スキップ (正当) | ✅ |
| 6. Custom Hook | `RegisterBlockCustom` で 7 Store を `TryAddScoped` | ✅ |

### Func Transform (BB.03 / ADR-016)

本モジュールは `Toggleable = false` でブロックレベル Options を持たないため、`Func<T, T>` 変換パターンの検証は **対象外**。

### Builder Pattern (BB.03)

`IVKAIPsycheEFCoreBuilder` を使用しており、SG 生成されたビルダーインターフェースに従っている。✅

### OptionsValidator Quality (BB.05)

ブロックレベル Options が存在しないため、`IValidateOptions<T>` の検証は **対象外**。

**Phase 2 結果**: PASS

---

## Phase 3: 実装監査 (Implementation Audit — Deep Analysis)

### 1. 設計原則 (Design Principles) — SOLID / KISS / YAGNI / DRY

| 原則 | 評価 | 詳細 |
|:-----|:----:|:-----|
| **SRP** | ✅ | 各 Store は対応ドメインの永続化のみを担当。Entity は純粋なデータモデル。Diagnostics は診断ログのみ |
| **OCP** | ✅ | `IVK{Domain}Store` インターフェース経由で拡張可能。`TryAddScoped` によりアプリケーション側でオーバーライド可能 |
| **LSP** | ✅ | 全 Store は対応インターフェースの契約を正確に実装 |
| **ISP** | ✅ | 各 Store インターフェースは最小限のメソッドに絞られている（1-2メソッド/インターフェース） |
| **DIP** | ✅ | `IVKEntityReadRepository<T>`, `IVKEntityRepository<T>`, `IVKUnitOfWork` など全て抽象化経由 |
| **KISS** | ✅ | 実装は極めてシンプル。各メソッドは Guard → Query → Map → Return の明確なフロー |
| **DRY** | ⚠️ | 7 Store 間で try-catch / VKGuard / VKResult パターンが繰り返されている。ただし、SG ベースの Entity/Mapping 自動生成で補完されており、現実的な複雑度では許容範囲 |

### 2. 設計パターン (Design Patterns)

| パターン | 適用箇所 | 評価 |
|:---------|:---------|:----:|
| **Repository** | 全 Store が `IVKEntityReadRepository<T>` / `IVKEntityRepository<T>` を利用 | ✅ 適切 |
| **Unit of Work** | `EchoStore` / `SessionStore` が書き込み操作で `IVKUnitOfWork` を使用 | ✅ 適切 |
| **Marker (BB.02)** | `VKAIPsycheEFCoreBlock` によるブロック識別 | ✅ 適切 |
| **Anti-Corruption Layer** | Entity ↔ Domain マッピングを SG 生成 `ToDomain()` / `ToEntity()` / `MapOnto()` で分離 | ✅ 優秀 |

### 3. アーキテクチャ原則 (Architectural Principles)

| 原則 | 評価 | 詳細 |
|:-----|:----:|:-----|
| **関心の分離** | ✅ | Entity (データモデル) / Store (データアクセス) / Diagnostics (ログ) が完全に分離 |
| **カプセル化** | ✅ | Store / Diagnostics は `internal sealed`。External は VK 接頭辞付き Entity のみ |
| **凝集性** | ✅ | 各フィーチャーフォルダが Entity + Store + Diagnostics を自己完結的に包含 |
| **結合度** | ✅ | 全依存は抽象（インターフェース）経由。EF Core の `Include()` のみ `KnowledgeStore` で直接使用 |

### 4. アーキテクチャ風格 (Architectural Styles)

本モジュールは **Clean Architecture** のインフラストラクチャ層に位置し、**Vertical Slice** レイアウトを採用している。

- ✅ ドメイン毎の独立フォルダ構造（7スライス）
- ✅ `Internal/` による実装カプセル化
- ✅ 依存方向: Domain ← Infra (正方向)

### 5. アーキテクチャパターン (Architectural Patterns)

- **DDD 永続化**: `[VKPersistEntity]` によるドメインモデル ↔ DB エンティティの明示的マッピング ✅
- **Repository Pattern**: VK.Blocks 標準の `IVKEntityReadRepository<T>` / `IVKEntityRepository<T>` 使用 ✅

### 6. エンタープライズパターン (Enterprise Patterns)

| パターン | 評価 | 詳細 |
|:---------|:----:|:-----|
| **冪等性** | ✅ | DI 登録は `TryAddScoped` で冪等。ブロックは `IsVKBlockRegistered` で二重登録防止 |
| **可観測性** | ✅ | 全 Store に `[LoggerMessage]` SG ベースの構造化ログ。`[VKBlockDiagnostics]` による ActivitySource 紐付け |
| **テナント分離** | ✅ | 全主要エンティティが `IVKTenantScoped` を実装。EF Core Global Filter で自動フィルタリング |
| **監査証跡** | ✅ | `IVKAuditable` / `IVKFullAuditable` で CreatedAt/UpdatedAt/DeletedAt を自動管理 |
| **論理削除** | ✅ | Directive / Knowledge / Pattern / Persona エンティティが `IVKFullAuditable` (= `IsDeleted` + `DeletedAt`) |

### 7. VK.Blocks 準拠度 (Deep)

| 項目 | 評価 | 詳細 |
|:-----|:----:|:-----|
| **境界防御 (VKGuard)** | ✅ | コンストラクタ全フィールド + メソッド引数で一貫して使用 |
| **非確定的 API (CS.06)** | ✅ | `DateTime.UtcNow` / `Guid.NewGuid()` の直接使用なし |
| **Result パターン (CS.01)** | ✅ | 全メソッドが `VKResult<T>` 返却。Error 定数 (`VKPersistenceErrors`) 使用 |
| **DI & モジュール化 (AP.02)** | ✅ | `TryAddScoped` のみ使用。`RegisterBlockCustom` による SG フック |
| **DDD & EF Core 連携** | ✅ | `[VKPersistEntity]` / `[VKPersistKey]` / `[VKPersistIndex]` / `[VKPersistColumn]` を一貫使用 |
| **Visibility (AP.03)** | ✅ | Public: Entity (VK接頭辞, ルートNS) / Internal: Store, Diagnostics (深いNS, VK接頭辞なし) |

### 深度論理 & 状態遷移審査

#### 実行パス脳内推演

**成功パス** (`DirectiveStore.GetDirectivesAsync`):
1. `cancellationToken.ThrowIfCancellationRequested()` → 正常
2. `VKGuard.NotNull(directiveIds)` → 正常
3. 空リストチェック → 短絡 `VKResult.Success([])` (正当な最適化)
4. `_repository.GetListAsync(...)` → EF Core クエリ実行
5. `entities.Select(e => e.ToDomain()).ToList()` → SG マッピング
6. `VKResult.Success<IReadOnlyList<VKDirectiveCharter>>(domainList)` → 呼び出し元に結果伝播 ✅

**失敗パス** (`SessionStore.UpdateSessionAsync`):
1. `VKGuard.NotNull(session)` → 正常
2. `_sessionRepository.GetTrackedFirstOrDefaultAsync(...)` → エンティティ取得
3. `existing is null` → `VKResult.Failure(VKPersistenceErrors.Repository.EntityNotFound)` → 呼び出し元に明確な失敗理由 ✅
4. DB 例外発生 → catch → `_logger.LogSaveSessionError()` → `VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed)` ✅

**状態伝播の正確性**: `ToDomain()` / `ToEntity()` / `MapOnto()` は SG 生成であり、プロパティの欠落リスクは低い。✅

#### 論理デッドエンドの探索

- **未使用の Diagnostics メソッド**: 各 `{Feature}Diagnostics` クラスには EventId 1-5 (CRUD 操作用) が宣言されているが、現在の Store 実装では EventId 6-7 (Store レベル操作) のみ使用されている。これは「将来の CRUD Store 拡張に備えた事前宣言」と解釈でき、**論理的な死コードではないが、未使用メソッドが多い状態**である。
  - 影響度: 低（SG 生成コストのみ、ランタイム影響なし）

#### 防御的逆向思考

- **潜在的リスク**: `EchoStore.GetHistoryAsync` で `.OrderBy(e => e.CreatedAt)` をインメモリで実行している。大量のエコーメッセージがある場合、全件取得後のメモリ上ソートはパフォーマンスリスクとなる。ただし、`GetListAsync` の内部実装が EF Core の `IQueryable` ベースであれば、DB 側でソートされる可能性が高く、リポジトリ API の仕様に依存する。
  - **推奨**: `GetListAsync` に `orderBy` パラメータを渡してDB側ソートを明示化する。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

_該当なし_ — 致命的な設計上の問題は検出されなかった。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[CS.08 String Boundaries]**: 以下のエンティティの `Content` プロパティに `[MaxLength]` が未指定。CS.08 は「ALL string properties MUST specify `.HasMaxLength()` explicitly」と規定しており、暗黙的な `TEXT` / `VARCHAR(MAX)` は禁止されている。
  - [VKPsycheEchoEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/VKPsycheEchoEntity.cs) — `Content` (L39)
  - [VKPsycheKnowledgeEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeEntity.cs) — `Content` (L33)
  - [VKPsychePatternEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Pattern/VKPsychePatternEntity.cs) — `Content` (L28)

  > **判断**: これらは「プロンプトテンプレート」「ナレッジコンテンツ」「チャットメッセージ」といった可変長テキストであり、意図的に長文を許容している可能性がある。ただし CS.08 準拠のためには、明示的な上限値（例: `[MaxLength(32000)]`）を設定すべきである。

- 🔒 **[パフォーマンス]**: [EchoStore.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoStore.cs) (L39) — `entities.OrderBy(e => e.CreatedAt)` がクライアントサイド実行の場合、高頻度チャットセッションで N+1 に近いメモリ消費リスクがある。リポジトリの `orderBy` パラメータの活用を推奨。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[テスト容易性]**: 全 Store は `IVKEntityReadRepository<T>` / `IVKEntityRepository<T>` / `IVKUnitOfWork` / `ILogger<T>` のインターフェース依存のみでコンストラクタインジェクションされており、単体テストで完全にモック可能。`sealed` クラスであるが `InternalsVisibleTo` が `VK.Blocks.AI.Psyche.EFCore.UnitTests` に設定済み。**テスト容易性は極めて高い**。

- ⚙️ **[疎結合性]**: EF Core への直接依存は `KnowledgeStore` の `Include()` 呼び出しのみ。それ以外は全て VK.Blocks 標準の Repository 抽象経由。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[OR.01 準拠]**: 全7フィーチャーに `[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]` 付きの `{Feature}Diagnostics` クラスが配置されている。全ログ出力が `[LoggerMessage]` Source Generator ベースの拡張メソッドを使用しており、`logger.LogXxx()` の直接呼び出しは **0 件**。

- 📡 **[構造化ログ]**: 全エラーログに `{DirectiveId}`, `{SessionId}` 等のセマンティックトークンが含まれており、TraceId との紐付けが可能。

- 📡 **[ActivitySource]**: `[VKBlockMarker]` SG により `VKBlocksPrefix + "AIPsycheEFCore"` の ActivitySource が自動生成されている。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[BB.01 Deviation — Common/ DI 構造の省略]**: BB.01 は `Common/DependencyInjection/` フォルダ構造を規定しているが、本モジュールでは `Common/` フォルダが空であり、カスタム登録ロジックはブロックマーカーの `RegisterBlockCustom` に直接記述されている。これは **非 Toggleable かつ Options なしの薄いインフラモジュール** としては合理的な判断であるが、標準からの逸脱として記録する。

- ⚠️ **[VKPsycheKnowledgeKeyEntity — TenantId 不在]**: [VKPsycheKnowledgeKeyEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeKeyEntity.cs) は `IVKTenantScoped` を実装していない。子テーブルであるため親の Knowledge エンティティの TenantId でフィルタリングされる設計と推測されるが、EF Core の Global Filter が直接適用されないリスクがある。

- ⚠️ **[VKPsycheKnowledgeKeyEntity — Id が raw Guid]**: [VKPsycheKnowledgeKeyEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeKeyEntity.cs) (L18) — Id プロパティが `Guid` 型であり、他のエンティティのように強く型付けされた ID（`VKKnowledgeKeyId` 等）ではない。CS.06 の観点から、`Guid.NewGuid()` が外部で使用される可能性がある。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **Vertical Slice の模範的実装**: 7つのドメインスライス（Directive, Echo, Knowledge, Pattern, Persona, Profile, Session）が独立したフォルダに Entity + Internal/(Store + Diagnostics) として完全に自己完結している。
2. **SG 活用による Anti-Corruption Layer**: `[VKPersistEntity]` + `FlattenBy` / `ProjectBy` 属性を活用し、ドメインモデル ↔ DB エンティティの双方向マッピングを Source Generator に委譲。手動マッピングコードゼロ。
3. **一貫した Result パターン**: 全 Store メソッドが try-catch + `VKResult<T>` + `VKPersistenceErrors` 定数の三位一体パターンを徹底している。
4. **防御的プログラミングの徹底**: `VKGuard` による境界防御がコンストラクタとメソッド入口の両方で一貫実施されている。
5. **読み取り/書き込みリポジトリの適切な分離**: 読み取り専用 Store は `IVKEntityReadRepository<T>`、書き込み可能 Store は `IVKEntityRepository<T>` + `IVKUnitOfWork` を使用しており、CQRS の原則に沿った設計。
6. **エンティティの監査インターフェース使い分け**: 論理削除が必要なエンティティ (Directive, Knowledge, Pattern, Persona) は `IVKFullAuditable`、不要なエンティティ (Echo, Profile, Session) は `IVKAuditable` と適切に使い分けている。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| 優先度 | 課題 | ルール | 対応案 |
|:------:|:-----|:------|:------|
| 🔴 | `Content` プロパティに `[MaxLength]` 未指定 (3 エンティティ) | CS.08 | `[MaxLength(32000)]` 等の明示的上限を設定。[VKPsycheEchoEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/VKPsycheEchoEntity.cs), [VKPsycheKnowledgeEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeEntity.cs), [VKPsychePatternEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Pattern/VKPsychePatternEntity.cs) |

### 2. リファクタリング提案 (Refactoring)

| 優先度 | 課題 | ルール | 対応案 |
|:------:|:-----|:------|:------|
| 🟡 | `VKPsycheKnowledgeKeyEntity` に `IVKTenantScoped` 未実装 | CS.08 | テナント分離の観点から `IVKTenantScoped` の追加を検討。または、親エンティティ経由のフィルタリングが十分であることを ADR に記録 |
| 🟡 | `VKPsycheKnowledgeKeyEntity.Id` が raw `Guid` | CS.06 | 強く型付けされた `VKKnowledgeKeyId` の導入を検討 |
| 🟡 | `EchoStore.GetHistoryAsync` のソート | CS.04 | `GetListAsync` の `orderBy` パラメータを使用して DB 側ソートを明示化 |
| ⚪ | 未使用 Diagnostics メソッド (EventId 1-5) | DL.02 | 将来の CRUD 拡張で使用予定であれば現状維持。不要であれば削除して Event ID 空間を節約 |

### 3. 推奨される学習トピック (Learning Suggestions)

- **EF Core Compiled Queries**: 高頻度クエリ（`EchoStore.GetHistoryAsync` 等）に対して `EF.CompileAsyncQuery` の適用を検討し、クエリプラン生成コストを削減する。
- **Bulk Operations**: `EchoStore.SaveHistoryAsync` が単一エコーずつ保存している。バッチ挿入のニーズがある場合、`AddRangeAsync` + 一括 `SaveChangesAsync` パターンの検討を推奨。

---

> **Phase 1: 22/22 (100%) | Phase 2: PASS | Phase 3 Score: 88/100**
