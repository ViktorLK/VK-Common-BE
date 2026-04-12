# 🏗️ アーキテクチャ監査レポート: VK.Blocks.Authorization

**監査日**: 2026-04-12  
**対象モジュール**: `/src/BuildingBlocks/Authorization`  
**監査者**: VK.Blocks Lead Architect (AI-Assisted)  
**対象ファイル数**: 62 ファイル (7 Feature Slices)

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **91 / 100**
- **対象レイヤー判定**: Infrastructure Layer — Authorization Pipeline (ASP.NET Core `IAuthorizationHandler` 拡張)
- **総評 (Executive Summary)**:

本モジュールは、VK.Blocks アーキテクチャ標準に対して **極めて高い準拠度** を達成している。Vertical Slice Architecture による Feature 分離、`IAuthorizationRequirementData` を活用した型安全な属性駆動設計、Provider-Evaluator-Handler の三層パイプライン、そして全ハンドラーに統一的に適用された `Result<T>` パターンは、エンタープライズ認可基盤として模範的な設計である。

`[LoggerMessage]` Source Generator ロギング、OpenTelemetry メトリクス統合、`TryAdd` パターンによる冪等 DI 登録、`stackalloc` / `Span<T>` による高性能 CIDR マッチングなど、非機能要件への対応も徹底されている。

一方で、いくつかの **軽微な設計上の改善余地** が確認された。以下に詳述する。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

> 該当する致命的な問題は **確認されなかった**。

レイヤー間の依存関係逆転、循環依存、深刻なパフォーマンスのボトルネックは存在しない。すべての Feature は公開インターフェースを介してのみ通信しており、Infrastructure 層への直接依存は完全に排除されている。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### ✅ 高評価ポイント

- 🔒 **CIDR マッチング**: `InternalNetworkAuthorizationHandler.IsInCidr()` は `stackalloc byte[16]` + `Span<T>` を使用し、ヒープアロケーション完全回避を実現 — Rule 4 準拠
- 🔒 **SuperAdmin バイパス**: 全ハンドラーに `user.IsSuperAdmin(options)` として集約拡張メソッドで統一適用 — セキュリティ一貫性確保
- 🔒 **テナント分離**: `StrictTenantIsolation` フラグで SuperAdmin のテナント横断バイパスを厳密制御

### ⚠️ 軽微な指摘事項

- ⚠️ **`PermissionHandler.HasPermissionsAsync` 内の `ToList()` 呼び出し**:
    - 箇所: [`PermissionHandler.cs:50`](/src/BuildingBlocks/Authorization/Features/Permissions/PermissionHandler.cs)
    - `permissions.ToList()` により毎回新しい `List<string>` がヒープに確保される。入力が `ImmutableArray<string>` であるケースでは、直接イテレーションが可能であり、不要なアロケーションを回避できる。
    - 影響度: **低** — パーミッション数は通常少数であり、実運用上のボトルネックにはならないが、ホットパス最適化の原則に沿えばリファクタリングが望ましい。

- ⚠️ **`PermissionHandler` に未使用の `ILogger` コンストラクタパラメータ名**:
    - コンストラクタで `ILogger<PermissionHandler> logger` を受け取っているが、`IPermissionProvider` をループ内で呼び出す設計のため、Provider 呼び出しがループ内で I/O を発生させる可能性がある（現在のデフォルト実装はクレーム読み取りのみだが、カスタム Provider ではDB アクセスが想定される）。
    - 影響度: **低** — `IPermissionProvider` のインターフェース契約で `ValueTask` を使用しており、同期完了が一般的なケースでは問題ない。

- ⚠️ **`DynamicRequirement` の `Operator` プロパティが `string` 型**:
    - 箇所: [`DynamicRequirement.cs:24`](/src/BuildingBlocks/Authorization/Features/DynamicPolicies/DynamicRequirement.cs)
    - 演算子を `string` で持つため、タイプミスが実行時まで検出されない。`DynamicPoliciesConstants` で定数化されているが、`enum` 型であれば **コンパイル時安全性** が向上する。
    - ただし、`DynamicAuthorizeAttribute` のコンストラクタが `string operator` を受け取る設計上、Attribute 引数の制約（enum はサポートされるが）を考慮する必要がある。
    - 影響度: **低** — `switch` 式の `_` パターンで `InvalidOperator` エラーが返されるため、ランタイム安全性は確保されている。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ✅ 高評価ポイント

- ⚙️ **全依存関係がインターフェース経由**: `IPermissionProvider`, `IRoleProvider`, `IUserTenantProvider`, `IRankProvider`, `IIpAddressProvider`, `IWorkingHoursProvider`, `IDynamicPolicyProvider`, `IDynamicPolicyEvaluator` — すべての外部依存が抽象化されている
- ⚙️ **`TimeProvider` 注入**: `WorkingHoursAuthorizationHandler` で `TimeProvider` を注入しており、時間依存テストの完全な制御が可能
- ⚙️ **Primary Constructor パターン**: 全ハンドラーが C# 12 の Primary Constructor を活用し、ボイラープレートを削減
- ⚙️ **Evaluator インターフェース**: 各ハンドラーが同時に `IXxxEvaluator` を実装しており、プログラマティックな認可チェック API としてもテスト可能
- ⚙️ **`InternalsVisibleTo`**: `.csproj` で `VK.Blocks.Authorization.UnitTests` へのアクセスが許可されており、Internal クラスのテストが可能

### ⚠️ 改善余地

- ⚙️ **`AuthorizationExtensions.ApplyResult` メソッドのテスト**:
    - `AuthorizationHandlerContext` は `sealed` ではないが、`AuthorizationFailureReason` の検証にはリフレクションが必要になる場合がある。
    - 現在のテストアプローチとして、各ハンドラーの統合テストでカバーされているため、実質的な問題ではない。

- ⚙️ **`InternalNetworkAuthorizationHandler.IsInCidr` が `private static`**:
    - CIDR マッチングロジックは独立してテスト可能にすることが理想的。`internal static` への昇格、または専用の `CidrMatcher` ユーティリティクラスへの抽出を検討しても良い。
    - 影響度: **低** — ハンドラーの統合テスト経由でカバー可能。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### ✅ 包括的な実装

| 項目                    | 準拠状況 | 詳細                                                                                    |
| ----------------------- | :------: | --------------------------------------------------------------------------------------- |
| `[LoggerMessage]` SG    |    ✅    | 全7Feature で `XxxLog.cs` として定義。`internal static partial class` 準拠              |
| 構造化テンプレート      |    ✅    | `"{UserId}"`, `"{PolicyName}"`, `"{Permission}"` 等のプレースホルダー一貫使用           |
| `[VKBlockDiagnostics]`  |    ✅    | `AuthorizationDiagnostics.cs` に `[VKBlockDiagnostics("VK.Blocks.Authorization")]` 定義 |
| Counter (Decisions)     |    ✅    | `authorization.decisions` — `policy` / `decision` タグ付き                              |
| Counter (Failures)      |    ✅    | `authorization.failure.reasons` — `policy` / `error_code` タグ付き                      |
| Histogram (Duration)    |    ✅    | `authorization.evaluation.duration` — ms 単位、OTel Semantic Conventions 準拠           |
| `ConfigureAwait(false)` |    ✅    | 全ハンドラー/Evaluator/Provider の `await` 呼び出しに徹底適用                           |
| `Result<T>` パターン    |    ✅    | 全ハンドラーが `Result<bool>` を返却。エラーは `AuthorizationErrors` 定数経由           |
| `Stopwatch` 計測        |    ✅    | `RecordEvaluation` 拡張メソッドによる統一的な計測・記録                                 |

### ⚠️ 軽微な指摘

- 📡 **`TenantAuthorizationHandler` で `RecordEvaluation` が SuperAdmin バイパス時にスキップされている**:
    - 箇所: [`TenantAuthorizationHandler.cs:55-59`](/src/BuildingBlocks/Authorization/Features/TenantIsolation/TenantAuthorizationHandler.cs)
    - SuperAdmin バイパス時は `Stopwatch` が開始されず、メトリクスが記録されない。運用監視の観点から、バイパスも `"Bypassed"` としてカウントすることが望ましい。
    - 同様のパターンが `InternalNetworkAuthorizationHandler`, `WorkingHoursAuthorizationHandler`, `MinimumRankAuthorizationHandler` にも見られる。
    - 影響度: **低** — バイパスであるため、正確な決定メトリクスへの影響は実質的に軽微。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### Rule 12 (Modern C# Semantics) 準拠チェック

| 規約                        | 準拠 | 備考                                                                   |
| --------------------------- | :--: | ---------------------------------------------------------------------- |
| `sealed class`              |  ✅  | 全ハンドラー・Provider・Validator に `sealed` 適用                     |
| `sealed record`             |  ✅  | 全 Requirement / DTO に `sealed record` 使用                           |
| `required` keyword          |  ✅  | `DynamicRequirement`, `Permission`, `EndpointAuthorizationInfo` に適用 |
| Pattern Matching            |  ✅  | `DefaultDynamicPolicyEvaluator` で `switch` 式を活用                   |
| `is null` / `is not null`   |  ✅  | 全体的に一貫使用                                                       |
| Collection Expressions `[]` |  ✅  | `_providers = [.. permissionProviders]` 等で使用                       |

### Rule 13 (Service Registration Pattern) 準拠チェック

| 規約                            | 準拠 | 備考                                                                             |
| ------------------------------- | :--: | -------------------------------------------------------------------------------- |
| Block マーカー型                |  ✅  | `AuthorizationBlock` が `sealed class` で定義                                    |
| Check-Self / Check-Prerequisite |  ✅  | `IsVKBlockRegistered<AuthorizationBlock>()` + `IsVKBlockRegistered<CoreBlock>()` |
| TryAdd パターン                 |  ✅  | 全 Registration クラスで `TryAddScoped` / `TryAddEnumerable` を使用              |
| Mark-Self                       |  ✅  | `services.AddVKBlockMarker<AuthorizationBlock>()` が最終ステップ                 |
| `AddVKBlockOptions<T>`          |  ✅  | `VKAuthorizationOptions` を Eager-bind + ValidateOnStart                         |
| Validator 登録                  |  ✅  | `TryAddEnumerableSingleton` で `VKAuthorizationOptionsValidator` を登録          |

### Rule 14 (Structural Organization) 準拠チェック

| 規約                  | 準拠 | 備考                                                                                                                      |
| --------------------- | :--: | ------------------------------------------------------------------------------------------------------------------------- |
| Feature-Driven Folder |  ✅  | 7つの Feature フォルダ (DynamicPolicies, InternalNetwork, MinimumRank, Permissions, Roles, TenantIsolation, WorkingHours) |
| 定数の可視性階層      |  ✅  | `internal static class XxxConstants` がFeature 内、`public static class` が `Common/` に配置                              |
| 1ファイル1型          |  ✅  | 全ファイルが単一型を宣言                                                                                                  |
| Internal 分離         |  ✅  | Default Provider / Constants / Log が `Internal/` サブフォルダに配置                                                      |

### ⚠️ 指摘事項

- ⚠️ **`InternalNetworkConstants` の可視性**:
    - 箇所: [`InternalNetworkConstants.cs`](/src/BuildingBlocks/Authorization/Features/InternalNetwork/Internal/InternalNetworkConstants.cs)
    - `public static class` として宣言されているが、`Internal/` フォルダ内に配置されている。`VKAuthorizationOptions.cs` から参照されるため `public` が必要だが、このクラスは `internal` にして、デフォルト値を `VKAuthorizationOptions` 自体の初期化子として直接定義する選択肢もある。
    - 影響度: **極めて低** — API 面への影響なし。

- ⚠️ **`WorkingHoursConstants` の可視性**:
    - 同様に `public static class` だが `Internal/` フォルダ内。`VKAuthorizationOptions` から参照されるため `public` が必要。
    - 影響度: **極めて低**。

- ⚠️ **`AuthorizationBlockExtensions.cs` 末尾の余分な空行**:
    - 箇所: [`AuthorizationBlockExtensions.cs:99-103`](/src/BuildingBlocks/Authorization/DependencyInjection/AuthorizationBlockExtensions.cs)
    - ファイル末尾に4行の不要な空行がある。
    - 影響度: **極めて低** — スタイルのみ。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### 🏆 卓越した設計判断

1. **`IAuthorizationRequirementData` の活用**: .NET 8+ の型安全属性 API を採用し、文字列パーシングを完全排除。`AuthorizePermissionAttribute`, `AuthorizeRolesAttribute`, `DynamicAuthorizeAttribute` が `GetRequirements()` で型安全な Requirement を返却する設計は極めて先進的。

2. **Provider-Evaluator-Handler 三層分離**: 各 Feature が以下の一貫した責務分離を維持:
    - **Provider**: データ取得（Claims / DB / 外部 API）
    - **Evaluator**: ビジネスロジック評価
    - **Handler**: ASP.NET Core パイプラインとの統合
    - これにより、ロジックの差し替え・テスト・再利用が最大限に容易化されている。

3. **`IVKAuthorizationRequirement` 契約**: `DefaultError` プロパティにより、各 Requirement が「失敗時のコンテキスト」を自己完結的に保持。`Result<T>` パターンとの連携が自然かつ堅牢。

4. **統一的な `ApplyResult` 拡張メソッド**: `AuthorizationExtensions.ApplyResult()` により、全ハンドラーで `Result<bool>` → `context.Succeed/Fail` のマッピングが完全に DRY 化。

5. **Feature 自己登録パターン**: 各 Feature が `XxxRegistration.cs` (internal static class) を持ち、`AuthorizationBlockExtensions` から呼び出される Vertical Slice 自律登録パターン。

6. **`PermissionsRegistration` の `TryAddEnumerable`**: `IPermissionProvider` を複数登録可能にし（Claims / Database / 外部 API）、OR ロジックでの横断評価を自然にサポート。

7. **Source Generator メタデータ基盤**: `[GeneratePermissions]`, `[GenerateRankAuthorize]`, `[GeneratePolicy]` による宣言的コード生成基盤が整備されており、ボイラープレートの自動生成と権限メタデータの静的検証が可能。

8. **`PermissionStoreExtensions.SyncIfChangedAsync`**: ハッシュベースの差分同期により、パーミッションの不必要な DB 書き込みを抑制する冪等同期パターン。

9. **IPv4-mapped IPv6 の自動正規化**: `InternalNetworkAuthorizationHandler` が `ip.IsIPv4MappedToIPv6` を検出して `MapToIPv4()` に変換する防御的実装。

10. **夜間シフト対応**: `WorkingHoursAuthorizationHandler` が `Start > End` のケース（日付跨ぎ）を正しくハンドリング。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

> 致命的な課題は検出されなかった。以下は優先度「中」の改善項目。

| #   | 項目                                                     | 影響度 | 対象ファイル                                                              |
| --- | -------------------------------------------------------- | ------ | ------------------------------------------------------------------------- |
| 1   | SuperAdmin バイパス時にもメトリクスを記録する            | 中     | 全ハンドラー (`Tenant`, `InternalNetwork`, `WorkingHours`, `MinimumRank`) |
| 2   | `AuthorizationBlockExtensions.cs` 末尾の不要な空行を削除 | 低     | `DependencyInjection/AuthorizationBlockExtensions.cs`                     |

### 2. リファクタリング提案 (Refactoring)

| #   | 提案                                                                                                                   | 根拠                           | 複雑度                   |
| --- | ---------------------------------------------------------------------------------------------------------------------- | ------------------------------ | ------------------------ |
| A   | `PermissionHandler.HasPermissionsAsync` の `ToList()` を除去し、`IReadOnlyList<string>` に変換または直接イテレーション | GC 圧力削減、ホットパス最適化  | 低                       |
| B   | `InternalNetworkAuthorizationHandler.IsInCidr` を `internal static` ユーティリティクラスに抽出                         | テスト容易性向上、再利用性確保 | 低                       |
| C   | `DynamicRequirement.Operator` を `string` → `enum` に変更検討                                                          | コンパイル時安全性向上         | 中（破壊的変更の可能性） |
| D   | `InternalNetworkConstants` / `WorkingHoursConstants` を `Internal/` から移動、またはアクセス修飾子の整合性を確認       | 規約上の一貫性                 | 低                       |

### 3. 推奨される学習トピック (Learning Suggestions)

| トピック                                           | 理由                                                                                                                                                        |
| -------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Policy Composition Pattern**                     | 複数ポリシーの AND/OR 合成を宣言的に記述できるフレームワーク設計。現在の `FinancialDataWrite` ポリシーのように手動で `AddRequirements` を連結する方式の改善 |
| **Distributed Authorization Caching**              | `IPermissionProvider` のカスタム実装で DB アクセスが発生する場合、`IDistributedCache` + Polly による resiliency-aware キャッシュ戦略                        |
| **ASP.NET Core 10 `IAuthorizationPolicyProvider`** | 動的ポリシー生成の拡張。`DynamicAuthorizeAttribute` と連携し、より柔軟なポリシー解決メカニズムの構築                                                        |
| **Benchmarking with BenchmarkDotNet**              | `IsInCidr` などの高性能メソッドに対するマイクロベンチマークの導入。`stackalloc` vs `ArrayPool` の定量的比較                                                 |

---

## 📋 監査チェックリスト (VK.Blocks Rules Compliance)

| Rule | 項目                      | 準拠 | 備考                                                                              |
| ---- | ------------------------- | :--: | --------------------------------------------------------------------------------- |
| R1   | Result\<T\> Pattern       |  ✅  | 全ハンドラー/Evaluator が `Result<bool>` を返却。`AuthorizationErrors` 定数使用   |
| R2   | Layer Dependencies        |  ✅  | Infrastructure 依存なし。`Microsoft.AspNetCore.App` FrameworkReference のみ       |
| R3   | Async / CancellationToken |  ✅  | 全 I/O 操作に `async/await` + `CancellationToken`。`ValueTask` をホットパスで使用 |
| R3   | ConfigureAwait(false)     |  ✅  | 全 `await` 呼び出しに `.ConfigureAwait(false)` 適用                               |
| R4   | Performance               |  ✅  | `stackalloc` / `Span<T>` 使用。ループ内 DB クエリなし                             |
| R6   | `[LoggerMessage]` SG      |  ✅  | 全 Feature に `internal static partial class XxxLog` 定義                         |
| R6   | Diagnostics               |  ✅  | `[VKBlockDiagnostics]` + Counter/Histogram + OTel 準拠タグ                        |
| R7   | Security / TenantId       |  ✅  | `SameTenantRequirement` + `StrictTenantIsolation`                                 |
| R9   | Testability               |  ✅  | 全依存関係がインターフェース経由。`InternalsVisibleTo` 設定済み                   |
| R10  | No Placeholders           |  ✅  | TODO / 未実装コードなし                                                           |
| R12  | Modern C#                 |  ✅  | `sealed`, `record`, `required`, Pattern Matching, Collection Expressions          |
| R13  | Service Registration      |  ✅  | Check-Self → Check-Prerequisite → TryAdd → Mark-Self パターン完全準拠             |
| R14  | Structural Organization   |  ✅  | Feature-Driven 7 Slices + Internal 分離 + 定数可視性階層                          |

---

> **結論**: `VK.Blocks.Authorization` は、VK.Blocks アーキテクチャ基準に高いレベルで準拠しており、エンタープライズ認可基盤として **本番運用に適した成熟度** を達成している。上記の軽微な改善点を段階的に対応することで、スコア 95+ への到達が見込まれる。
