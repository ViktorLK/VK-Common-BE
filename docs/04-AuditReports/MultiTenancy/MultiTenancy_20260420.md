# アーキテクチャ監査レポート: MultiTenancy

**対象モジュール**: `VK.Blocks.MultiTenancy`
**監査日**: 2026-04-20
**対象レイヤー**: Infrastructure / Cross-Cutting Concern (Middleware + Ambient Context)
**バージョン**: 0.9.0

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **82 / 100**
- **対象レイヤー判定**: Cross-Cutting Infrastructure Layer（マルチテナント解決ミドルウェア + コンテキスト管理）
- **総評 (Executive Summary)**:
  MultiTenancy モジュールは、Strategy Pattern / Pipeline Pattern を中核としたプラグイン型のテナント解決アーキテクチャを採用しており、全体的に高い設計品質を維持している。`IVKBlockOptions` / `IVKBlockMarker` / `[VKBlockDiagnostics]` といった VK.Blocks フレームワーク標準への準拠度は極めて高い。一方で、`TenantSecurityMiddleware` のハードコードされたロール名、`OverrideTenantResolver` のマジックストリング (`context.Items` キー)、および一部のクラスにおけるミドルウェアの直接 `ILogger` 呼び出し（`[LoggerMessage]` SG 未使用）が改善すべき主要課題として特定された。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **マジックストリング: `context.Items` キー**: [[OverrideTenantResolver.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/Resolvers/OverrideTenantResolver.cs:L1123)] / [[TenantSecurityMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Internal/TenantSecurityMiddleware.cs:L1447)]
  `context.Items["VK_MultiTenancy_Source"]` および `"Override"` が両ファイルに散在しており、共有定数として抽出されていない。片方のみ変更された場合、テナント偽装検証のサイレント障害を引き起こす可能性がある。`MultiTenancyConstants` に集約すべき。

- ❌ **ハードコードされたロール名**: [[TenantSecurityMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Internal/TenantSecurityMiddleware.cs:L1464)]
  `"SuperAdmin"` がハードコードされている。コード中のコメントにも「Ideally, we'd pull this from a central constant」と記載されているが未対応。ロール名はセキュリティ要件に直結するため、`MultiTenancyOptions` に設定化するか、`MultiTenancyConstants` に定数として定義すべき重大事項。

- ❌ **`[LoggerMessage]` SG 未使用 (Rule 6 違反)**: [[TenantSecurityMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Internal/TenantSecurityMiddleware.cs:L1452-L1474)]
  `TenantSecurityMiddleware` 内で `_logger.LogWarning()` / `_logger.LogCritical()` / `_logger.LogInformation()` を直接呼び出している。VK.Blocks Rule 6 では、`[LoggerMessage]` Source Generator を使用した `internal static partial class` 経由のログ記録が**必須**であり、標準ロガーメソッドの直接呼び出しは**禁止**されている。`MultiTenancyLog` クラスにエントリを追加し、Source Generator パターンに移行する必要がある。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **テナント ID バリデーション**: [[TenantResolutionMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/TenantResolutionMiddleware.cs:L1276-L1280)]
  `IsValidTenantId` は `LINQ.All()` を使用しているが、`ReadOnlySpan<char>` ベースの文字走査に置き換えることで、ゼロアロケーションかつ高パフォーマンスな検証となる。ホットパス（全リクエスト通過）であるため、Rule 4 (Performance / Span) の観点から推奨。

- 🔒 **テナント偽装のセキュリティ境界**: [[OverrideTenantResolver.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/Resolvers/OverrideTenantResolver.cs)]
  `OverrideTenantResolver` は常に DI に登録される (`RegisterResolvers` メソッド内で無条件 `TryAddEnumerable`)。`EnableImpersonation = false` の場合はランタイムで `ResolverSkipped` を返すが、不要なサービスが DI コンテナに存在すること自体がセキュリティの表面積を広げる。`EnableImpersonation` が `false` の場合は登録自体をスキップすることが推奨される。

- 🔒 **`TenantInfo` の一時的な ID = Name マッピング**: [[TenantResolutionMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/TenantResolutionMiddleware.cs:L1247)]
  `new TenantInfo(tenantId, tenantId)` で ID と Name を同一値で設定している。コメントに `// Using ID as Name for now` とある。`ITenantStore` を経由して正規のテナント名を解決するか、ドメインモデルの完全性を保証するための戦略が必要。

- 🔒 **`TenantResolutionMiddleware` のコンストラクタインジェクション**: [[TenantResolutionMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/TenantResolutionMiddleware.cs:L1215-L1219)]
  ASP.NET Core のミドルウェアはシングルトンとして動作するため、コンストラクタで Scoped サービス (`ITenantResolutionPipeline`) を直接インジェクションすると、**Captive Dependency** 問題が発生する。`InvokeAsync` のパラメータインジェクションとして受け取るのが正しいパターン。`TenantContext` は既に `InvokeAsync` で正しくメソッドインジェクションされているが、`ITenantResolutionPipeline` は Scoped 登録であるにもかかわらずコンストラクタインジェクションされており、重大なランタイムバグの原因となり得る。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **高いテスト容易性**: 各リゾルバーは `ITenantResolver` インターフェースを実装し、`ITenantResolutionPipeline` 経由で呼び出されるため、個別のユニットテストが容易。
- ⚙️ **`ITenantStore` の分離**: テナントストアはインターフェース抽象化されており、DB / キャッシュ / 外部 API 等のバックエンドからテナント解決ロジックを完全に分離している。
- ⚙️ **`TenantContextAccessor` の薄いラッパー**: `ITenantContext` に委譲するだけの薄いアクセサクラスは、テスト時のモック差し替えが容易。ただし、`TenantContextAccessor` は `ITenantContext` との機能重複が見られ、その存在意義の再評価が必要（YAGNI 観点）。
- ⚙️ **`InternalsVisibleTo` 設定済**: `.csproj` に `VK.Blocks.MultiTenancy.UnitTests` への公開設定があり、`internal` クラスのテスト基盤は整備済み。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **`[VKBlockDiagnostics]` 準拠**: [[MultiTenancyDiagnostics.cs](/src/BuildingBlocks/MultiTenancy/Diagnostics/MultiTenancyDiagnostics.cs)] に `[VKBlockDiagnostics<MultiTenancyBlock>]` 属性が正しく適用され、`ActivitySource` / `Meter` が Source Generator 経由で生成されている。
- 📡 **メトリクス定数の定義**: [[MultiTenancyDiagnosticsConstants.cs](/src/BuildingBlocks/MultiTenancy/Diagnostics/MultiTenancyDiagnosticsConstants.cs)] にて全メトリクス名とタグキーが定数化されており、OpenTelemetry Semantic Conventions に準拠。
- 📡 **`[LoggerMessage]` SG 部分準拠**: [[MultiTenancyLog.cs](/src/BuildingBlocks/MultiTenancy/Diagnostics/MultiTenancyLog.cs)] で主要なログエントリは SG パターンを使用しているが、`TenantSecurityMiddleware` で直接 `ILogger` メソッド呼び出しが残存。
- 📡 **RFC 7807 ProblemDetails**: [[TenantResolutionMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/TenantResolutionMiddleware.cs:L1294-L1306)] でエラーレスポンスが RFC 7807 準拠の `ProblemDetails` 形式で返却されている。`traceId` も含まれており、運用監視要件を十分に満たす。
- 📡 **重複タグ定数**: `MultiTenancyDiagnosticsConstants` 内に `TagTenantId` と `TenantIdTagName` が同じ値 `"tenant_id"` で二重定義されている。`// Legacy tags if still needed` コメント付きだが、DRY 原則違反。レガシータグの削除を推奨。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **`MultiTenancyOptions.EnabledResolvers` の型**: [[MultiTenancyOptions.cs](/src/BuildingBlocks/MultiTenancy/MultiTenancyOptions.cs:L1698)]
  `List<TenantResolverType>` が `public` プロパティとして公開されているが、外部からの不正な操作を防ぐため `IReadOnlyList<TenantResolverType>` に変更し、`init` アクセサを使用すべき（Rule 12 - Immutable Data）。`sealed record` であっても、`List<T>` プロパティはミュータブルなコレクション参照を持つ。

- ⚠️ **`EnableImpersonation` に `required` キーワード未使用**: [[MultiTenancyOptions.cs](/src/BuildingBlocks/MultiTenancy/MultiTenancyOptions.cs:L1692)]
  `EnforceTenancy` には `required` が付いているが、`EnableImpersonation` には付いていない。Rule 12 では非 nullable プロパティに `required` を使用することが推奨される。`bool` 型にデフォルト値がある場合でも一貫性のため統一すべき。

- ⚠️ **`ExceptionHandling` への不明な依存**: [[VK.Blocks.MultiTenancy.csproj](/src/BuildingBlocks/MultiTenancy/VK.Blocks.MultiTenancy.csproj:L1921)]
  `ExceptionHandling` プロジェクトへの `ProjectReference` が設定されているが、コードベース全体を走査した限り、`ExceptionHandling` からの型やサービスを直接使用している箇所が確認できない。不要な依存は Layer Dependencies (Rule 2) の観点から除去を推奨。

- ⚠️ **`MultiTenancyConstants.Errors` と `MultiTenancyErrors` の責務重複**: 
  `MultiTenancyConstants.Errors` にはエラーコード文字列 (`MissingTenantCode`) とメッセージ文字列 (`MissingTenantMessage`) が定義されているが、`MultiTenancyErrors` には `Error` オブジェクトが定義されている。`MultiTenancyConstants.Errors` のエラー関連定数は `MultiTenancyErrors` の `Error` オブジェクトから参照されておらず、**未使用の可能性が高い**。DRY 原則違反。

- ⚠️ **`TenantResolutionMiddleware` のアクセス修飾子**: [[TenantResolutionMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/TenantResolutionMiddleware.cs:L1215)]
  `TenantResolutionMiddleware` が `public` として宣言されている。ミドルウェアは `UseMultiTenancy()` 拡張メソッド経由でのみ使用されるため、`internal` に変更し、カプセル化を強化すべき。同様に、`TenantResolutionPipeline` も `public` だが、`ITenantResolutionPipeline` インターフェース経由で使用されるため `internal` が適切。

- ⚠️ **Resolvers の `public` アクセス修飾子**: 全リゾルバー (`HeaderTenantResolver`, `ClaimsTenantResolver`, `DomainTenantResolver`, `QueryStringTenantResolver`, `OverrideTenantResolver`) が `public sealed class` として宣言されているが、DI 登録は `MultiTenancyBlockExtensions` 内で行われ、`ITenantResolver` インターフェース経由で利用される。`internal sealed class` で十分であり、不必要な API サーフェスの縮小が推奨される。

---

## ✅ 評価ポイント (Highlights / Good Practices)

- ✅ **Service Registration Pattern 完全準拠 (Rule 13)**: `IsVKBlockRegistered<MultiTenancyBlock>()` → `EnsureVKCoreBlockRegistered` → `AddVKBlockOptions` → `AddVKBlockMarker` → 実際の登録、という「Check-Self, Check-Prerequisite, Actual Registration, Mark-Self」パターンが `AddVKMultiTenancy` / `AddMultiTenancyInternal` の双方で正確に実装されている。
- ✅ **TryAdd パターン完全準拠 (Rule 13)**: `AddMultiTenancyCore` 内の全サービス登録が `TryAddScoped` / `TryAddEnumerable` を使用しており、冪等性が保証されている。
- ✅ **`IVKBlockOptions` 準拠 (Rule 15)**: `MultiTenancyOptions` / `TenantResolutionOptions` ともに `IVKBlockOptions` を実装し、`SectionName` が `VKBlocksConstants.VKBlocksConfigPrefix` を使用して定義されている。
- ✅ **Result Pattern 準拠 (Rule 1)**: 全リゾルバー、パイプライン、Feature Evaluator が `Result<T>` を返却。`MultiTenancyErrors` に構造化された `Error` オブジェクトが定義されており、`Result.Failure("raw string")` の使用はゼロ。
- ✅ **ValueTask 活用 (Rule 3)**: 同期完了が一般的なリゾルバー (`Header`, `Claims`, `QueryString`) で `ValueTask<Result<string>>` が適切に使用されている。
- ✅ **ConfigureAwait(false) (Rule 3)**: 全 `await` 呼び出しに `.ConfigureAwait(false)` が付与されている。
- ✅ **Span ベースのドメイン解析 (Rule 4)**: `DomainTenantResolver.ExtractTenantSegment` で `ReadOnlySpan<char>` を活用したゼロアロケーション文字列解析が実装されている。
- ✅ **Sealed Record + Required 適用 (Rule 12)**: `TenantInfo` / `MultiTenancyOptions` / `TenantResolutionOptions` が `sealed record` として定義され、`required` キーワードが適用されている。
- ✅ **Feature-Driven Folder Layout (Rule 14)**: `Features/Context`, `Features/Entitlements`, `Features/Resolution` という名詞ベースのドメイン駆動フォルダ構造が採用されている。
- ✅ **Builder Pattern**: `IVKMultiTenancyBuilder` を返却する Fluent API 設計により、モジュール消費者が段階的にサービスを構築可能。
- ✅ **環境制限付きリゾルバー**: `QueryStringTenantResolver` が `IHostEnvironment.IsDevelopment()` チェックを含み、本番環境での不正なテナント注入を防止。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| # | 問題 | 影響 | 対象ファイル |
|---|------|------|-------------|
| 1 | **Captive Dependency**: `TenantResolutionMiddleware` のコンストラクタで Scoped サービス (`ITenantResolutionPipeline`) をインジェクション | ランタイムバグ: シングルトンミドルウェアが初回リクエストの Scoped インスタンスをキャプチャし、全後続リクエストで同じインスタンスを再利用 | [TenantResolutionMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/TenantResolutionMiddleware.cs) |
| 2 | **`[LoggerMessage]` SG への移行**: `TenantSecurityMiddleware` の直接 `ILogger` 呼び出し | Rule 6 違反、構造化ログの欠損 | [TenantSecurityMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Internal/TenantSecurityMiddleware.cs) |
| 3 | **マジックストリング排除**: `context.Items` キーとロール名の定数化 | サイレント障害リスク、セキュリティ要件の不透明化 | [OverrideTenantResolver.cs](/src/BuildingBlocks/MultiTenancy/Features/Resolution/Resolvers/OverrideTenantResolver.cs), [TenantSecurityMiddleware.cs](/src/BuildingBlocks/MultiTenancy/Internal/TenantSecurityMiddleware.cs) |

### 2. リファクタリング提案 (Refactoring)

| # | 提案 | 根拠 |
|---|------|------|
| 1 | **アクセス修飾子の引き締め**: `TenantResolutionMiddleware`, `TenantResolutionPipeline`, 全 Resolver を `internal` に変更 | カプセル化の強化、不要な API サーフェスの削減 |
| 2 | **`IsValidTenantId` の Span 化**: `tenantId.All(...)` を `ReadOnlySpan<char>` ループに変更 | ホットパスでの GC プレッシャー低減 (Rule 4) |
| 3 | **`EnabledResolvers` の不変化**: `List<TenantResolverType>` → `IReadOnlyList<TenantResolverType>` + `init` アクセサ | Rule 12 - Immutable Data 準拠 |
| 4 | **`OverrideTenantResolver` の条件付き登録**: `EnableImpersonation = false` 時に DI 登録自体をスキップ | セキュリティ表面積の最小化 |
| 5 | **`MultiTenancyConstants.Errors` の精査**: 未使用エラー定数 (`MissingTenantCode` / `MissingTenantMessage` / `InvalidTenantImplementationCode`) の削除、または `MultiTenancyErrors` との統合 | DRY 原則、コードベースの簡素化 |
| 6 | **レガシー診断タグの削除**: `MultiTenancyDiagnosticsConstants` 内の `TenantIdTagName` / `TenantNameTagName` | DRY 原則、`TagTenantId` との重複 |
| 7 | **`ExceptionHandling` 依存の検証と除去**: `.csproj` の `ProjectReference` が実際に使用されているか確認し、不要であれば除去 | Rule 2 - Layer Dependencies |

### 3. 推奨される学習トピック (Learning Suggestions)

| # | トピック | 関連性 |
|---|---------|--------|
| 1 | **ASP.NET Core Middleware Lifetime**: シングルトン vs Scoped の Captive Dependency 問題と正しいメソッドインジェクションパターン | `TenantResolutionMiddleware` の Captive Dependency 修正 |
| 2 | **Tenant Data Isolation パターン**: Schema-per-Tenant / Database-per-Tenant の EF Core 実装 | `ITenantStore` の運用実装と `TenantInfo.ConnectionString` / `Schema` の活用 |
| 3 | **OpenTelemetry Baggage Propagation**: テナント ID をサービス間で透過的に伝播するための OTel Baggage API | マイクロサービス間のテナントコンテキスト伝播 |

---

## 📋 監査スコア詳細

| 評価次元 | スコア | コメント |
|---------|--------|---------|
| 設計原則 (SOLID/DRY/YAGNI) | 14/20 | SRP/DIP 準拠は良好だが、DRY 違反（定数重複）と YAGNI 疑義（TenantContextAccessor）あり |
| 設計パターン | 18/20 | Strategy/Pipeline/Builder/Options パターンの適用は模範的 |
| アーキテクチャ原則 | 14/20 | カプセル化の不足（public 過多）、Captive Dependency 問題 |
| アーキテクチャ風格 | 16/20 | Vertical Slice 準拠、Feature-Driven 構造は良好 |
| VK.Blocks フレームワーク準拠 | 15/20 | Options/Marker/Diagnostics は模範的だが、Rule 6 (LoggerMessage SG) 違反あり |
| セキュリティ & 運用 | 5/10 | ハードコードロール名、Captive Dependency、マジックストリングがリスク要因 |

**総合: 82 / 100**
