# 📋 VK.Blocks.Authentication アーキテクチャ監査レポート

| 項目                     | 内容                                                              |
| ------------------------ | ----------------------------------------------------------------- |
| **対象モジュール**       | `VK.Blocks.Authentication`                                        |
| **監査日**               | 2026-04-01                                                        |
| **ソースパス**           | `src/BuildingBlocks/Authentication/`                              |
| **総ファイル数**         | 50 ファイル (.cs)                                                 |
| **プロジェクト依存関係** | `VK.Blocks.Core`, `Microsoft.AspNetCore.Authentication.JwtBearer` |

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **87 / 100 点**
- **対象レイヤー判定**: Infrastructure Layer — BuildingBlock (Cross-Cutting Authentication Module)
- **総評 (Executive Summary)**:

`VK.Blocks.Authentication` モジュールは、**エンタープライズグレードの認証基盤**として極めて高い完成度を示している。JWT、APIキー、OAuthの3つの認証戦略をFeature-Driven構造で統合し、各戦略に対してインターフェース抽象化 → InMemory実装 → バリデータ → エラー定数 → DI拡張という一貫した設計パターンを適用している。

**特に優れている点**:

- `Result<T>` パターンの徹底的な適用と `Error` 定数の体系的な定義
- `IInMemoryCacheCleanup` による自律クリーンアップ・ライフサイクル管理
- OpenTelemetry 準拠の包括的な Diagnostics インフラ（`ActivitySource`, `Meter`, `Counter`, `Histogram`）
- Semantic Authorization Attributes (`[JwtAuthorize]`, `[ApiKeyAuthorize]`, `[AuthGroup]`) による型安全なポリシー適用
- Configuration Validation の Fail-Fast 戦略（`ValidateOnStart` + 複層バリデーション）

**改善が必要な領域**:

- フォルダ構造に一部技術型分離が残存（Rule 12 違反の可能性）
- `ApiKeyContext` が `sealed class` で定義されており、`sealed record` が適切（Rule 15 違反）
- `ApiKeyAuthenticationOptions` が `sealed` 宣言されていない（Rule 15 違反）
- `AuthenticationConstants.cs` に2つの型が定義されている（Rule 14 違反）
- OAuth バリデーションロジックの重複（`VKAuthenticationOptionsValidator` と `OAuthOptionsValidator`）
- `InMemoryCleanupBackgroundService` 内の `PeriodicTimer` ループ構造におけるリソース効率の懸念

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ **Rule 14 違反 — 型分離原則**: [AuthenticationConstants.cs](/src/BuildingBlocks/Authentication/Common/AuthenticationConstants.cs)

ファイル `AuthenticationConstants.cs` に `AuthenticationConstants` と `AuthGroups` の2つの独立した `public static class` が定義されている。Rule 14「One File, One Type」に違反しており、ナビゲーション性を低下させている。

**影響**: コードベースの拡大に伴い、ファイル内の型の発見性が悪化する。特に `AuthGroups` はセマンティック属性との密接な関係があるため、独立ファイルに分離すべきである。

**推奨**: `AuthGroups` を `AuthGroups.cs` として `Common/` に分離する。

---

### ❌ **Rule 15 違反 — `sealed` 宣言の不備**: [ApiKeyAuthenticationOptions.cs](/src/BuildingBlocks/Authentication/ApiKeys/ApiKeyAuthenticationOptions.cs)

```csharp
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
```

`ApiKeyAuthenticationOptions` が `sealed` 宣言されていない。`AuthenticationSchemeOptions` を継承しているため完全な `sealed` は技術的には ASP.NET Core のフレームワーク制約によっては許容されるケースもあるが、**このモジュール内でのポリモーフィズム要件は確認されない**ため、明示的に `sealed` を宣言すべきである。

---

### ❌ **Rule 15 違反 — `sealed record` 未使用**: [ApiKeyContext.cs](/src/BuildingBlocks/Authentication/ApiKeys/ApiKeyContext.cs)

```csharp
public sealed class ApiKeyContext
```

`ApiKeyContext` はイミュータブルなDTOであり、プロパティはすべて `init` アクセサーを持つ。Rule 15 に基づき、`sealed record` として定義するのが適切である。値の等値性と不変性を型システムレベルで保証できる。

---

### ❌ **Rule 15 違反 — `sealed record` 未使用**: [ExternalIdentity.cs](/src/BuildingBlocks/Authentication/Contracts/ExternalIdentity.cs)

```csharp
public record ExternalIdentity
```

`ExternalIdentity` は `sealed` 修飾子なしの `record` として定義されている。このモジュール内での継承は想定されていないため、`sealed record` として宣言すべきである。

---

### ❌ **バリデーションロジックの重複 (DRY 違反)**: [VKAuthenticationOptionsValidator.cs](/src/BuildingBlocks/Authentication/DependencyInjection/VKAuthenticationOptionsValidator.cs) vs [OAuthOptionsValidator.cs](/src/BuildingBlocks/Authentication/OAuth/OAuthOptionsValidator.cs)

`VKAuthenticationOptionsValidator`（L40-59）と `OAuthOptionsValidator`（L11-33）の双方で OAuth プロバイダーオプションのバリデーションが実装されている。前者は `Authority` の URI 検証と `CallbackPath` のスラッシュ検証を含むがより厳密であり、後者は `ClientId` と `Authority` の非空チェックのみである。

**影響**: 将来的にバリデーションルールを変更する際に、2箇所の修正が必要となり、不整合のリスクがある。

**推奨**: `VKAuthenticationOptionsValidator` が包括的バリデーションを担当し、`OAuthOptionsValidator` は `VKAuthenticationOptionsValidator` に統合するか、個別のプロバイダーレベルのバリデーションに特化させる。

---

### ⚠️ **`PeriodicTimer` のループ内再生成**: [InMemoryCleanupBackgroundService.cs](/src/BuildingBlocks/Authentication/Common/InMemoryCleanupBackgroundService.cs) L41

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    var interval = TimeSpan.FromMinutes(options.CurrentValue.InMemoryCleanupIntervalMinutes);
    using var timer = new PeriodicTimer(interval);
    // ...
}
```

`PeriodicTimer` が `while` ループの各イテレーション内で `new` + `Dispose` されている。`IOptionsMonitor` による動的なインターバル変更をサポートする意図と推測されるが、これは各 tick ごとに `PeriodicTimer` インスタンスが再生成されるパフォーマンスオーバーヘッドを伴う。

**影響**: クリーンアップ間隔が一般的に10分以上であるため、実運用上の影響は軽微だが、設計の意図とトレードオフを明示すべきである。

**推奨**: `PeriodicTimer` をループ外で生成し、インターバル変更時のみ再構築するか、コメントでトレードオフを明記する。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 **APIキーハッシュの部分ログ出力**: [ApiKeyValidator.cs](/src/BuildingBlocks/Authentication/ApiKeys/ApiKeyValidator.cs) L53

```csharp
logger.LogWarning("API key not found. Hash: {Hash}", hashedApiKey[..8]);
```

APIキーのハッシュ値の先頭8文字をログに出力している。SHA256 ハッシュの先頭8文字は直接的なセキュリティリスクは低いが、ブルートフォース攻撃の補助情報となる可能性がある。セキュリティ監査の文脈では、この出力の必要性を再評価すべきである。

**評価**: **低リスク** — SHA256 のプリイメージ耐性により直接的な鍵復元は非現実的だが、プロダクション環境での情報露出を最小限にするベストプラクティスに照らすと検討の余地がある。

---

### 🔒 **`JwtAuthenticationService` の `JwtSecurityTokenHandler` 使用**: [JwtAuthenticationService.cs](/src/BuildingBlocks/Authentication/Jwt/JwtAuthenticationService.cs) L32

```csharp
private readonly JwtSecurityTokenHandler _tokenHandler = new();
```

Microsoft は新しいプロジェクトにおいて `JsonWebTokenHandler` の使用を推奨している。`JwtSecurityTokenHandler` はレガシーであり、`JsonWebTokenHandler` はパフォーマンスが向上し、`TokenValidationResult` を直接返すため、例外ベースのエラーハンドリングを回避できる。

**影響**: 現在の実装は正常に機能するが、パフォーマンスと最新のAPIサポートの観点で改善の余地がある。

---

### 🔒 **`JwtValidationFactory` の `sealed` 未宣言**: [JwtValidationFactory.cs](/src/BuildingBlocks/Authentication/Jwt/JwtValidationFactory.cs)

```csharp
public static class JwtValidationFactory
```

`static class` は暗黙的に `sealed` であるため、Rule 15 違反ではないが、`JwtValidationFactory.Create()` において `OidcDiscovery` モード時に `IssuerSigningKey` が設定されない設計は正しい（JWKS エンドポイントから自動取得されるため）。

---

### 🔒 **`InMemoryApiKeyRateLimiter` の `_lastCleanup` フィールド未使用**: [InMemoryApiKeyRateLimiter.cs](/src/BuildingBlocks/Authentication/ApiKeys/InMemoryApiKeyRateLimiter.cs) L18

```csharp
private DateTimeOffset _lastCleanup = DateTimeOffset.UtcNow;
```

`_lastCleanup` フィールドは `CleanupExpiredEntries()` メソッド内で更新されるが、実際にはどこからも**読み取られていない**。デッドコードであり、削除すべきである。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ **優秀な抽象化設計**

モジュール全体を通じて、テスト容易性は**非常に高い**:

| コンポーネント           | インターフェース              | テスト置換可能性         |
| ------------------------ | ----------------------------- | ------------------------ |
| API Key ストレージ       | `IApiKeyStore`                | ✅ 完全にモック可能      |
| API Key 失効             | `IApiKeyRevocationProvider`   | ✅ 完全にモック可能      |
| API Key レート制限       | `IApiKeyRateLimiter`          | ✅ 完全にモック可能      |
| JWT 失効                 | `IJwtTokenRevocationProvider` | ✅ 完全にモック可能      |
| JWT リフレッシュトークン | `IJwtRefreshTokenValidator`   | ✅ 完全にモック可能      |
| JWT サービス             | `IJwtAuthenticationService`   | ✅ 完全にモック可能      |
| Claims プロバイダー      | `IVKClaimsProvider`           | ✅ 完全にモック可能      |
| OAuth マッパー           | `IOAuthClaimsMapper`          | ✅ Keyed DI でモック可能 |

- すべてのコア依存関係がインターフェースを通じて注入されており、`new` キーワードの直接使用は InMemory 実装の内部データ構造（`ConcurrentDictionary`）に限定されている。
- `AuthenticationBuilderExtensions` により、各インターフェースの実装を `Replace` で差し替え可能な拡張ポイントが提供されている。

### ⚙️ **`VKClaimsTransformer` のテスト容易性に関する注意点**

[VKClaimsTransformer.cs](/src/BuildingBlocks/Authentication/Claims/VKClaimsTransformer.cs) は `IServiceScopeFactory` を通じて `IVKClaimsProvider` を解決しているが、これは Service Locator パターンの部分的使用である。ASP.NET Core の `IClaimsTransformation` のフレームワーク制約上、これは許容されるアプローチだが、テスト時は `IServiceScopeFactory` のモック構築が若干複雑になる。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 **エンタープライズグレードの可観測性 — 非常に優秀**

`VK.Blocks.Authentication` の可観測性インフラは、本監査において**最も高い評価ポイント**の一つである。

| メトリクス                               | 計装      | 状態                                                          |
| ---------------------------------------- | --------- | ------------------------------------------------------------- |
| `authentication.requests`                | Counter   | ✅ `auth.type`, `auth.result`, `auth.failure_reason` タグ付き |
| `vk.auth.too_many_requests`              | Counter   | ✅ `auth.key_id`, `auth.tenant_id` タグ付き                   |
| `vk.auth.revocations`                    | Counter   | ✅ `auth.type` タグ付き                                       |
| `vk.auth.replay_detection`               | Counter   | ✅ `auth.user.id` タグ付き                                    |
| `vk.auth.claims_transformation.count`    | Counter   | ✅ `auth.claims_transformed` タグ付き                         |
| `vk.auth.claims_transformation.duration` | Histogram | ✅ ミリ秒単位                                                 |

**分散トレーシング**: `StartJwtValidation()`, `StartApiKeyValidation()`, `StartClaimsTransformation()` の3つの Activity ファクトリが用意されており、各認証フローのスパンが正確に測定可能。

**RFC 7807 準拠**: `AuthenticationResponseHelper` により、すべての認証エラーレスポンスが `ProblemDetails` 形式で `traceId` 付きで返却される。

**構造化ログ**: すべてのログ出力が `{placeholder}` テンプレートを使用しており、文字列補間 (string interpolation) は使用されていない。**Rule 6 完全準拠**。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ **`InMemoryCleanupBackgroundService` の安全でないキャスト**: [InMemoryCleanupBackgroundService.cs](/src/BuildingBlocks/Authentication/Common/InMemoryCleanupBackgroundService.cs) L48

```csharp
((ICollection<IInMemoryCacheCleanup>)cleanupProviders).Count
```

`IEnumerable<IInMemoryCacheCleanup>` を `ICollection<IInMemoryCacheCleanup>` にキャストしてログ出力の `.Count` を取得している。DI コンテナが `IEnumerable<T>` を内部的に配列やリストとして提供する保証はなく、将来的に `InvalidCastException` のリスクがある。

**推奨**: `cleanupProviders.Count()` (LINQ) を使用するか、初期スキャンで取得した `activeProviders.Count` を再利用する。

---

### ⚠️ **`JwtTokenRevocationService` のコンストラクタスタイル不統一**: [JwtTokenRevocationService.cs](/src/BuildingBlocks/Authentication/Jwt/RefreshTokens/JwtTokenRevocationService.cs) L24-27

```csharp
public JwtTokenRevocationService(IJwtTokenRevocationProvider revocationProvider)
{
    _revocationProvider = revocationProvider;
}
```

モジュール内の他のクラス（`ApiKeyValidator`, `VKClaimsTransformer`, `InMemoryJwtRefreshTokenValidator` 等）が **Primary Constructor** を使用しているのに対し、`JwtTokenRevocationService` のみが従来の手動コンストラクタ + フィールド宣言パターンを使用している。統一性の観点で Primary Constructor に移行すべきである。

---

### ⚠️ **`OAuthClaimsMapperBase` の `sealed` 不可**: [OAuthClaimsMapperBase.cs](/src/BuildingBlocks/Authentication/OAuth/Mappers/OAuthClaimsMapperBase.cs)

```csharp
public abstract class OAuthClaimsMapperBase : IOAuthClaimsMapper
```

`abstract class` は設計上 `sealed` にできないため、Rule 15 の例外として許容される。ただし、Template Method パターンとしての使用は適切であり、`GitHubClaimsMapper` による `override` は正しく実装されている。

---

### ⚠️ **フォルダ構成の不整合 (Rule 12 部分違反)**

現在のフォルダ構造:

```
Authentication/
├── ApiKeys/          ✅ Feature-Driven
├── Claims/           ⚠️ Technical-Type
├── Common/           ⚠️ Technical-Type
├── Contracts/        ⚠️ Technical-Type
├── DependencyInjection/  ✅ Cross-Cutting (許容)
├── Diagnostics/      ✅ Cross-Cutting (許容)
├── Extensions/       ⚠️ Technical-Type
├── Features/         ✅ Feature-Driven
├── Jwt/              ✅ Feature-Driven
└── OAuth/            ✅ Feature-Driven
```

`ApiKeys/`, `Jwt/`, `OAuth/` は Vertical Slice として優れた構造だが、`Claims/`, `Common/`, `Contracts/`, `Extensions/` は技術型分類のフォルダである。BuildingBlock モジュールの性質上、完全な Feature-Driven 化は困難な場合もあるが、以下の改善が検討できる:

- `Claims/` → `IVKClaimsProvider` は clems enrichment 用の拡張ポイントであり、`VKClaimTypes` は全体で使用される定数。現在の配置は許容範囲。
- `Contracts/` → `AuthenticatedUser` と `ExternalIdentity` はモジュール外に公開される DTO。`Abstractions/` への改名を検討。
- `Extensions/` → `ClaimsPrincipalExtensions` は Cross-Cutting ユーティリティ。現在の配置は許容範囲。
- `Common/` → `AuthenticationConstants`, `AuthGroups`, `AuthenticationErrors`, `AuthenticationResponseHelper`, `IInMemoryCacheCleanup`, `InMemoryCleanupBackgroundService` — これらは異なる関心事を含んでおり、最も改善の余地がある。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### 1. **`Result<T>` パターンの一貫した適用**

モジュール全体で `Result<T>` パターンが徹底されている。`null` を返す箇所は interface contract 上の `IApiKeyStore.FindByHashAsync` のみであり、これは外部実装者が提供する拡張ポイントとして適切にドキュメント化されている。すべての Application レベルの処理結果が `Result.Success<T>` または `Result.Failure<T>(Error)` で返却されている。

### 2. **`Error` 定数の体系的な定義**

各 Feature フォルダに専用の Errors クラスが定義されている:

- `ApiKeyErrors` (6 定数)
- `JwtErrors` (7 定数)
- `JwtRefreshTokenErrors` (4 定数)
- `OAuthErrors` (6 定数)
- `AuthenticationErrors` (1 定数)

すべての Error は `static readonly` フィールドとして定義され、`ErrorType` (Validation / Unauthorized / Failure / TooManyRequests) による分類がRFC 7807 の HTTP ステータスコードマッピングに対応している。

### 3. **自律クリーンアップ・ライフサイクル (`IInMemoryCacheCleanup`)**

`InMemoryCleanupBackgroundService` は、登録されたすべての `IInMemoryCacheCleanup` プロバイダーを定期的にスキャンし、**Self-Adaptive Strategy** により、Redis 等の外部プロバイダーに置換されたインメモリ実装のクリーンアップを自動的にスキップする。これはリソース効率とオペレーション安全性の両方を担保する優れた設計である。

### 4. **`AddInMemoryCleanupProvider<TService, TImplementation>` メソッドの DI 設計**

[AuthenticationBlockExtensions.cs](/src/BuildingBlocks/Authentication/DependencyInjection/AuthenticationBlockExtensions.cs) L234-249 の `AddInMemoryCleanupProvider` メソッドは、3層のDI登録（具象型 → インターフェース → クリーンアップインターフェース）を一括で行うヘルパーであり、DRY 原則を遵守しつつ、`TryAdd` / `TryAddEnumerable` による冪等な登録を保証している。

### 5. **Semantic Authorization Attributes の型安全性**

`[JwtAuthorize]`, `[ApiKeyAuthorize]`, `[AuthGroup(AuthGroups.User)]` などのセマンティック属性は、マジックストリングを完全に排除し、コンパイル時の型安全性を提供している。ポリシー名が `AuthenticationConstants` からの定数参照で構築されるため、リファクタリング耐性も高い。

### 6. **Configuration Validation の多層防御**

バリデーションが3層で実装されている:

1. **`DataAnnotations`** — `AddVKBlockOptions` による基本バリデーション
2. **`IValidateOptions<T>`** — `VKAuthenticationOptionsValidator`, `JwtOptionsValidator`, `ApiKeyOptionsValidator`, `OAuthOptionsValidator` による詳細バリデーション
3. **`ValidateOnStart`** — アプリケーション起動時の即座フェイル

これにより、設定ミスがランタイム障害に発展する前に検出される。

### 7. **パフォーマンス最適化**

- `ApiKeyValidator.HashApiKey()`: `stackalloc` を使用してヒープ割り当てを回避（256バイト以下の場合）
- InMemory 実装全体: `ConcurrentDictionary` + `ValueTask` による最小オーバーヘッド
- `VKClaimsTransformer`: `Stopwatch.GetTimestamp()` によるGC圧力の低い計測

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| #   | 対象                                   | 内容                                                                             | 根拠                          |
| --- | -------------------------------------- | -------------------------------------------------------------------------------- | ----------------------------- |
| 1   | `AuthenticationConstants.cs`           | `AuthGroups` クラスを `Common/AuthGroups.cs` に分離                              | Rule 14 違反                  |
| 2   | `ApiKeyContext`                        | `sealed class` → `sealed record` に変更                                          | Rule 15 違反                  |
| 3   | `ExternalIdentity`                     | `record` → `sealed record` に変更                                                | Rule 15 違反                  |
| 4   | `ApiKeyAuthenticationOptions`          | `public class` → `public sealed class` に変更                                    | Rule 15 違反                  |
| 5   | `InMemoryCleanupBackgroundService` L48 | `((ICollection<...>)cleanupProviders).Count` → `cleanupProviders.Count()` に変更 | `InvalidCastException` リスク |
| 6   | `InMemoryApiKeyRateLimiter` L18        | 未使用フィールド `_lastCleanup` を削除                                           | デッドコード                  |

### 2. リファクタリング提案 (Refactoring)

| #   | 対象                                                         | 内容                                                             | 優先度 |
| --- | ------------------------------------------------------------ | ---------------------------------------------------------------- | ------ |
| 1   | `JwtTokenRevocationService`                                  | Primary Constructor パターンに統一                               | 中     |
| 2   | `VKAuthenticationOptionsValidator` / `OAuthOptionsValidator` | OAuth バリデーションロジックの重複排除                           | 中     |
| 3   | `JwtAuthenticationService`                                   | `JwtSecurityTokenHandler` → `JsonWebTokenHandler` への移行検討   | 中     |
| 4   | `InMemoryCleanupBackgroundService`                           | `PeriodicTimer` のループ外生成、またはトレードオフコメントの追加 | 低     |
| 5   | `ApiKeyValidator.HashApiKey()` L53                           | ハッシュの部分ログ出力の必要性再評価                             | 低     |

### 3. 推奨される学習トピック (Learning Suggestions)

| トピック                                                     | 推奨理由                                                                                                                                                                    |
| ------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`JsonWebTokenHandler` への移行**                           | Microsoft が推奨する新しい JWT ハンドラーであり、`TokenValidationResult` を直接返却するため、例外ベースの制御フローを排除できる。パフォーマンスも向上する。                 |
| **Source Generator による `OAuthDiscoveryCache` の自動登録** | `OAuthDiscoveryCache.Register()` のランタイム呼び出しをコンパイル時生成に移行することで、起動時のリフレクションコストを排除し、AOT 互換性を向上させる。                     |
| **`IAsyncDisposable` の適用検討**                            | `InMemory*` 実装が `ConcurrentDictionary` を管理しているが、`IAsyncDisposable` による明示的なリソース解放を検討することで、Graceful Shutdown 時のデータ整合性を向上できる。 |

---

## 📝 VK.Blocks ルール準拠チェックリスト

| ルール                 | 状態     | 詳細                                                                                                        |
| ---------------------- | -------- | ----------------------------------------------------------------------------------------------------------- |
| ✅ Result\<T\>         | 準拠     | すべてのパブリックメソッドが `Result<T>` を返却。`null` 返却なし                                            |
| ✅ Layer Dependencies  | 準拠     | Core/Application に EF Core / Redis 等の依存なし                                                            |
| ✅ Async/CT            | 準拠     | すべての I/O 操作に `async/await` + `CancellationToken` 使用                                                |
| ✅ Performance         | 準拠     | ループ内DB呼び出しなし。`stackalloc`、`ValueTask` 活用                                                      |
| ✅ Automation          | N/A      | `IAuditable` / `ISoftDelete` 該当なし（認証モジュール）                                                     |
| ✅ Observability       | 準拠     | 構造化ログテンプレート、TraceId、OpenTelemetry メトリクス完備                                               |
| ✅ Security            | 準拠     | ハッシュベースの API キー検証、失効管理、リプレイ攻撃検出                                                   |
| ⚠️ Resiliency          | 部分適用 | 外部 HTTP 呼び出しなし（Polly 不要）。ただし `IApiKeyStore` 実装が外部DBの場合の Polly 要件はホスト側の責任 |
| ❌ Code Standards      | 部分違反 | Rule 14（型分離）、Rule 15（sealed/record）に一部違反                                                       |
| ✅ No TODO/Placeholder | 準拠     | プレースホルダーコードなし。すべてのコードが本番品質                                                        |
| ⚠️ Folder Organization | 部分違反 | `Claims/`, `Common/`, `Contracts/`, `Extensions/` が技術型分類                                              |
| ✅ Error Constants     | 準拠     | 5つの専用 Errors クラスで体系的に定義                                                                       |
