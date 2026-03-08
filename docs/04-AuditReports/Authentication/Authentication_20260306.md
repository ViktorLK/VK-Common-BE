# アーキテクチャ監査レポート: VK.Blocks.Authentication

| 項目               | 内容                                       |
| ------------------ | ------------------------------------------ |
| **対象モジュール** | `src/BuildingBlocks/Authentication`        |
| **監査実施日**     | 2026-03-06                                 |
| **監査者**         | VK.Blocks Lead Architect (AI Audit System) |
| **前回監査日**     | 2026-03-05 (`Authentication_20260305.md`)  |

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **88 / 100**
- **対象レイヤー判定**: Infrastructure Layer (Authentication / Security / ApiKey) + Internal DSL (Fluent DI)
- **総評 (Executive Summary)**:

前回監査（2026-03-05）から大幅な改善が確認された。特に、`ITokenBlacklist` の抽象化によるリボケーション責務の分離、`IApiKeyBlacklist` の導入によるリボケーションとバリデーションの明確な責任分界、`IApiKeyRateLimiter` によるレートリミティング機能の追加、および `[VKBlockDiagnostics]` ソースジェネレーターを活用した可観測性のインフラ化は、いずれも高水準のアーキテクチャ判断と評価できる。

ただし、いくつかの中程度の指摘事項が残存する。特に、`VKAuthenticationOptions` および `ApiKeyAuthenticationOptions` がミュータブルな `class` として定義されている点、`ApiKeyRecord` が `sealed` でない点、`VKClaimsTransformer` が `sealed` でない点、および `JwtBearerEventsFactory` 内のサービスロケーターパターン（`GetService<T>`）の使用は、プロジェクト標準との乖離として引き続き改善が推奨される。

全体的には、Clean Architecture の依存関係ルール、`Result<T>` パターン、非同期処理の規約は正しく適用されており、エンタープライズレベルの認証モジュールとして十分に機能する品質に達している。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

> 前回監査と比較して、致命的な問題はすべて解消された。以下は継続的な注意が必要な事項。

- ⚠️ **[Service Locator アンチパターン]**: `JwtBearerEventsFactory.cs` (Line 31) — `context.HttpContext.RequestServices.GetService<ITokenBlacklist>()` はサービスロケーターパターンであり、DIP に反する。`ITokenBlacklist` はファクトリクラスのコンストラクタに注入することが本来望ましいが、`JwtBearerEvents` がフレームワーク起点でのコールバックである性質上、完全な回避は困難。コードコメントでその理由を明示するべき。

- ⚠️ **[型設計の非準拠]**: `VKAuthenticationOptions.cs` (Line 6), `ApiKeyAuthenticationOptions.cs` (Line 8) — オプションクラスが `record` ではなく `class` として定義されている。プロジェクトルール15（Immutable Data: DTO・設定は `sealed record`）に違反している。フレームワークのバインディング要件（`IOptions<T>` は `new()` 制約を要求）と競合するため、設計上のトレードオフが生じているが、このトレードオフはコメントで明示されるべきである。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[セキュリティ: ハッシュ処理]**: `ApiKeyValidator.cs` (Line 110-121) — SHA-256 ハッシュと `stackalloc` による低アロケーション実装は良好。ただし、SHA-256 は high-value API キーには適切だが、将来的に HMAC-SHA256（HMAC with secret pepper）への移行を検討すること。現状の純粋なハッシュは、データベース漏洩時に Rainbow Table 攻撃に晒されるリスクがある。

- 🔒 **[セキュリティ: ログマスキング未実施]**: `ApiKeyValidator.cs` (Line 51) — `hashedKey[..8]` を警告ログに出力しているが、ハッシュの先頭8文字であっても、同一ハッシュプレフィックスを持つキーを特定するヒントになりうる。PII マスキング規約（Rule 7）の観点から、ログ出力は `[REDACTED]` に変更、または専用マスクプロセッサに委譲することが望ましい。

- 🔒 **[セキュリティ: JWTのClaimsIdentifierの二重チェック]**: `JwtAuthenticationService.cs` (Lines 70-87) — userId と jti の両方でリボケーションチェックを実施している点は評価できる。ただし、`userId` が取得できない場合（匿名トークン等）はユーザーレベルのチェックがスキップされる動作について、明示的なログ出力がない。潜在的なセキュリティイベントを見逃すリスクがある。

- 🔒 **[セキュリティ: Scope 強制の網羅性]**: `ScopeAuthorizationHandler.cs` — スコープ検証は正しく実装されている。しかし、`DependencyInjection/AuthenticationBlockExtensions.cs` において、スコープポリシーを自動登録する仕組みがない。スコープポリシーの登録はアプリケーション側に委ねられており、設定漏れのリスクがある。

- 🟡 **[パフォーマンス: Fixed Window Rate Limiting の精度]**: `DistributedCacheApiKeyRateLimiter.cs` (Lines 52-72) — IDistributedCache フォールバック実装では、`GetStringAsync` → `SetStringAsync` がアトミックでないため、並行リクエスト環境でレートリミットが正確に機能しない（Race Condition リスク）。フォールバック使用時にはその旨のログが出力（Line 35）されているが、Polly による Circuit Breaker と組み合わせて Redis が回復した場合の動作切替を定義すること。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[良好: インタフェース主導設計]**: `JwtAuthenticationService`, `ApiKeyValidator`, `TokenRevocationService`, `DistributedRefreshTokenValidator` はすべてインタフェースに依存しており、Moq 等でのユニットテストが容易。

- ⚙️ **[改善推奨: `VKClaimsTransformer` の密結合]**: `VKClaimsTransformer.cs` (Line 41) — `scopeFactory.CreateScope()` を内部で呼び出すパターンは、テスト時の DI コンテナモックアップを困難にする。`IVKClaimsProvider` を直接コンストラクタインジェクションし、ASP.NET Core の `Scoped` ライフサイクル管理に委ねるシンプルなアプローチが望ましい。

- ⚙️ **[改善推奨: `ApiKeyAuthenticationHandler` の具象クラス依存]**: `ApiKeyAuthenticationHandler.cs` (Line 21) — `ApiKeyValidator` が `IApiKeyValidator` ではなく具象クラスで直接注入されている。ユニットテスト時に Validator ロジック全体が実行されてしまい、Handler の分離テストが困難。`IApiKeyValidator` インタフェースの導入を推奨する。

- ⚙️ **[改善推奨: `AuthenticationBlockExtensions` のテスト困難性]**: DI 拡張メソッドの統合テストが未確認の状態。`WebApplicationFactory` を使用した Integration Test で、各スキームが正しく登録・動作することを確認するスモークテストが必要。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[優秀: Source Generator による可観測性のインフラ化]**: `AuthenticationDiagnostics.cs` — `[VKBlockDiagnostics]` ソースジェネレーターが `ActivitySource` と `Meter` を自動生成する設計は、モジュール横断的な可観測性インフラの理想的な実装。`authentication.requests` と `authentication.rate_limit_exceeded` のメトリクスは OpenTelemetry によるダッシュボード連携に直結できる。

- 📡 **[準拠: RFC 7807 準拠のエラーレスポンス]**: `AuthenticationResponseHelper.cs` (Line 27) — `ProblemDetails` に `traceId` を付加しており、RFC 7807 および Rule 6（TraceId の必須化）に適合している。これは優れた実装パターン。

- 📡 **[軽微な不備: ログテンプレートの一部逸脱]**: `JwtAuthenticationService.cs` (Line 53) — `logger.LogError("JWT Validation options are not configured properly.")` は構造化ログではあるが、プレースホルダーが存在しない。Rule 6 の「構造化ログテンプレート with プレースホルダー」要件には厳密には適合していない。`logger.LogError("JWT Validation failed. Module: {Module}", nameof(JwtAuthenticationService))` のように識別子を付加することを推奨。

- 📡 **[改善推奨: TraceId の明示的なアクティビティタグ付け]**: `JwtAuthenticationService.cs` (Line 91) — 認証成功パスでは `activity.SetTag("auth.user.id", ...)` でユーザー ID が記録されるが、失敗パス（Line 97-105）では `activity.SetTag("auth.failure.reason", ...)` による失敗理由のタグ付けが欠如している。失敗トレースのデバッグ能力向上のために追加を推奨。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[Rule 15 違反: `sealed` 欠如]**: 以下のクラスが `sealed` でない（Rule 15: 全 Application/Infrastructure クラスは明示的な多態性要件がない限り `sealed` にすること）:
    - `VKAuthenticationOptions.cs` — `public class VKAuthenticationOptions` → フレームワーク制約上の例外だがコメントが必要
    - `ApiKeyAuthenticationOptions.cs` — `public class ApiKeyAuthenticationOptions` → 同上
    - `ApiKeyRecord.cs` — `public class ApiKeyRecord` → `sealed` にすべき
    - `VKClaimsTransformer.cs` — `public class VKClaimsTransformer` → `sealed` にすべき
    - `AzureB2CClaimsMapper.cs`, `GoogleClaimsMapper.cs`, `GitHubClaimsMapper.cs` — 継承元の `OAuthClaimsMapperBase` が `abstract` のため、各実装クラスは `sealed` にできる

- ⚠️ **[Rule 15 違反: `record` 未使用]**: `ApiKeyContext.cs` (Line 9) — `sealed class` として正しく宣言されているが、`sealed record` にするとバリューイコールティと不変性がより強く保証される。`AuthUser.cs` や `OAuthUserInfo.cs` は `record` を使用しており、一貫性のために `ApiKeyContext` も `sealed record` に移行を推奨。

- ⚠️ **[Rule 14 準拠確認: 一ファイル一型]**: 全ファイルを確認した結果、各 `.cs` ファイルには単一の型のみが定義されており、Rule 14 に完全に準拠している。

- ⚠️ **[マジックストリング残存の可能性]**: `JwtAuthenticationService.cs` (Line 119) — `"preferred_username"` がハードコードされている。Rule 13（マジックストリング排除）に従い、`VKClaimTypes` または `OpenIdClaimTypes` 等のクラスで定数化することを推奨。

- ⚠️ **[TokenRevocationService の userId 引数の未使用]**: `TokenRevocationService.cs` (Line 42) — `RevokeUserTokensAsync` の `userId` パラメータがメソッド本体内で使用されていない（コメントでも「currently unused」と明記）。この引数は API の混乱を招く。将来的な用途が確定していない場合は削除し、インタフェースも更新することを推奨。

---

## ✅ 評価ポイント (Highlights / Good Practices)

- **`Result<T>` パターンの完全な適用**: `IAuthenticationService`, `IRefreshTokenValidator` の戻り値型として `Task<Result<T>>` が一貫して用いられ、null を返す箇所がない。Rule 1 に完全準拠。

- **二層のリボケーション設計**: ユーザーレベル（`IsUserRevokedAsync`）とトークンレベル（`IsRevokedAsync`）の両方でリボケーションチェックが行われており、強固なセキュリティアーキテクチャを実現。JwtBearerEvents での事前チェックと JwtAuthenticationService での二重チェックの組み合わせは特に評価できる。

- **`IApiKeyBlacklist` の分離**: バリデーション（`ApiKeyValidator`）とリボケーション（`IApiKeyBlacklist`）の责任が明確に分離されており、将来的な実装交換が容易。SRP を正しく体現している。

- **ソースジェネレーターによるテレメトリ自動化**: `[VKBlockDiagnostics]` ソースジェネレーターを使用した `AuthenticationDiagnostics.cs` は、ボイラープレートコードを排除する賢明な選択。コードの一貫性と保守性を高めている。

- **Polly なしでの Redis フォールバック設計**: `DistributedCacheApiKeyRateLimiter.cs` における Redis – 優先・IDistributedCache フォールバック設計は、インフラの回復性を向上させる実用的なアプローチ。フォールバック発生時に `LogWarning` でオペレーターを警告している点も適切。

- **RFC 7807 準拠のエラーレスポンス**: `AuthenticationResponseHelper.cs` により、認証エラーが一貫した `ProblemDetails` 形式で返却され、API コンシューマーの標準的な扱いを可能にする。

- **`IOptionsMonitor` による動的設定**: `JwtAuthenticationService` と `ApiKeyValidator` が `IOptionsMonitor<T>` を利用しており、アプリ再起動なしに設定変更を反映できる。

- **`AuthUser` / `OAuthUserInfo` の `record` 実装**: 不変の値オブジェクトとして `record` を正しく使用。イコールティベースの比較が自動的に提供される。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| 優先度  | 対象ファイル                                                                  | 改善内容                                      |
| ------- | ----------------------------------------------------------------------------- | --------------------------------------------- |
| 🔴 High | `ApiKeyValidator.cs:51`                                                       | `hashedKey[..8]` のログ出力をマスクまたは削除 |
| 🔴 High | `ApiKeyRecord.cs`                                                             | `sealed` 修飾子の追加 (Rule 15)               |
| 🔴 High | `VKClaimsTransformer.cs`                                                      | `sealed` 修飾子の追加 (Rule 15)               |
| 🔴 High | `AzureB2CClaimsMapper.cs` / `GoogleClaimsMapper.cs` / `GitHubClaimsMapper.cs` | `sealed` 修飾子の追加 (Rule 15)               |

### 2. リファクタリング提案 (Refactoring)

| 優先度    | 対象ファイル                           | 改善内容                                                                                           |
| --------- | -------------------------------------- | -------------------------------------------------------------------------------------------------- |
| 🟡 Medium | `ApiKeyAuthenticationHandler.cs`       | `ApiKeyValidator` を `IApiKeyValidator` インタフェース経由で注入するよう変更し、テスト容易性を向上 |
| 🟡 Medium | `ApiKeyContext.cs`                     | `sealed class` → `sealed record` に変更し、他のコントラクトとの一貫性を確保                        |
| 🟡 Medium | `TokenRevocationService.cs`            | 未使用の `userId` パラメータを削除し、インタフェースシグネチャを更新                               |
| 🟡 Medium | `JwtAuthenticationService.cs:119`      | `"preferred_username"` を `VKClaimTypes` または `OpenIdClaimTypes` 定数へ移行                      |
| 🟡 Medium | `JwtAuthenticationService.cs` 失敗パス | `activity?.SetTag("auth.failure.reason", ...)` を追加し、失敗トレースを強化                        |
| 🟢 Low    | `VKClaimsTransformer.cs`               | `IServiceScopeFactory` による内部スコープ生成をやめ、`IVKClaimsProvider` を直接インジェクション    |

### 3. 推奨される学習トピック (Learning Suggestions)

- **HMAC-SHA256 with Pepper (APIキー保護)**: 純粋な SHA-256 ハッシュから、ペッパー付き HMAC-SHA256 への移行を検討すること。.NET の `System.Security.Cryptography.HMACSHA256` で実装可能。
- **`Microsoft.AspNetCore.RateLimiting` (Built-in Rate Limiter, .NET 7+)**: カスタム `IApiKeyRateLimiter` の代替として、ASP.NET Core 組み込みのレートリミターポリシーの活用を検討。`FixedWindowRateLimiter` や `SlidingWindowRateLimiter` が利用可能。
- **Integration Testing with `WebApplicationFactory`**: `ApiKeyAuthenticationHandler` の認証フロー全体を検証するために、`Microsoft.AspNetCore.Mvc.Testing` パッケージを使用した統合テストの追加を推奨。

---

## 📈 スコア推移

| 監査日                 | スコア     | 主要改善点                                                                        |
| ---------------------- | ---------- | --------------------------------------------------------------------------------- |
| 2026-03-03 (初回)      | 45/100     | 基礎的な実装のみ                                                                  |
| 2026-03-05 (第2回)     | 72/100     | `ITokenBlacklist` 抽象化、`IApiKeyBlacklist` 導入、エラー定数集中化               |
| **2026-03-06 (第3回)** | **88/100** | Source Generator テレメトリ、`IApiKeyRateLimiter` 導入、RFC 7807 エラーレスポンス |
