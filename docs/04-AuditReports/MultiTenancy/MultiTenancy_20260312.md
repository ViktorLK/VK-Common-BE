# アーキテクチャ監査レポート — VK.Blocks.MultiTenancy

**監査日**: 2026-03-12  
**対象モジュール**: `VK.Blocks.MultiTenancy`  
**対象ファイル数**: 21 ファイル (.cs)  
**監査者**: VK.Blocks Lead Architect (AI)

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **80 / 100**
- **対象レイヤー判定**: Cross-Cutting Concern / Multi-Tenant Infrastructure
- **総評 (Executive Summary)**:

本モジュールは、マルチテナント SaaS アプリケーションにおけるテナント識別・解決・伝播の基盤を提供する。Chain of Responsibility パターンによるリゾルバパイプライン、Strategy パターンによる複数解決戦略（Header / Claims / Domain / QueryString）、RFC 7807 準拠のエラーレスポンス、定数の一元管理（`MultiTenancyConstants`）など、VK.Blocks の設計哲学に高い準拠度を示す。

前回監査（2026-03-10）からの改善状況を踏まえ、主に以下の点で改善余地がある：

1. **`TenantResolutionResult` が独自の Result 型** — `VK.Blocks.Core.Results.Result<T>` と類似の構造を持ちながら独立して実装されており、モジュール間の一貫性が損なわれる。
2. **DI 登録における Options パターンの逸脱** — 他モジュールと同様、`new` による手動構築が行われている。
3. **`TenantContextAccessor` が Service Locator パターンを使用** — `GetService(typeof(ITenantContext))` による手動解決。
4. **`TenantResolutionMiddleware` での匿名型 ProblemDetails** — `VK.Blocks.ExceptionHandling` の `VKProblemDetails` を使用すべき。
5. **`MultiTenancyOptions` 内の `TenantResolverType` enum** — Rule 14（Type Segregation: One File, One Type）違反。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ CS-01: `TenantResolutionResult` と `Result<T>` の重複設計

**該当箇所**: [TenantResolutionResult.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Abstractions/Contracts/TenantResolutionResult.cs)

`TenantResolutionResult` は `IsSuccess` / `TenantId` / `Error` を持つ独自の成功・失敗パターンを実装しているが、これは `VK.Blocks.Core.Results.Result<T>` と設計目的が重複している。

**問題点**:
1. **モジュール間の一貫性の欠如**: `Result<T>` を標準としている VK.Blocks エコシステムの中で、独自の Result 型が存在する。
2. **エラー表現の非構造化**: `Error` が単なる `string?` であり、`VK.Blocks.Core.Results.Error` のような構造化エラー（コード + メッセージ + 種別）ではない。

**推奨**: `Result<string>` または `Result<TenantId>` への移行を検討すること。ただし、テナント解決は「失敗は通常のフロー」（次のリゾルバに委譲）であるため、`Result<T>` の意味論と異なる可能性があり、設計判断として ADR に記録すべき。

---

### ❌ CS-02: DI 登録における Options パターンの逸脱

**該当箇所**: [MultiTenancyServiceCollectionExtensions.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/DependencyInjection/MultiTenancyServiceCollectionExtensions.cs#L31-L41)

```csharp
var multiTenancyOptions = new MultiTenancyOptions();
configureOptions?.Invoke(multiTenancyOptions);
services.Configure<MultiTenancyOptions>(options =>
{
    options.EnforceTenancy = multiTenancyOptions.EnforceTenancy;
    options.EnabledResolvers = multiTenancyOptions.EnabledResolvers;
});

var resolutionOptions = new TenantResolutionOptions();
configureResolution?.Invoke(resolutionOptions);
services.AddSingleton(resolutionOptions);
```

**問題点**:
1. **二重構築**: `MultiTenancyOptions` が `new` で構築された後、プロパティを手動コピーして `services.Configure` に再登録している。
2. **`TenantResolutionOptions` の直接インスタンス登録**: `services.AddSingleton(resolutionOptions)` により Options パイプラインを完全にバイパスしている。`IOptions<TenantResolutionOptions>` ではなく具象型として注入されている。
3. **`PostConfigure` 非対応**: 他のモジュールやテストが `PostConfigure` で設定を上書きすることができない。

**推奨**: `services.Configure<MultiTenancyOptions>(configureOptions)` + `services.Configure<TenantResolutionOptions>(configureResolution)` で統一し、リゾルバには `IOptions<TenantResolutionOptions>` をインジェクションすること。

---

### ❌ CS-03: `MultiTenancyOptions.cs` に `TenantResolverType` enum が同居 (Rule 14 違反)

**該当箇所**: [MultiTenancyOptions.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Options/MultiTenancyOptions.cs#L29-L42)

```csharp
public sealed class MultiTenancyOptions { ... }

public enum TenantResolverType { Header, Claims, Domain, QueryString }
```

Rule 14（One File, One Type）に違反。`TenantResolverType` は外部から参照される公開型であり、独立したファイルに分離すべきである。

**推奨**: `Options/TenantResolverType.cs` に分離すること。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 NF-01: `TenantResolutionMiddleware` でのテナント ID 入力未検証

**該当箇所**: [TenantResolutionMiddleware.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Resolution/TenantResolutionMiddleware.cs#L52)

```csharp
var tenantInfo = new TenantInfo(result.TenantId!, result.TenantId!);
tenantContext.SetTenant(tenantInfo);
```

リゾルバから取得した `TenantId` に対するサニタイジング・バリデーションが行われていない。`HeaderTenantResolver` や `QueryStringTenantResolver` はユーザーが自由に値を設定できるため、異常な長さの文字列やインジェクション攻撃文字列が送信されるリスクがある。

さらに、`TenantInfo` の `Name` パラメータに `TenantId` をそのまま代入しており、テナント名の意味的な情報が失われている。

**推奨**:
1. テナント ID のフォーマットバリデーション（長さ制限、許可文字チェック）を追加すること。
2. `ITenantStore.GetByIdAsync` でテナントの実在性を確認する処理を追加するか、少なくとも設計上の意図をコメントで明文化すること。

---

### 🔒 NF-02: `QueryStringTenantResolver` の本番利用リスク

**該当箇所**: [QueryStringTenantResolver.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Resolution/Resolvers/QueryStringTenantResolver.cs)

XML コメントに "Intended for use in development environments only" と記載されているが、本番環境での無効化を保証する機構が存在しない。`MultiTenancyOptions.EnabledResolvers` から除外することは可能だが、デフォルト登録（`enabledResolvers.Count == 0` → 全登録）の場合、本番でも有効になる。

**推奨**: `QueryStringTenantResolver` に `IHostEnvironment` をインジェクションし、`Development` 環境以外では無効化するガード処理を追加すること。またはデフォルト動作から除外すること。

---

### 🔒 NF-03: `TenantResolutionMiddleware` の匿名型 ProblemDetails

**該当箇所**: [TenantResolutionMiddleware.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Resolution/TenantResolutionMiddleware.cs#L70-L77)

```csharp
var problemDetails = new
{
    type = "https://tools.ietf.org/html/rfc7807",
    title = "Tenant Resolution Failed",
    status = (int)HttpStatusCode.Unauthorized,
    detail = MultiTenancyConstants.Errors.MissingTenantMessage,
    traceId = context.TraceIdentifier
};
```

匿名型で ProblemDetails を構築しているため、以下の問題がある：

1. **`VKProblemDetails` との不一致**: `ExceptionHandling` モジュールが定義する `VKProblemDetails`（`ErrorCode`, `Timestamp` 付き）と構造が異なる。
2. **マジックストリング**: `"Tenant Resolution Failed"`, `"https://tools.ietf.org/html/rfc7807"` がリテラル。
3. **型安全性の欠如**: プロパティ名のタイポが検出されない。

**推奨**: `VKProblemDetails` を使用してレスポンスを生成すること。ただし、`ExceptionHandling` への依存関係追加が必要な場合は、`ProblemDetails`（ASP.NET Core 組み込み）を使用すること。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ TC-01: `TenantContextAccessor` の Service Locator パターン

**該当箇所**: [TenantContextAccessor.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Context/TenantContextAccessor.cs#L29-L30)

```csharp
var tenantContext = _httpContextAccessor.HttpContext?
    .RequestServices.GetService(typeof(ITenantContext)) as ITenantContext;
```

`RequestServices.GetService` による手動解決は Service Locator アンチパターンである。テスト時に `IHttpContextAccessor` のモック設定が複雑になり、`RequestServices` のモックチェーンが必要になる。

**推奨**: `TenantContextAccessor` は `ITenantContext` を直接コンストラクタインジェクションで受け取り、`IHttpContextAccessor` への依存を排除すること。Scoped ライフタイムの都合で直接注入が困難な場合は、設計上の理由を XML コメントで明文化すること。

---

### ⚙️ TC-02: `TenantResolutionPipeline` がインターフェースを持たない

**該当箇所**: [TenantResolutionPipeline.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Resolution/TenantResolutionPipeline.cs)

`TenantResolutionPipeline` は具象クラスとして `TenantResolutionMiddleware` にインジェクションされている。インターフェース (`ITenantResolutionPipeline`) が存在しないため、ミドルウェアの単体テスト時にパイプラインのモック化が困難。

**推奨**: `ITenantResolutionPipeline` インターフェースを `Abstractions` に導入すること。

---

### ⚙️ TC-03: DI 登録で `ITenantContext` と `TenantContext` の二重登録

**該当箇所**: [MultiTenancyServiceCollectionExtensions.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/DependencyInjection/MultiTenancyServiceCollectionExtensions.cs#L45-L46)

```csharp
services.TryAddScoped<ITenantContext, TenantContext>();
services.TryAddScoped<TenantContext>();
```

`ITenantContext` と `TenantContext` が別々に Scoped 登録されているため、**異なるインスタンスが解決される可能性がある**。ミドルウェアが `TenantContext` を直接受け取り `SetTenant` を呼んでも、`ITenantContext` で解決される別インスタンスには反映されない。

**推奨**: 単一インスタンスを共有するために Factory 登録パターンを使用すること：
```csharp
services.TryAddScoped<TenantContext>();
services.TryAddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
```

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 OB-01: 構造化ログの一貫した実装 ✅

`TenantResolutionMiddleware` および `TenantResolutionPipeline` では、`{TenantId}`, `{TraceId}`, `{ResolverType}`, `{Error}` などの構造化プレースホルダを使用した適切なログ出力が実装されている。Rule 6 (Observability) に準拠。

### 📡 OB-02: 個別リゾルバのログ不足

**該当箇所**: 全リゾルバ (`HeaderTenantResolver`, `ClaimsTenantResolver`, `DomainTenantResolver`, `QueryStringTenantResolver`)

個別のリゾルバに `ILogger` が注入されていない。`TenantResolutionPipeline` で `LogTrace` によるトレースログが出力されているため致命的ではないが、リゾルバ内部のデバッグ情報（例: `DomainTenantResolver` の `ExtractTenantSegment` のマッチング過程）が利用できない。

**推奨**: 重要度は低いが、`DomainTenantResolver` のような複雑なロジックを持つリゾルバには `ILogger` を追加し、トレースレベルでマッチング過程をログ出力することを検討。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ CQ-01: `TenantResolutionMiddleware` のマジックストリング

**該当箇所**: [TenantResolutionMiddleware.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Resolution/TenantResolutionMiddleware.cs#L68-L76)

| マジックストリング | 行 |
|---|---|
| `"application/problem+json"` | L68 |
| `"https://tools.ietf.org/html/rfc7807"` | L72 |
| `"Tenant Resolution Failed"` | L73 |

**推奨**: `MultiTenancyConstants` に追加するか、`VKProblemDetails` を使用して排除すること。

---

### ⚠️ CQ-02: `DomainTenantResolver.ExtractTenantSegment` の `"{tenant}"` マジックストリング

**該当箇所**: [DomainTenantResolver.cs](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/MultiTenancy/Resolution/Resolvers/DomainTenantResolver.cs#L76)

```csharp
const string placeholder = "{tenant}";
```

メソッドローカルの `const` であるが、`MultiTenancyConstants.Defaults` に移動すべきである。テンプレートのプレースホルダ名を変更する場合にこのメソッドのみが影響を受ける設計は脆弱。

---

### ⚠️ CQ-03: リゾルバの失敗メッセージに `$""` 補間を多用

**該当箇所**: 全リゾルバの `TenantResolutionResult.Fail(...)` 呼び出し

リゾルバの失敗メッセージに文字列補間 (`$"Header '{_headerName}' not found or empty."`) が使用されている。これらはログメッセージではなく Result のエラー文言として使用されているため、Rule 6 の直接的な違反ではないが、構造化された `Error` オブジェクトに移行する場合は定数化が必要になる。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **Strategy + Chain of Responsibility パターン**: `ITenantResolver` × 4 リゾルバ + `TenantResolutionPipeline` による優先順位付き解決パイプラインが適切に実装されている。
2. **定数の一元管理**: `MultiTenancyConstants` に Headers, Claims, QueryString, Defaults, Errors がカテゴリ別に整理されており、Rule 13 に準拠。
3. **`sealed` の適切な適用**: 全リゾルバ (`HeaderTenantResolver`, `ClaimsTenantResolver`, `DomainTenantResolver`, `QueryStringTenantResolver`)、`TenantContext`, `TenantContextAccessor`, `TenantResolutionMiddleware`, `TenantResolutionPipeline`, `TenantContextTenantProvider` が `sealed` 宣言されている (Rule 15 準拠)。
4. **`sealed record TenantInfo`**: イミュータブルなテナント情報を `sealed record` で表現 (Rule 15)。
5. **`sealed record TenantResolutionResult`**: Factory メソッドパターン (`Success()`, `Fail()`) による生成を強制。
6. **`CancellationToken` の完全伝播**: `ITenantResolver.ResolveAsync`, `ITenantStore.GetByIdAsync/GetByDomainAsync`, `TenantResolutionPipeline.ResolveAsync` すべてで `CancellationToken` が伝播されている。
7. **RFC 7807 準拠レスポンス**: テナント解決失敗時に ProblemDetails 形式のレスポンスを返却（匿名型ではあるが構造は準拠）。
8. **Fail-Fast 設計**: `EnforceTenancy = true` のデフォルト設定でテナント未解決リクエストを拒否する防御的設計。
9. **`BaseException` 継承例外**: `TenantNotProvidedException`, `InvalidTenantImplementationException` が `BaseException` を継承し、定数化されたエラーコードを使用。
10. **構造化ログ出力**: `TenantResolutionMiddleware` / `TenantResolutionPipeline` で `{TenantId}`, `{TraceId}` プレースホルダを使用した Rule 6 準拠のログ。
11. **File-Scoped Namespace**: 全ファイルで file-scoped namespace を使用。
12. **`#region` による構造化**: 全クラスで一貫した `#region` タグの使用。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| # | 対応項目 | 重要度 | 対象ファイル |
|---|---|---|---|
| 1 | `ITenantContext` / `TenantContext` の二重 Scoped 登録を修正 (TC-03) | 🔴 高 | `MultiTenancyServiceCollectionExtensions.cs` |
| 2 | テナント ID のサニタイズ / バリデーションを追加 (NF-01) | 🔴 高 | `TenantResolutionMiddleware.cs` |
| 3 | `QueryStringTenantResolver` の本番環境ガードを追加 (NF-02) | 🔴 高 | `QueryStringTenantResolver.cs` |

### 2. リファクタリング提案 (Refactoring)

| # | 対応項目 | 重要度 | 対象ファイル |
|---|---|---|---|
| 4 | `TenantResolverType` enum を別ファイルに分離 (CS-03) | 🟡 中 | `MultiTenancyOptions.cs` → 新規 `TenantResolverType.cs` |
| 5 | DI 登録の Options 二重構築を `services.Configure` に統一 (CS-02) | 🟡 中 | `MultiTenancyServiceCollectionExtensions.cs` |
| 6 | 匿名型 ProblemDetails を `ProblemDetails` / `VKProblemDetails` に置換 (NF-03) | 🟡 中 | `TenantResolutionMiddleware.cs` |
| 7 | `ITenantResolutionPipeline` インターフェースの導入 (TC-02) | 🟡 中 | 新規 `ITenantResolutionPipeline.cs` + `TenantResolutionPipeline.cs` |
| 8 | `TenantContextAccessor` の Service Locator を DI に移行 (TC-01) | 🟡 中 | `TenantContextAccessor.cs` |
| 9 | マジックストリングの定数化 (CQ-01, CQ-02) | 🟢 低 | `TenantResolutionMiddleware.cs`, `DomainTenantResolver.cs` |
| 10 | `TenantResolutionResult` と `Result<T>` の統合検討 + ADR (CS-01) | 🟢 低 | 設計判断 |

### 3. 推奨される学習トピック (Learning Suggestions)

1. **Scoped Service の Factory 登録パターン** — `TryAddScoped<TInterface>(sp => sp.GetRequiredService<TConcrete>())` による単一インスタンス共有の手法を習得すること。
2. **Options パターンの高度な活用** — `IConfigureOptions<T>`, `IPostConfigureOptions<T>`, `IOptionsMonitor<T>` による設定管理の一元化。
3. **テナント分離の深層防御** — EF Core Global Query Filter との連携、テナント ID のバリデーション戦略、クロステナントアクセス防止の多層アプローチ。
4. **`IHostEnvironment` による環境別機能制御** — 開発環境専用機能の安全なガード手法。

---

## 📋 VK.Blocks 規約チェックリスト

- ✅ Result<T> → 本モジュールは `TenantResolutionResult` を使用。`Result<T>` との統一は設計判断事項として記録 (CS-01)。
- ✅ Async/CT → `ITenantResolver.ResolveAsync`, `ITenantStore`, `TenantResolutionPipeline` すべてで `CancellationToken` を正しく伝播。
- ✅ TenantId → 本モジュール自体がテナント分離の基盤を提供。`TenantContext` による Scoped テナント伝播を実装。
- ✅ LogTemplate → `TenantResolutionMiddleware` / `TenantResolutionPipeline` で構造化ログテンプレートを使用。
- ✅ No Null → `ITenantProvider.GetCurrentTenantId()` は `string?` を返すが、これはテナント未解決の正当な状態を表現している。
- ❌ Error Constant → `TenantResolutionMiddleware.cs` の匿名型 ProblemDetails にマジックストリングが存在 (CQ-01)。
- ✅ Polly → 本モジュールは外部 HTTP/Third-party 呼び出しを行わないため対象外。`ITenantStore` の実装側で Polly を適用すべき。
- ✅ NoTracking → DB アクセスを直接行わないため対象外。`ITenantStore` の実装側で適用すべき。
