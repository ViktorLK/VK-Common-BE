# 🏛️ アーキテクチャ監査レポート — Authorization BuildingBlock

> **監査日:** 2026-04-09  
> **対象モジュール:** `src/BuildingBlocks/Authorization`  
> **監査者:** VK.Blocks Architect (AI-Assisted)  
> **総ファイル数:** 54 (.cs)

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **91 / 100**
- **対象レイヤー判定**: Infrastructure Layer / Authorization Policy Handlers (Cross-cutting BuildingBlock)
- **総評 (Executive Summary)**:

Authorization BuildingBlock は、ASP.NET Core の Authorization パイプラインを VK.Blocks アーキテクチャの原則に極めて高い精度で準拠させた、成熟度の高いモジュールである。Vertical Slice アーキテクチャ、Provider/Evaluator 分離パターン、`IVKAuthorizationRequirement` による統一エラーコントラクト、Source Generator (`[LoggerMessage]`, `[VKBlockDiagnostics]`) の全面採用、そして `Result<T>` パターンの徹底的な適用が確認される。DI登録は Marker/Idempotent パターンを厳格に実装しており、各機能の拡張ポイントが Fluent Builder で一貫して提供されている。

減点要因は、軽微な設計上の不整合（`AuthorizePermissionAttribute` の `sealed` 欠如、`RoleHandler.HandleRequirementAsync` 内での `ApplyResult` 未使用パス）、およびオプション型の `required` キーワード適用に関する改善余地に限定される。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ RoleHandler.HandleRequirementAsync — ApplyResult 拡張メソッドの不統一

**ファイル:** `src/BuildingBlocks/Authorization/Features/Roles/RoleHandler.cs` (L41–L76)

`RoleHandler.HandleRequirementAsync` は、他のすべての Handler が使用する `context.ApplyResult(requirement, result, this)` パターンを使用せずに、`context.Succeed()` / `context.Fail()` を直接呼び出している。これは以下のリスクを生む:

1. **ApplyResult の三分岐ロジック**（成功 / 論理拒否 / インフラエラー）をバイパスしており、`IVKAuthorizationRequirement.DefaultError` が使用されない
2. 保守時に、`ApplyResult` のロジックを変更しても `RoleHandler` に反映されない
3. 全 Handlers の一貫性が破壊されている

**影響度**: 中 — 機能的には正しく動作するが、アーキテクチャの統一性と保守性を損なう

### ❌ AuthorizePermissionAttribute — `sealed` 未宣言

**ファイル:** `src/BuildingBlocks/Authorization/Features/Permissions/AuthorizePermissionAttribute.cs` (L11)

```csharp
public class AuthorizePermissionAttribute(string permission) : AuthorizeAttribute, IAuthorizationRequirementData
```

Rule 15（Sealed by Default）に違反。`AuthorizeRolesAttribute` と `DynamicAuthorizeAttribute` は `sealed` 宣言されているが、`AuthorizePermissionAttribute` のみ `public class` のままである。意図しない拡張や仮想ディスパッチのオーバーヘッドが発生する。

**影響度**: 低 — セキュリティリスクは小さいが、コーディング規約の一貫性を損なう

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 InternalNetworkAuthorizationHandler.HandleRequirementAsync — 不必要な `.ToArray()` 割り当て

**ファイル:** `src/BuildingBlocks/Authorization/Features/InternalNetwork/InternalNetworkAuthorizationHandler.cs` (L33)

```csharp
var result = await IsInternalNetworkAsync(allowedCidrs: requirement.AllowedCidrs.ToArray())
```

`requirement.AllowedCidrs` は `IReadOnlyList<string>` 型であるにもかかわらず、毎回 `.ToArray()` で新しい配列を割り当てている。`IsInternalNetworkAsync` のシグネチャが `string[]?` を受け取るため発生するが、`IReadOnlyList<string>?` または `ReadOnlySpan<string>` への変更で回避可能。リクエストごとのヒープアロケーションである。

### 🔒 InternalNetworkAuthorizationHandler — ハードコードされた `userId = "System"`

**ファイル:** `src/BuildingBlocks/Authorization/Features/InternalNetwork/InternalNetworkAuthorizationHandler.cs` (L46)

```csharp
var userId = "System";
```

`HandleRequirementAsync` から呼び出される際は `context.User` が利用可能であるが、`IsInternalNetworkAsync` にはユーザー情報が渡されないため、ログ出力のユーザー ID がハードコードで `"System"` となる。運用時のトラブルシューティングで、どのユーザーのリクエストが拒否されたか特定困難。

### 🔒 DefaultDynamicPolicyEvaluator.EvaluateAsync — case-sensitive 文字列比較

**ファイル:** `src/BuildingBlocks/Authorization/Features/DynamicPolicies/DefaultDynamicPolicyEvaluator.cs` (L35)

```csharp
DynamicPoliciesConstants.OperatorEquals => Result.Success(Equals(claimValue, requirement.Value?.ToString())),
```

`object.Equals` を使用しているが、Claim の値は一般的に case-insensitive 比較が期待される場面が多い。また `OperatorContains` のパスでも `string.Contains` が Ordinal 比較（.NET の既定動作）であり、文化依存の比較問題は回避されているが、明示的な `StringComparison` の指定が推奨される。

### 🔒 VKAuthorizationOptions — `InternalCidrs` が `List<string>` (ミュータブル)

**ファイル:** `src/BuildingBlocks/Authorization/DependencyInjection/VKAuthorizationOptions.cs` (L92)

```csharp
public List<string> InternalCidrs { get; set; } = [.. InternalNetworkConstants.DefaultPrivateCidrs];
```

構成オプションのプロパティがミュータブルな `List<string>` として公開されている。バインド後に外部から改変可能であり、セキュリティ構成の不変性が保証されない。`IReadOnlyList<string>` またはバインド後のフリーズパターンが望ましい。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ 優秀な Provider/Evaluator 分離

全 7 機能において **Provider（データ取得） ↔ Evaluator（ロジック判定） ↔ AuthorizationHandler（ASP.NET Core 統合）** の三層分離が一貫して適用されている。各 Provider・Evaluator はインターフェースで抽象化され、DI コンテナ経由で解決される。

| Feature         | Provider Interface       | Evaluator Interface         | Handler                               |
| --------------- | ------------------------ | --------------------------- | ------------------------------------- |
| Permissions     | `IPermissionProvider`    | `IPermissionEvaluator`      | `PermissionHandler`                   |
| Roles           | `IRoleProvider`          | `IRoleEvaluator`            | `RoleHandler`                         |
| TenantIsolation | `IUserTenantProvider`    | `ITenantEvaluator`          | `TenantAuthorizationHandler`          |
| WorkingHours    | `IWorkingHoursProvider`  | `IWorkingHoursEvaluator`    | `WorkingHoursAuthorizationHandler`    |
| MinimumRank     | `IRankProvider`          | `IMinimumRankEvaluator`     | `MinimumRankAuthorizationHandler`     |
| InternalNetwork | `IIpAddressProvider`     | `IInternalNetworkEvaluator` | `InternalNetworkAuthorizationHandler` |
| DynamicPolicies | `IDynamicPolicyProvider` | `IDynamicPolicyEvaluator`   | `DynamicRequirementHandler`           |

この設計により:

- Handler の ASP.NET Core 依存を排除したロジックのみの単体テストが `IXxxEvaluator` 経由で可能
- Provider を Mock することで外部依存なしのテストが可能
- `TimeProvider` の DI 注入により時間依存テストも制御可能

### ⚙️ IPermissionProvider のデフォルト実装未提供

`IRoleProvider` → `DefaultRoleProvider`、`IRankProvider` → `DefaultRankProvider` 等は全てデフォルト実装が提供されているが、`IPermissionProvider` のデフォルト実装が存在しない。`AddVKAuthorizationBlock` の DI 登録でも `IPermissionProvider` の `TryAdd` が欠如している（L77–L79 参照）。これにより、Permission 機能を利用する場合に消費者側で Provider を登録しないと `PermissionHandler` で `IPermissionProvider` の解決に失敗する。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 100% Source Generator ロギング準拠

全 7 機能において `[LoggerMessage]` Source Generator ベースのロギングクラスが各フィーチャーフォルダ内に配置されている:

| Feature         | Log Class               |
| --------------- | ----------------------- |
| DynamicPolicies | `DynamicPoliciesLog.cs` |
| InternalNetwork | `InternalNetworkLog.cs` |
| MinimumRank     | `MinimumRankLog.cs`     |
| Permissions     | `PermissionsLog.cs`     |
| Roles           | `RolesLog.cs`           |
| TenantIsolation | `TenantIsolationLog.cs` |
| WorkingHours    | `WorkingHoursLog.cs`    |

すべてが `internal static partial class` で `ILogger` 拡張メソッドパターンを使用しており、Rule 6 に完全準拠。テンプレートは構造化プレースホルダ (`{UserId}`, `{PolicyName}`) を使用し、文字列補間は一切使用されていない。

### 📡 `[VKBlockDiagnostics]` による Meter/ActivitySource 自動生成

`AuthorizationDiagnostics` クラスが `[VKBlockDiagnostics("VK.Blocks.Authorization")]` を使用し、3種の計器を定義:

- `Counter<long>` — authorization.decisions / authorization.failure.reasons
- `Histogram<double>` — authorization.evaluation.duration

定数は全て `AuthorizationDiagnosticsConstants` に集約されており、OpenTelemetry Semantic Conventions に準拠。

### 📡 RecordEvaluation / ApplyResult 拡張による統一テレメトリ

`AuthorizationExtensions.RecordEvaluation` と `ApplyResult` により、各 Handler のテレメトリ記録と ASP.NET Core Context へのマッピングが一元化。ボイラープレートの排除とメトリクス取得の一貫性が確保されている。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ VKAuthorizationOptions — `required` キーワード未使用

**ファイル:** `src/BuildingBlocks/Authorization/DependencyInjection/VKAuthorizationOptions.cs`

Rule 15 では「Use `required` keyword for all non-nullable properties in record or DTO types」を義務付けている。`VKAuthorizationOptions` は `sealed class` であり `record` ではないが、`RoleClaimType`, `PermissionClaimType`, `TenantClaimType`, `RankClaimType` 等の非 nullable 文字列プロパティにはデフォルト値が設定されているため、Options バインディングの文脈では問題ないが、直接インスタンス化する場合の安全性が低い。

### ⚠️ AuthorizationBuilderExtensions.AddVKPolicies — LINQ による ServiceDescriptor 検索

**ファイル:** `src/BuildingBlocks/Authorization/DependencyInjection/AuthorizationBuilderExtensions.cs` (L43–L46)

```csharp
var options = builder.Services
    .Where(d => d.ServiceType == typeof(VKAuthorizationOptions))
    .Select(d => d.ImplementationInstance as VKAuthorizationOptions)
    .FirstOrDefault();
```

`IServiceCollection` をリニアスキャンして `VKAuthorizationOptions` のインスタンスを検索している。これは:

1. `AddVKBlockOptions` パターンでは `IOptions<T>` 経由でバインドされるため、`ImplementationInstance` が `null` になる可能性がある
2. `ServiceProvider` を構築していない段階での Options 解決は脆弱
3. **正しいアプローチ**: Options バインディング段階で `IConfigureOptions<T>` パターンを使用するか、パラメータとして明示的に受け取る

### ⚠️ DynamicPoliciesConstants — `internal` 宣言だが `public const` メンバー

**ファイル:** `src/BuildingBlocks/Authorization/Features/DynamicPolicies/DynamicPoliciesConstants.cs`

クラスが `internal static class` であるにもかかわらず、メンバーが `public const` で宣言されている。コンパイルは通るが、意図的に内部公開に制限する場合は `internal const` が望ましい（一貫性向上のため）。

### ⚠️ InternalNetworkAuthorizationHandler.HandleRequirementAsync — 認証チェック欠如

**ファイル:** `src/BuildingBlocks/Authorization/Features/InternalNetwork/InternalNetworkAuthorizationHandler.cs`

他の Handler（Permission, Roles, MinimumRank, TenantIsolation）は `HandleRequirementAsync` の冒頭で `context.User.Identity?.IsAuthenticated != true` をチェックして早期リターンしているが、`InternalNetworkAuthorizationHandler` と `DynamicRequirementHandler` はこのチェックが欠如。IP ベースの評価であるため機能上は許容されうるが、一貫性の観点からは統一が望ましい。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### ✅ 1. Vertical Slice Architecture の模範的実装

7機能（DynamicPolicies / InternalNetwork / MinimumRank / Permissions / Roles / TenantIsolation / WorkingHours）が完全に独立したフィーチャーフォルダに分離されている。各フォルダは以下の構成を持つ:

- **Requirement** (`sealed record` + `IVKAuthorizationRequirement`)
- **Handler** (`sealed class` + `AuthorizationHandler<TRequirement>` + `IXxxEvaluator`)
- **Provider Interface** + **Default Provider**
- **Evaluator Interface**
- **Log Class** (`[LoggerMessage]` SG)
- **Constants** (必要な場合)

### ✅ 2. IVKAuthorizationRequirement — 統一エラーコントラクト

`IVKAuthorizationRequirement` が `IAuthorizationRequirement` を拡張し、`Error DefaultError` プロパティを追加することで、Result パターンと ASP.NET Core Authorization パイプラインの橋渡しを実現。全 7 Requirement が `AuthorizationErrors` の定義済みエラー定数を返す。

### ✅ 3. DI 登録の完全な Rule 16/17 準拠

- `AuthorizationBlock` マーカー型宣言 ✅
- 冪等性チェック (`IsVKBlockRegistered`) ✅
- `TryAdd` パターン完全適用 ✅
- `AddVKBlockMarker` による自己登録 ✅
- `AddVKBlockOptions` によるオプション登録 ✅
- `IValidateOptions<T>` による起動時バリデーション ✅

### ✅ 4. VKAuthorizationOptionsValidator — 包括的構成バリデーション

CIDR フォーマット検証（IPv4/IPv6 対応）、Working Hours の論理検証、必須 Claim Type の空白チェックを網羅。`Enabled = false` 時の早期リターンも適切に実装。

### ✅ 5. Fluent Builder による全プロバイダーのオーバーライド可能性

`AuthorizationBuilderExtensions` で `WithXxxProvider<T>` / `WithXxxEvaluator<T>` の Fluent API が全 7 機能 + 追加コンポーネント（`IPermissionStore`, `ISyncStateStore`, `TimeProvider`）に対して提供されており、消費者側の拡張性が最大化されている。

### ✅ 6. Source Generator 統合 (権限管理の自動化)

`[GeneratePermissions]` / `[GenerateRankAuthorize]` 属性により、権限定義とランクポリシーのソースジェネレーション基盤が確立。`IAuthorizationRequirementData` の使用により、文字列ベースのポリシー名解析を完全に排除。

### ✅ 7. ConfigureAwait(false) の徹底

全 `await` 呼び出しで `.ConfigureAwait(false)` が使用されており、ライブラリコードの同期コンテキストデッドロックリスクが排除されている (Rule 3 完全準拠)。

### ✅ 8. ValueTask の一貫採用

全 Provider / Evaluator インターフェースが `ValueTask<T>` を返却。キャッシュヒットやクレームベースの同期解決パスでのアロケーション最小化を実現。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 🔴 最優先対応 (Immediate Action)

| #   | 対象                                 | 課題                             | 推奨対応                                                                   |
| --- | ------------------------------------ | -------------------------------- | -------------------------------------------------------------------------- |
| 1   | `RoleHandler.HandleRequirementAsync` | `ApplyResult` パターン未使用     | `context.ApplyResult(requirement, finalResult, this)` に統一               |
| 2   | `AuthorizePermissionAttribute`       | `sealed` 欠如                    | `public sealed class` に変更                                               |
| 3   | `IPermissionProvider`                | デフォルト実装未提供 / DI 未登録 | Claim ベースの `DefaultPermissionProvider` を追加し、`TryAddScoped` で登録 |

### 2. 🟡 リファクタリング提案 (Refactoring)

| #   | 対象                                  | 課題                                                | 推奨対応                                                                                                             |
| --- | ------------------------------------- | --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| 4   | `InternalNetworkAuthorizationHandler` | `.ToArray()` アロケーション / ハードコード `userId` | `IsInternalNetworkAsync` のシグネチャに `ClaimsPrincipal?` を追加。CIDR パラメータを `IReadOnlyList<string>?` に変更 |
| 5   | `AddVKPolicies`                       | `IServiceCollection` LINQ スキャン                  | Options パラメータの明示的受け渡しに変更。または `IOptions<T>` の遅延解決パターンに切り替え                          |
| 6   | `VKAuthorizationOptions`              | `List<string> InternalCidrs` のミュータブル性       | バインド後の `IReadOnlyList<string>` 化、または Post-Configure によるフリーズ                                        |
| 7   | `DefaultDynamicPolicyEvaluator`       | 文字列比較の暗黙的 Ordinal                          | `StringComparison.OrdinalIgnoreCase` の明示的指定を検討                                                              |
| 8   | `DynamicPoliciesConstants`            | `internal class` / `public const` の不整合          | `internal const` に統一                                                                                              |
| 9   | 全 Handler                            | 認証チェックの一貫性                                | `InternalNetworkAuthorizationHandler` / `DynamicRequirementHandler` にも `IsAuthenticated` チェックを追加検討        |

### 3. 📘 推奨される学習トピック (Learning Suggestions)

| #   | トピック                                       | 理由                                                                                                                                        |
| --- | ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **ASP.NET Core IAuthorizationRequirementData** | .NET 8+ での宣言的ポリシー発行パターンを既に採用しているが、`IAuthorizationPolicyProvider` との統合パスの最適化余地を探る                   |
| 2   | **ReadOnlySpan\<T\> for CIDR matching**        | `InternalNetworkAuthorizationHandler.IsInCidr` の `cidr.Split('/')` を `ReadOnlySpan<char>` ベースに変更し、ゼロアロケーション化を検討      |
| 3   | **Options Validation with FluentValidation**   | `VKAuthorizationOptionsValidator` の `IValidateOptions<T>` 実装は堅牢だが、将来的な複雑性増大に備え FluentValidation との統合パターンを検討 |

---

> **結論**: Authorization BuildingBlock は VK.Blocks のアーキテクチャ原則を極めて高い水準で実装しており、特に Vertical Slice 設計、Provider/Evaluator 分離パターン、Source Generator 活用、および DI 登録の冪等性において模範的なコードベースである。発見された課題は全て軽微であり、即時に対応可能な範囲に収まっている。
