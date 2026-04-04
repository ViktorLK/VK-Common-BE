# アーキテクチャ監査レポート — Authentication.OpenIdConnect

> **モジュール**: `VK.Blocks.Authentication.OpenIdConnect`
> **監査日**: 2026-04-03
> **監査対象パス**: `src/BuildingBlocks/Authentication.OpenIdConnect/`
> **総ファイル数**: 9 (.cs ファイル、bin/obj 除外)

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **72 / 100**
- **対象レイヤー判定**: Infrastructure Extension Layer (Authentication BuildingBlock の OIDC 拡張モジュール)
- **総評 (Executive Summary)**:

本モジュールは、VK.Blocks.Authentication コアモジュールを拡張し、OpenID Connect プロトコルによるフェデレーション認証を実現するインフラストラクチャ拡張ライブラリである。Feature-Driven Vertical Slice アーキテクチャへの準拠、Source-Generated ロガー (`[LoggerMessage]`) の採用、OpenTelemetry ベースの診断基盤 (`OidcDiagnostics`) の導入など、モダンな設計原則が適用されている。

しかしながら、**DI 登録フロー内の `BuildServiceProvider()` アンチパターン**、**OidcConfigurationValidator と既存 OAuthOptionsValidator 間のバリデーション責務の重複**、**`ExternalIdentity` 構築時の `required` プロパティ不使用**、および **`OidcHandlerFactory` 内の `sealed` 修飾子欠落**といった設計上の懸念が存在する。これらは運用時のサービスプロバイダーリーク、バリデーションロジックの散逸、コンパイル時安全性の低下を引き起こすリスクがあり、早急な対応が推奨される。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ C-01: `BuildServiceProvider()` アンチパターン — サービスプロバイダーリーク

- **該当箇所**: [`OidcAuthenticationBuilderExtensions.cs` L50](src/BuildingBlocks/Authentication.OpenIdConnect/DependencyInjection/OidcAuthenticationBuilderExtensions.cs)
- **重大度**: 🔴 Critical

```csharp
var sp = services.BuildServiceProvider(); // ← L50
var configuration = sp.GetRequiredService<IConfiguration>();
```

**問題の詳細**:
DI 登録フェーズで `BuildServiceProvider()` を呼び出している。これは以下の深刻な問題を引き起こす:

1. **Captive Dependency (囚われ依存)**: 中間 `ServiceProvider` が構築されるため、Singleton として登録されたサービスが最終的なルートプロバイダーとは別のインスタンスとして生成される可能性がある。
2. **リソースリーク**: 構築されたプロバイダーが `Dispose()` されないため、`IConfiguration` 等のサービスが適切に解放されない。
3. **ASP.NET Core Analyzer 警告**: `ASP0000` — "Calling 'BuildServiceProvider' from application code results in an additional copy of singleton services being created."

**推奨修正**:
`IConfiguration` は `IServiceCollection` の登録フェーズで直接注入するか、`PostConfigure` / `IConfigureOptions<T>` パターンを使用して遅延解決に変更する。

```csharp
// 推奨: IConfiguration を引数として受け取る
public static IVKBlockBuilder<AuthenticationBlock> AddDiscoveryOAuth(
    this IVKBlockBuilder<AuthenticationBlock> builder,
    IConfiguration configuration)
```

---

### ❌ C-02: バリデーション責務の重複 — 単一責任原則 (SRP) 違反

- **該当箇所**: [`OidcConfigurationValidator.cs`](src/BuildingBlocks/Authentication.OpenIdConnect/DependencyInjection/OidcConfigurationValidator.cs) 全体
- **重大度**: 🟠 High

**問題の詳細**:
`OidcConfigurationValidator` は `IValidateOptions<OAuthOptions>` を実装し、`Authority` の URI 妥当性と `ClientId` の必須チェックを行っている。しかし、コアモジュールの [`OAuthOptionsValidator`](src/BuildingBlocks/Authentication/Features/OAuth/OAuthOptionsValidator.cs) が**完全に同一の検証ロジック**（Authority、ClientId、CallbackPath）を既に実装している。

| 検証項目 | `OAuthOptionsValidator` (コア) | `OidcConfigurationValidator` (OIDC) |
|---|---|---|
| Authority 必須 | ✅ | ✅ (重複) |
| Authority URI 形式 | ✅ | ✅ (重複) |
| ClientId 必須 | ✅ | ✅ (重複) |
| CallbackPath 形式 | ✅ | ❌ (未検証) |

**影響**:
- 同一の `IValidateOptions<OAuthOptions>` に対して 2 つのバリデーターが DI に登録される。Options フレームワークは全バリデーターを順次実行するため、**同一エラーが二重に報告される**可能性がある。
- バリデーションルールの変更時に 2 箇所のメンテナンスが必要となる。

**推奨修正**:
`OidcConfigurationValidator` を**削除**し、コアモジュールの `OAuthOptionsValidator` に統合する。OIDC 固有の検証が必要な場合は、拡張バリデーターとして差分のみを追加する設計とする。

---

### ❌ C-03: `ExternalIdentity` 構築時の `required` プロパティ不使用

- **該当箇所**: [`OidcHandlerFactory.cs` L62-73](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcHandlerFactory.cs)
- **重大度**: 🟠 High

```csharp
return new ExternalIdentity
{
    Provider = providerName,
    ProviderId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                 ?? context.Principal?.FindFirst("sub")?.Value 
                 ?? "unknown",  // ← マジックストリング
    Email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value 
            ?? context.Principal?.FindFirst("email")?.Value,
    // ...
    Claims = claims ?? new Dictionary<string, string>()
};
```

**問題の詳細**:
1. `ExternalIdentity` は `required string Provider` と `required string ProviderId` を持つ `sealed record` であるが、`ProviderId` のフォールバック値として `"unknown"` というマジックストリングが使用されている。Rule 13 (定数の可視性階層) に違反。
2. `Claims` プロパティは `IReadOnlyDictionary<string, string>` 型だが、`Dictionary<string, string>` で初期化している。型の不一致自体は暗黙変換で問題ないが、`ToDictionary()` の結果が null の場合に新規辞書を生成しており、`GroupBy` のパイプラインで `claims` 変数が null になりうる。

**推奨修正**:
- `"unknown"` を `OidcConstants` に定数として定義する。
- null 合体演算子のチェーンを、`ExternalIdentity` の生成を担当する明示的なファクトリメソッドに抽出する。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 S-01: マジックストリングの存在 — `"VK.Federated"` 認証タイプ

- **該当箇所**: [`OidcHandlerFactory.cs` L45](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcHandlerFactory.cs)

```csharp
var identity = new ClaimsIdentity(internalClaims, "VK.Federated"); // ← マジックストリング
```

認証タイプ `"VK.Federated"` はモジュール全体で共有されるべき定数だが、インラインで宣言されている。Rule 13 に従い、`OidcConstants` に昇格させるべきである。

---

### 🔒 S-02: `ResponseType` のハードコード

- **該当箇所**: [`OidcAuthenticationBuilderExtensions.cs` L84](src/BuildingBlocks/Authentication.OpenIdConnect/DependencyInjection/OidcAuthenticationBuilderExtensions.cs)

```csharp
options.ResponseType = "code"; // ← ハードコード
```

OAuth 2.0 Authorization Code Flow のレスポンスタイプがハードコードされている。今後 Hybrid Flow (`"code id_token"`) や Implicit Flow への対応が必要になった場合に変更が困難。設定可能な定数または `OAuthProviderOptions` のプロパティとして外部化すべきである。

---

### 🔒 S-03: `"sub"` / `"email"` / `"name"` のマジックストリング

- **該当箇所**: [`OidcHandlerFactory.cs` L65-71](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcHandlerFactory.cs)

```csharp
?? context.Principal?.FindFirst("sub")?.Value    // ← マジックストリング
?? context.Principal?.FindFirst("email")?.Value   // ← マジックストリング
?? context.Principal?.FindFirst("name")?.Value    // ← マジックストリング
```

OpenID Connect 標準クレームの名称がマジックストリングとして散在している。`System.Security.Claims.ClaimTypes` との二重参照パスも煩雑であり、OIDC 標準クレーム名を `OidcConstants` に集約すべきである。

---

### 🔒 S-04: `await Task.CompletedTask` の無駄な await

- **該当箇所**: [`OidcHandlerFactory.cs` L52](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcHandlerFactory.cs)

```csharp
await Task.CompletedTask; // ← 不要
```

`CreateOnTokenValidated` のラムダは `async` 宣言されているが、実際には非同期 I/O を行っていない。`Task.CompletedTask` を返す最適な方法は async 修飾子を削除して `return Task.CompletedTask` にするか、将来的に非同期処理が追加される前提で現状を維持するかの判断が必要。現状では不要な `async` ステートマシンのオーバーヘッドが生じている。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ T-01: `OidcHandlerFactory` の静的クラス設計によるテスト困難性

`OidcHandlerFactory` は `internal static class` として宣言されているが、以下の理由からユニットテストが極めて困難:

1. **`TokenValidatedContext` への直接依存**: ASP.NET Core 認証イベントのコンテキストオブジェクトをモックすることが困難。
2. **`context.HttpContext.RequestServices` からの直接サービス解決**: Service Locator パターンに相当し、依存関係が不透明。
3. **内部メソッド `ExtractExternalIdentity` のテスト不能**: `private static` であるため直接テストできない。

**推奨修正**:
- `ExtractExternalIdentity` を `internal` に昇格し、`[InternalsVisibleTo]` でテストプロジェクトからアクセス可能にする。
- 長期的には、イベントハンドラーのロジックを DI 可能なサービスクラスに抽出し、コンテキスト依存を最小化する。

---

### ⚙️ T-02: `sealed` 修飾子の欠落

- **該当箇所**: [`OidcHandlerFactory.cs` L19](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcHandlerFactory.cs)

```csharp
internal static class OidcHandlerFactory // ← static class は sealed 不要 (暗黙的に sealed)
```

*注記*: `static class` は C# では暗黙的に `sealed` であるため、この指摘は実質的な影響なし。ただし、他のクラス（`OidcConstants`, `OidcDiagnosticsConstants`, `OidcDiagnostics`）も同様に `static` であるため問題なし。

Claims Mapper (`StandardOidcClaimsMapper`, `GoogleOidcClaimsMapper`, `AzureB2COidcClaimsMapper`) は適切に `sealed class` が適用されている。✅

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 O-01: Source-Generated ロガーの適切な実装 ✅

[`OidcLog.cs`](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcLog.cs) は `[LoggerMessage]` ソースジェネレーターを正しく使用しており、Rule 16 に完全準拠している:

- `LogOidcProviderRegistered` — Information レベル
- `LogOidcAuthenticationSuccess` — Information レベル
- `LogOidcAuthenticationFailed` — Warning レベル
- `LogOidcMappingError` — Error レベル

全メッセージに構造化テンプレートと `TraceId` パラメータが含まれている。

---

### 📡 O-02: OpenTelemetry 診断基盤 ✅

[`OidcDiagnostics.cs`](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcDiagnostics.cs) は `[VKBlockDiagnostics]` ソースジェネレーター属性を使用し、`ActivitySource` と `Meter` を自動生成している:

- `Counter<long> AuthenticationRequests` — 認証試行回数の計測
- `RecordAuthAttempt()` — 成功/失敗のタグ付きメトリクス記録
- `StartOidcValidation()` — 分散トレーシングアクティビティの開始

[`OidcDiagnosticsConstants.cs`](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcDiagnosticsConstants.cs) によるメトリクス名・タグ名の定数集約も適切。

---

### 📡 O-03: `LogOidcProviderRegistered` 呼び出し時の空 TraceId

- **該当箇所**: [`OidcAuthenticationBuilderExtensions.cs` L67](src/BuildingBlocks/Authentication.OpenIdConnect/DependencyInjection/OidcAuthenticationBuilderExtensions.cs)

```csharp
logger?.LogOidcProviderRegistered(schemeName, providerOptions.Authority, string.Empty);
// TraceId = string.Empty ← 起動時コンテキストのため TraceId 不在
```

DI 登録フェーズでは HTTP リクエストコンテキストが存在しないため、`TraceId` が空文字列となる。これはアーキテクチャ上の制約であるが、ログの一貫性を保つために `"startup"` 等の定数値を使用することを推奨する。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ Q-01: `OidcConstants` の可視性が `internal` だが Mapper は `public`

- **該当箇所**: [`OidcConstants.cs` L6](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcConstants.cs)

`OidcConstants` クラスは `internal static class` として宣言されているが、メンバー定数は `public const` で宣言されている。`internal` クラス内の `public` メンバーは実質 `internal` スコープに制限されるため機能上の問題はないが、意図の明確化のために `internal const` に統一すべきである。

---

### ⚠️ Q-02: `OidcLog` クラスの `ILogger` 型制約

- **該当箇所**: [`OidcLog.cs`](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcLog.cs)

`OidcLog` のロガーメソッドは `ILogger` (非ジェネリック) を拡張しているが、実際の呼び出し箇所では `ILogger<AuthenticationBlock>` を使用している。型安全性の観点から、特定のカテゴリに制約するか、非ジェネリック `ILogger` の使用を一貫させるべきである。現状で機能上の問題はないが、ログカテゴリの一貫性に影響する。

---

### ⚠️ Q-03: `OAuthProviderAttribute` の利用方法 — `StandardOidcClaimsMapper`

- **該当箇所**: [`StandardOidcClaimsMapper.cs` L11](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/Mappers/StandardOidcClaimsMapper.cs)

```csharp
[OAuthProvider("Standard")] // ← マジックストリング
public sealed class StandardOidcClaimsMapper : OAuthClaimsMapperBase
```

`"Standard"` はキー値として DI 登録および `GetKeyedService` で使用されるが、`OidcConstants` に定義されていない。`OidcConstants.Google` / `OidcConstants.AzureB2C` と同様に定数化が必要。

---

### ⚠️ Q-04: Null Conditional 演算子チェーンの過多

- **該当箇所**: [`OidcHandlerFactory.cs` L58-72](src/BuildingBlocks/Authentication.OpenIdConnect/Features/Oidc/OidcHandlerFactory.cs)

`ExtractExternalIdentity` メソッドは `context.Principal?.Claims` に対して `?.` 演算子を多用しているが、`TokenValidatedContext` が呼び出される時点で `Principal` が null であるケースは認証失敗を意味する。防御的プログラミングとして正当だが、ガード句 (`if (context.Principal is null) return ...`) を先頭に配置してフェイルファスト化すべきである。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### ✅ H-01: Feature-Driven Vertical Slice アーキテクチャ
`Features/Oidc/` 配下にハンドラーファクトリ、定数、診断、マッパーが集約されており、Rule 12 に準拠した優れた構造設計。

### ✅ H-02: `[VKBlockDiagnostics]` ソースジェネレーターの活用
OpenTelemetry 互換のメトリクス・トレーシング基盤が標準化されたパターンで統合されている。

### ✅ H-03: `[LoggerMessage]` ソースジェネレーターの完全採用
直接の `logger.LogXxx()` 呼び出しが存在せず、Rule 16 に完全準拠。高パフォーマンスな構造化ロギングが実現されている。

### ✅ H-04: `sealed` 修飾子の適用
全 Claims Mapper (`StandardOidcClaimsMapper`, `GoogleOidcClaimsMapper`, `AzureB2COidcClaimsMapper`) に `sealed` が適用されており、Rule 15 に準拠。

### ✅ H-05: Strategy パターンによるクレームマッピング
`IOAuthClaimsMapper` インターフェースと `OAuthClaimsMapperBase` 基底クラスによる Strategy パターンが適切に実装されており、プロバイダー固有のロジック拡張が容易。`[OAuthProvider]` 属性による Keyed DI 登録も洗練されている。

### ✅ H-06: 拡張モジュールとしての適切な依存方向
`Authentication.OpenIdConnect` → `Authentication` (コア) への一方向依存であり、Dependency Inversion Principle に準拠。コアモジュールは OIDC 固有の実装に一切依存していない。

### ✅ H-07: Backchannel HttpClient の外部化
`OidcBackchannelName` 定数を公開し、アプリケーションレベルでの Polly ポリシー適用を可能にしている。レジリエンス責務のライブラリからの分離は Rule 8 の精神に沿った適切なアーキテクチャ判断。

### ✅ H-08: Fail-Fast バリデーション
`ValidateOnStart()` による起動時設定検証が実装されており、誤設定の早期検出が保証されている。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action) — 🔴

| # | 対象 | 問題 | 推奨アクション |
|---|---|---|---|
| C-01 | `OidcAuthenticationBuilderExtensions.cs` L50 | `BuildServiceProvider()` アンチパターン | `IConfiguration` を `AddDiscoveryOAuth` の引数として受け取るか、`PostConfigure` パターンに移行 |
| C-02 | `OidcConfigurationValidator.cs` | コア `OAuthOptionsValidator` との重複 | クラス削除、コアバリデーターへの統合 |
| S-01 | `OidcHandlerFactory.cs` L45 | `"VK.Federated"` マジックストリング | `OidcConstants.FederatedAuthType` として定数化 |
| S-03 | `OidcHandlerFactory.cs` L65-71 | `"sub"`, `"email"`, `"name"` マジックストリング | `OidcConstants` にOIDC 標準クレーム定数を追加 |
| Q-03 | `StandardOidcClaimsMapper.cs` L11 | `"Standard"` マジックストリング | `OidcConstants.Standard` として定数化 |

### 2. リファクタリング提案 (Refactoring) — 🟠

| # | 対象 | 問題 | 推奨アクション |
|---|---|---|---|
| C-03 | `OidcHandlerFactory.cs` L62-73 | `ExternalIdentity` 構築 — `"unknown"` マジックストリング | `OidcConstants.UnknownProviderId` 定数化 + ファクトリメソッド抽出 |
| S-02 | `OidcAuthenticationBuilderExtensions.cs` L84 | `"code"` ハードコード | `OAuthProviderOptions.ResponseType` プロパティ化の検討 |
| S-04 | `OidcHandlerFactory.cs` L52 | `await Task.CompletedTask` 冗長 | `async` 修飾子削除または将来拡張のコメント追加 |
| T-01 | `OidcHandlerFactory.cs` | 静的クラスによるテスト困難性 | `ExtractExternalIdentity` を `internal` に変更、`InternalsVisibleTo` 設定 |
| Q-01 | `OidcConstants.cs` | `internal class` 内の `public const` | `internal const` に統一 |
| Q-04 | `OidcHandlerFactory.cs` L58 | 過多な null conditional チェーン | ガード句による Early Return 導入 |
| O-03 | `OidcAuthenticationBuilderExtensions.cs` L67 | 起動時ログの空 TraceId | `OidcConstants.StartupTraceId` として定数化 (`"startup"`) |

### 3. 推奨される学習トピック (Learning Suggestions) — 🟢

| トピック | 理由 |
|---|---|
| **Options Pattern — `IPostConfigureOptions<T>` / `IConfigureNamedOptions<T>`** | `BuildServiceProvider()` 排除後の遅延オプション構成パターンを正しく理解するため |
| **ASP.NET Core Keyed DI Services** | `GetKeyedService` の高度な活用パターンと、属性ベース自動登録の最適化 |
| **Disposable ServiceProvider の回避パターン** | `ASP0000` アナライザー警告の根本原因と、DI 登録フェーズでの設定読み取りのベストプラクティス |
| **`InternalsVisibleTo` によるテスト戦略** | `internal` クラスのユニットテスタビリティ確保と、アセンブリレベルのアクセス制御設計 |

---

> **次のステップ**: 最優先対応 (C-01, C-02) の修正後、統合テストの追加と共にコアモジュール `Authentication` との整合性を再検証することを推奨する。
