# 🏛️ アーキテクチャ監査レポート: AI.Psyche.EFCore

**監査日**: 2026-08-23  
**対象モジュール**: `src/BuildingBlocks/AI.Psyche.EFCore`  
**監査者**: VK.Blocks Lead Architect (Automated)  
**Audit Load**: `[BB.01:ctx, AP.03:ctx, BB.02:ctx, BB.03:ctx, BB.07:ctx, DL.01:ctx, CS.01:L3, AP.01:L3, AP.02:L3, BB.04:L3, BB.05:L3, CS.02:L3] | Status: Verified ✅`

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **78/100**
- **Fast Audit スコア**: 27/30 (90%)
- **対象レイヤー判定**: Infrastructure Layer / EFCore Persistence Implementation
- **総評 (Executive Summary)**: AI.Psyche.EFCore は、AI.Psyche ドメインの EF Core 永続化層として、高い水準で VK.Blocks アーキテクチャに準拠している。全ファイルの `sealed` / `internal` 分離が徹底され、`VKGuard` + `Result<T>` + `ConfigureAwait(false)` + `[LoggerMessage]` SG の四本柱が全ドメインスライスで確認できる。しかし、**2 件の Type A (🔴) 違反**（`DateTimeOffset.UtcNow` 直呼び [CS.06]、`Guid.NewGuid()` 直呼び [CS.06]）、**1 件の `null!` 使用**（[AP.01]）、および **`GetListAsync(e => true)` による無制限全件取得**（[CS.04] ページネーション欠如）が発見された。特に `DateTimeOffset.UtcNow` はテスト不能な時刻依存を生み、`Guid.NewGuid()` は非決定的 ID 生成として修正が必要である。

---

## ⚡ Fast Audit: AI.Psyche.EFCore

**Score**: 27/30 (90%)

### 📁 Structure (BB.01)
- ✅ **S-01**: Feature Slice 構造 — `Directive/`, `Echo/`, `Knowledge/`, `Pattern/`, `Persona/`, `Profile/`, `Session/` が存在。`Common/` も配置済み。
- ✅ **S-02**: `Common/Internal/` ディレクトリが存在。各 Feature にも `Internal/` が配備。
- ⚠️ **S-03**: `Diagnostics/` ディレクトリ不在。`[VKBlockDiagnostics]` 属性も未使用。Feature-level Logs は存在するが、Block-level Diagnostics は欠如。
- ⚠️ **S-04**: `Diagnostics/Internal/` 不在（S-03 に連動）。
- ✅ **S-05**: Marker ファイル `VKPsycheEFCoreBlock.cs` がモジュールルートに存在。
- ✅ **S-06**: Options 不要（`Toggleable = false` のインフラブロック）。Options ファイルは N/A。

### 🏷️ Marker (BB.02)
- ✅ **M-01**: `[VKBlockMarker]` 属性が使用されている。
- ✅ **M-02**: Legacy `IVKBlockMarker` 手動実装なし。
- ✅ **M-03**: `public sealed partial class VKAIPsycheEFCoreBlock` — 正しい宣言。
- ✅ **M-04**: `Dependencies = [typeof(VKAIPsycheBlock), typeof(VKPersistenceEFCoreBlock)]` が宣言済み。

### 🔌 DI Registration (BB.03, AP.02)
- ✅ **D-01**: `IsVKBlockRegistered` — SG 自動生成（`[VKBlockMarker]` 使用のため）。
- ✅ **D-02**: `AddVKBlockMarker` — SG 自動生成。
- ✅ **D-03**: `AddVKBlockOptions` — N/A（Options 不要のブロック）。
- ✅ **D-04**: `TryAdd` パターンが 16 箇所で確認。`TryAddEnumerable` + `TryAddScoped` のみ使用。
- ✅ **D-05**: 直接 `services.AddSingleton/Scoped/Transient` の呼び出しなし。
- ✅ **D-06**: `BlockRegistration.Register` — SG 自動生成。
- ✅ **D-07**: `AddVK.*Block` — SG 自動生成。

### ⚙️ Options (BB.05)
- N/A — このブロックは `Toggleable = false` のインフラ実装ブロックであり、独自の Options を持たない。

### 🔍 Implementation Patterns
- ✅ **I-01**: Sealed 比率 — `public sealed`: 9 件、`public class` (unsealed): 0 件。`internal sealed class`: 23 件。**100% sealed**。
- ✅ **I-02**: `VKGuard.` 使用 — 50+ 箇所で確認。全コンストラクタ境界 + メソッド引数境界で適用。
- ✅ **I-03**: `ConfigureAwait(false)` — 全 `await` 呼び出しに対して適用済み。比率: 50+/50+ (100%)。
- ✅ **I-04**: `[LoggerMessage]` SG — 7 Feature 全てに `*Logs.cs` (internal static partial class) が存在。
- ✅ **I-05**: 直接 `logger.LogXxx()` 呼び出しなし。
- ❌ **I-06**: `[VKBlockDiagnostics]` 属性が未使用。Block-level のメトリクス/トレース基盤が欠如。
- ✅ **I-07**: `Result<T>` / `VKResult` パターン — 全メソッドで使用。エラー定数 `VKPersistenceErrors` を一貫して使用。
- ✅ **I-08**: `DateTime.UtcNow` / `DateTime.Now` 使用なし。
- ❌ **I-09**: `Guid.NewGuid()` が [KnowledgeRepository.cs:L128](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/Internal/KnowledgeRepository.cs) で検出。**🔴 CS.06 違反**。
- ✅ **I-10**: `JsonSerializer.Serialize/Deserialize` 直呼びなし（`IVKJsonSerializer` を使用）。
- ✅ **I-11**: EF Core 依存は Infrastructure 層 (`Internal/` 配下) に限定。Application logic への汚染なし。

### 📛 Naming & Visibility (AP.03)
- ✅ **N-01**: 全 public 型が `VK` / `IVK` プレフィックスを使用。
- ✅ **N-02**: `Internal/` 配下の全型が `internal sealed class` で宣言。
- ✅ **N-03**: Namespace がフォルダパスと一致。

### 📊 Summary Table

| Category | Tier | ✅ | ❌ | ⚠️ |
| :--- | :--- | :--- | :--- | :--- |
| Structure | 🟡 | 4 | 0 | 2 |
| Marker | 🔴 | 4 | 0 | 0 |
| DI Registration | 🔴 | 7 | 0 | 0 |
| Options | 🟡 | N/A | N/A | N/A |
| Implementation | 🔴 | 9 | 1 | 0 |
| Naming | 🟡 | 3 | 0 | 0 |
| **合計** | | **27** | **1** | **2** |

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[CS.06 — 非確定的 API 直呼び]**: [[KnowledgeRepository.cs:L128](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/Internal/KnowledgeRepository.cs)] — `Guid.NewGuid()` がハードコードされている。`IVKGuidGenerator` を経由すべき。テストで ID を固定できず、非決定的な振る舞いを生む。

    ```csharp
    // 🔴 VIOLATION
    Id = key.Id == Guid.Empty ? Guid.NewGuid() : key.Id,
    // ✅ SHOULD BE
    Id = key.Id == Guid.Empty ? _guidGenerator.NewGuid() : key.Id,
    ```

- ❌ **[CS.06 — 非確定的 API 直呼び]**: [[EchoRepository.cs:L134](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs)] — `DateTimeOffset.UtcNow` がハードコードされている。`TimeProvider` を経由すべき。テスト時に時刻を固定できない。

    ```csharp
    // 🔴 VIOLATION
    existing.UpdatedAt = DateTimeOffset.UtcNow;
    // ✅ SHOULD BE
    existing.UpdatedAt = _timeProvider.GetUtcNow();
    ```

- ❌ **[AP.01 — `null!` 使用禁止]**: [[VKPsycheKnowledgeEntity.cs:L46](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeEntity.cs)] — `Knowledge` ナビゲーションプロパティに `null!` が使用されている。EF Core ナビゲーションの制約上やむを得ないが、代替パターン（`required` または SG 付きの partial）を検討すべき。

    ```csharp
    // 🔴 VIOLATION
    public VKPsycheKnowledgeEntity Knowledge { get; set; } = null!;
    ```

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[CS.04 — ページネーション欠如]**: 複数のリポジトリで `GetListAsync(e => true)` による **全件取得** が実装されている（[SessionRepository.cs:L57](/src/BuildingBlocks/AI.Psyche.EFCore/Session/Internal/SessionRepository.cs), [ProfileRepository.cs:L58](/src/BuildingBlocks/AI.Psyche.EFCore/Profile/Internal/ProfileRepository.cs), [PersonaRepository.cs:L58](/src/BuildingBlocks/AI.Psyche.EFCore/Persona/Internal/PersonaRepository.cs), [PatternRepository.cs:L58](/src/BuildingBlocks/AI.Psyche.EFCore/Pattern/Internal/PatternRepository.cs), [KnowledgeRepository.cs:L63](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/Internal/KnowledgeRepository.cs)）。データ量増加時に深刻なパフォーマンス劣化・OOM リスクあり。

- 🔒 **[CS.04 — メモリ内ソート]**: [EchoRepository.cs:L81-L88](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs) の `GetHistoryAsync` は全 Echo を取得してからメモリ内で `OrderByDescending + Take(limit)` を実行。DB 側の `OrderBy` + `Take` に変換すべき。

- 🔒 **[知識状態の二重取得リスク]**: [SessionRepository.UpdateAsync](/src/BuildingBlocks/AI.Psyche.EFCore/Session/Internal/SessionRepository.cs) は `GetFirstOrDefaultAsync` (NoTracking) で既存エンティティを取得しているが、[SessionStore.UpdateSessionAsync](/src/BuildingBlocks/AI.Psyche.EFCore/Session/Internal/SessionStore.cs) は `GetTrackedFirstOrDefaultAsync` を正しく使用。Repository 層と Store 層で tracked/untracked の扱いに微妙な差異がある。

- 🔒 **[ClearHistoryAsync のループ削除]**: [EchoRepository.cs:L208-L211](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs) — `foreach` ループで 1 件ずつ `DeleteAsync` を呼び出しており、N+1 的なパフォーマンス問題を抱えている。`ExecuteDeleteAsync` やバッチ削除の検討が必要。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[テスト容易性 — 高]**: 全実装クラスが `internal sealed` かつインターフェース経由で DI 注入されているため、単体テストが容易。`InternalsVisibleTo` が UnitTests プロジェクトに設定済み。`DynamicProxyGenAssembly2` も設定されており、Moq/NSubstitute 互換。

- ⚙️ **[テスト容易性 — リスク]**: 上記の `Guid.NewGuid()` と `DateTimeOffset.UtcNow` はテスト非決定性の原因となり、テスト容易性を損なう。`IVKGuidGenerator` / `TimeProvider` への置換が必要。

- ⚙️ **[Factory パターン]**: `IVKPsycheModelFactory` によるドメインオブジェクト生成の抽象化が全 Store で一貫して使用されており、ドメインモデルの構築ロジックをモック可能にしている。優れた設計。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[LoggerMessage SG — 完全準拠]**: 全 7 Feature に `*Logs.cs` (internal static partial class) が配置され、`[LoggerMessage]` Source Generator を使用。構造化ログテンプレートが一貫して使用されている。

- 📡 **[VKBlockDiagnostics — 未設定]**: `[VKBlockDiagnostics]` 属性が未使用であり、Block-level の `ActivitySource` / `Meter` による構造化トレース・メトリクスが欠如。運用監視の観点から、最低限のカウンター (CRUD 操作数、エラー率) の追加が望ましい。

- 📡 **[Error Reporting — 優秀]**: 全例外が `catch` され、`Result.Failure` + `VKPersistenceErrors` 定数に変換されている。例外がフレームワーク外に漏洩するリスクは極めて低い。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[AP.03 — 型分離違反]**: [VKPsycheKnowledgeEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeEntity.cs) に `VKPsycheKnowledgeEntity` と `VKPsycheKnowledgeKeyEntity` の **2 つの public class** が同一ファイルに宣言されている。AP.03 Type Segregation ルール (One File, One Type) に違反。

- ⚠️ **[EchoRepository.UpdateAsync — ログ関数名不一致]**: [EchoRepository.cs:L142](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs) — `UpdateAsync` のエラーハンドリングで `LogCreateEchoEntityError` が呼ばれている。正しくは `LogUpdateEchoEntityError` であるべき。ログ分析時の誤解を生む。

- ⚠️ **[Entity namespace フラット化]**: 全 Entity クラスが `namespace VK.Blocks.AI.Psyche.EFCore;` というフラットなルート名前空間に宣言されている。Feature Slice 単位の名前空間（例: `VK.Blocks.AI.Psyche.EFCore.Session`）ではなく、`VK.Blocks.AI.Psyche.EFCore` に集約されている。Public API として意図的であれば許容されるが、Entity 数の増加に伴い名前衝突のリスクがある。

- ⚠️ **[Repository 内の手動プロパティマッピング]**: 全 Repository の `UpdateAsync` メソッドで、既存エンティティへの手動プロパティコピー（20+ 行の `existing.Xxx = entity.Xxx`）が繰り返されている。`IVKMapper` や AutoMapper 等の標準マッピング機構の導入、または EF Core の `Entry.CurrentValues.SetValues()` の使用で保守性を改善可能。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **Vertical Slice の徹底**: 7 Feature (`Directive`, `Echo`, `Knowledge`, `Pattern`, `Persona`, `Profile`, `Session`) がそれぞれ完全な垂直スライス（Entity + Interface + Internal/Repository + Internal/Store + Internal/Config + Internal/Logs）を構成。BB.01 に忠実。

2. **TryAdd 完全準拠**: DI 登録が `TryAddScoped` / `TryAddEnumerable` のみで構成。AP.02 の冪等性要件を完全に満たす。

3. **Result<T> パターンの徹底**: 例外を投げず、全メソッドが `VKResult<T>` / `VKResult` を返却。Infrastructure 例外は `catch` → `Result.Failure` → 構造化エラー定数へ変換。CS.01 に忠実。

4. **ConfigureAwait(false) 100% 準拠**: 全 `await` 呼び出しに `.ConfigureAwait(false)` が適用済み。CS.03 ライブラリ要件を完全に満たす。

5. **IVKJsonSerializer 活用**: `System.Text.Json.JsonSerializer` を直接使用せず、`IVKJsonSerializer` 抽象を一貫して使用。CS.06 準拠。

6. **VKGuard の徹底活用**: 全コンストラクタ引数 + メソッド境界引数に `VKGuard.NotNull` / `VKGuard.NotDefault` が適用。AP.01 の防御的プログラミング要件を高水準で満たす。

7. **Model Contributor パターン**: `PsycheModelContributor` が `IVKModelCreatingContributor` + `IVKModelConventionContributor` を実装し、EF Core の `ModelBuilder` / `ModelConfigurationBuilder` を自動設定。Zero-Config DI 登録の好例。

8. **Value Converter の Strongly-Typed ID 対応**: 7 種の Strongly-Typed ID (`VKEchoId`, `VKSessionId`, etc.) に対する `ValueConverter` が `PsycheModelContributor` 内の private sealed class として一箇所に集約。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action) — 🔴 Type A 違反の修正

| 優先度 | 対象ファイル | 修正内容 |
|:---:|:---|:---|
| P0 | [EchoRepository.cs:L134](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs) | `DateTimeOffset.UtcNow` → `TimeProvider.GetUtcNow()` に置換。コンストラクタに `TimeProvider` を DI 注入。 |
| P0 | [KnowledgeRepository.cs:L128](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/Internal/KnowledgeRepository.cs) | `Guid.NewGuid()` → `IVKGuidGenerator.NewGuid()` に置換。コンストラクタに `IVKGuidGenerator` を DI 注入。 |
| P0 | [VKPsycheKnowledgeEntity.cs:L46](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeEntity.cs) | `= null!` を除去。EF Core ナビゲーションには `= default!` または late-init パターンの検討。 |

### 2. リファクタリング提案 (Refactoring)

| 優先度 | 対象 | 修正内容 |
|:---:|:---|:---|
| P1 | 全 Repository の `GetListAsync(e => true)` | ページネーションパラメータ (`int skip, int take`) を追加し、DB 側で `Skip/Take` を適用。最低限 `Take(1000)` 等の上限ガードを設置。 |
| P1 | [EchoRepository.GetHistoryAsync](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs) | メモリ内ソートを DB クエリ (`OrderByDescending + Take`) に変換。 |
| P1 | [EchoRepository.ClearHistoryAsync](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs) | `foreach` ループ削除を `ExecuteDeleteAsync` に置換。 |
| P1 | [VKPsycheKnowledgeEntity.cs](/src/BuildingBlocks/AI.Psyche.EFCore/Knowledge/VKPsycheKnowledgeEntity.cs) | `VKPsycheKnowledgeKeyEntity` を独立ファイル `VKPsycheKnowledgeKeyEntity.cs` に分離 (AP.03 Type Segregation)。 |
| P2 | [EchoRepository.cs:L142](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs) | `LogCreateEchoEntityError` → `LogUpdateEchoEntityError` に修正（ログ関数名不一致）。 |
| P2 | 全 Repository の手動プロパティマッピング | `Entry.CurrentValues.SetValues()` または mapper 導入の検討。 |
| P3 | Block-level Diagnostics | `[VKBlockDiagnostics]` 属性の追加と `VKAIPsycheEFCoreDiagnosticsConstants.cs` の作成。 |

### 3. 推奨される学習トピック (Learning Suggestions)

- **EF Core Bulk Operations**: `ExecuteDeleteAsync` / `ExecuteUpdateAsync` による高パフォーマンスバッチ操作。
- **IVKGuidGenerator / TimeProvider パターン**: VK.Blocks Core の非決定的 API 置換メカニズムの理解と適用。
- **VK.Blocks Diagnostics Blueprint (BB.04)**: `[VKBlockDiagnostics]` による構造化メトリクスの実装手法。

---

## 🏷️ 深度逻辑与状态演进审查 (Deep Logic & State Evolution Audit)

### 執行路径の脳内推演 (Mental Execution)

**成功パス** (`EchoStore.SaveHistoryAsync`):
1. `VKGuard.NotNull(trace)` — 入力検証 ✅
2. `new VKPsycheEchoEntity { ... }` — Domain → Entity 変換 ✅
3. `_repository.AddAsync` → `_unitOfWork.SaveChangesAsync` — 永続化 ✅
4. `return VKResult.Success()` — 成功結果 ✅
→ **正常に完了。状態は DB に永続化される。**

**失敗パス** (`SessionStore.UpdateSessionAsync` — Entity 不在):
1. `VKGuard.NotNull(session)` — 入力検証 ✅
2. `_sessionRepository.GetTrackedFirstOrDefaultAsync` — Entity 取得 (tracked)
3. `existing is null` → `return VKResult.Failure(EntityNotFound)` ✅
→ **失敗が Result.Failure として正しく伝播される。**

### 逻辑死胡同 (Dead Ends)

- **[EchoRepository.UpdateAsync:L125](/src/BuildingBlocks/AI.Psyche.EFCore/Echo/Internal/EchoRepository.cs)**: フィルタ条件 `entity.SessionId.IsEmpty || e.SessionId == entity.SessionId` — `SessionId.IsEmpty` の場合、SessionId の一致検証がスキップされ、**異なるセッションの Echo が誤って更新される可能性がある**。SessionId が Empty であること自体が異常状態であり、ここでの `IsEmpty` チェックは防御的に見えるが、実際には意図しないデータ書き換えのリスクを生む。

### 防御性逆向思考 (Destructive Thinking)

**漏洞: EchoRepository.UpdateAsync の SessionId バイパス**

`entity.SessionId.IsEmpty` が true の場合、`EchoId` のみで検索されるため、**テナント A のユーザーがテナント B の Echo を更新できる可能性がある**（EF Core の TenantId Global Filter が正しく機能していることを前提とすれば安全だが、Filter が無効化された場合は cross-tenant データ汚染のリスク）。

**推奨**: `SessionId` が Empty の場合は即座に `Result.Failure` を返すべき。

---

Audit: 🚩 [CS.06] `DateTimeOffset.UtcNow` (EchoRepository.cs:L134) + `Guid.NewGuid()` (KnowledgeRepository.cs:L128)  
Audit: 🚩 [AP.01] `null!` 使用 (VKPsycheKnowledgeEntity.cs:L46)  
Audit: 🚩 [AP.03] 型分離違反 — 2 public class in 1 file (VKPsycheKnowledgeEntity.cs)
