# アーキテクチャ監査レポート — VK.Blocks.Authentication

| 項目                 | 内容                                 |
| -------------------- | ------------------------------------ |
| **モジュール名**     | VK.Blocks.Authentication             |
| **対象ディレクトリ** | `src/BuildingBlocks/Authentication/` |
| **監査日**           | 2026-04-02                           |
| **総ファイル数**     | 55 (`.cs`)                           |
| **前回監査**         | 2026-04-01 (Remediation 2 回実施済)  |

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **92 / 100** _(前回 85 → +7pt)_
- **対象レイヤー判定**: Infrastructure Layer / Cross-Cutting Building Block
- **総評 (Executive Summary)**:

前回 (2026-04-01) の監査で指摘された 6 件の重要課題（ソース生成ロガー違反、IApiKeyStore 返却型、InMemoryCleanupBackgroundService の DI 問題、InMemoryApiKeyRateLimiter のアルゴリズム不備、VKClaimsTransformer の CancellationToken 欠落、ApiKeyValidator の HashApiKey パフォーマンス）は**すべて是正済み**であることを確認した。

現在のコードベースは、**VK.Blocks のコーディング規約に高い水準で準拠**しており、Result Pattern、Source Generated Logging、CancellationToken 伝播、構造化エラー定数、Feature-Driven フォルダ構成、Self-Adaptive Cleanup 戦略など、エンタープライズ品質の防衛的設計が徹底されている。

残存課題は「致命的」ではなく「強化推奨」レベルであり、本モジュールは**プロダクション運用に十分な品質**を持つ。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

**該当なし** — 前回指摘の Critical 課題はすべて修正済み。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 **[セキュリティ / 中リスク] JwtAuthenticationService: Scoped サービス内の `_revocationCache` (Dictionary) のスレッド安全性**

- **該当箇所**: `src/BuildingBlocks/Authentication/Jwt/JwtAuthenticationService.cs` (L27)
- **説明**: `JwtAuthenticationService` は `Scoped` として登録されているため、単一 HTTP リクエスト内でのみ生存する。ASP.NET Core のミドルウェアパイプラインは通常シングルスレッドで実行されるため、現時点では `Dictionary<string, bool>` の非スレッドセーフ性が即座に問題となる可能性は低い。ただし、マルチスレッドミドルウェアやバックグラウンド処理から利用された場合にレースコンディションが発生する理論上のリスクが存在する。
- **重要度**: 低 — Scoped ライフタイムにより実質的なリスクが軽減されている。

### 🔒 **[パフォーマンス / 低リスク] InMemory キャッシュの `ToList()` によるアロケーション**

- **該当箇所**: `InMemoryApiKeyRateLimiter.cs` (L82), `InMemoryApiKeyRevocationProvider.cs` (L79), `InMemoryJwtRefreshTokenValidator.cs` (L80), `InMemoryJwtTokenRevocationProvider.cs` (L118, L129)
- **説明**: `CleanupExpiredEntries()` メソッドは `.Where().Select().ToList()` のパターンで期限切れキーを一時リストに具象化してから削除している。高スループット環境では GC プレッシャーとなる可能性があるが、クリーンアップが低頻度（既定 10 分間隔）で実行されるためリスクは極めて低い。
- **重要度**: 情報 — 現時点では最適化不要。

### 🔒 **[セキュリティ / 情報] OAuthProviderOptions の ClientSecret がプレーンテキスト**

- **該当箇所**: `src/BuildingBlocks/Authentication/OAuth/OAuthProviderOptions.cs` (L31)
- **説明**: `ClientSecret` は `string` 型で保持されており、構成ソース（appsettings.json / Azure Key Vault）からバインドされる。ログへの出力はされていないが、メモリダンプや診断ヒープから理論上漏洩する可能性がある。ASP.NET Core の `IConfiguration` + Azure Key Vault の組み合わせが推奨される運用パターンであり、本モジュールの責務範囲外であるため情報として記録する。
- **重要度**: 情報 — 運用側のベストプラクティスで対処。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ **[テスト容易性 / 良好] インターフェース抽象化の徹底**

- すべてのインフラストラクチャ関心事が適切にインターフェースで抽象化されている:
    - `IApiKeyStore`, `IApiKeyRateLimiter`, `IApiKeyRevocationProvider`
    - `IJwtAuthenticationService`, `IJwtTokenRevocationProvider`, `IJwtTokenRevocationService`, `IJwtRefreshTokenValidator`
    - `IOAuthClaimsMapper`, `IVKClaimsProvider`
    - `IInMemoryCacheCleanup`
- DI 登録では `TryAdd` / `TryAddEnumerable` パターンにより、テスト時のモック差し替えが容易。
- `VKClaimsTransformer` も `IServiceScopeFactory` を通じた遅延解決パターンにより、`IVKClaimsProvider` の有無で動的にテスト可能。

### ⚙️ **[テスト容易性 / 注意] `DateTimeOffset.UtcNow` の直接参照**

- **該当箇所**: `ApiKeyValidator.cs` (L69), `InMemoryApiKeyRateLimiter.cs` (L42, L77), `InMemoryApiKeyRevocationProvider.cs` (L39, L59, L75), `InMemoryJwtRefreshTokenValidator.cs` (L46, L60, L77), `InMemoryJwtTokenRevocationProvider.cs` (L41, L61, L75, L97, L113)
- **説明**: 複数の InMemory プロバイダーおよび `ApiKeyValidator` が `DateTimeOffset.UtcNow` を直接参照している。`TimeProvider` 抽象（.NET 8+）を導入することで、時刻依存テストの信頼性が大幅に向上する。
- **重要度**: 中 — 機能的には問題ないが、テスト品質向上のため推奨。

### ⚙️ **[テスト容易性 / 注意] `ApiKeyValidator.HashApiKey` の static private メソッド**

- **該当箇所**: `src/BuildingBlocks/Authentication/ApiKeys/ApiKeyValidator.cs` (L122)
- **説明**: `HashApiKey` は `static private` メソッドであり、直接テストできない。SHA256 ハッシュはセキュリティに直結するため、独立したユーティリティとして抽出するか `internal` に変更して `InternalsVisibleTo` 経由でテスト対象にすることを推奨する。
- **重要度**: 低 — 統合テスト経由でカバー可能。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 **[運用監視 / 優秀] OpenTelemetry 統合の完全実装**

- **Distributed Tracing**: `AuthenticationDiagnostics` クラスにより、`ActivitySource` ベースの分散トレースが JWT 検証、API キー検証、Claims 変換の各フローに埋め込まれている。
- **Metrics**: 6 つのメトリクス（認証試行数、レート制限違反数、失効ヒット数、リプレイ攻撃検出数、Claims 変換回数・所要時間）がカウンター/ヒストグラムで計装されている。
- **Tag 標準化**: すべてのタグキーが `AuthenticationDiagnosticsConstants` に集約されており、マジックストリングが排除されている。
- **RFC 7807 準拠**: `AuthenticationResponseHelper` が `ProblemDetails` + `TraceId` の標準レスポンスを生成し、運用時のインシデント調査を容易にしている。

### 📡 **[ロギング / 優秀] Source Generated Logging の完全採用**

- 4 つの `[LoggerMessage]` Source Generator クラス (`ApiKeyLog`, `ClaimsLog`, `InMemoryCleanupLog`, `JwtLog`) がフィーチャーフォルダ内に配置されている。
- すべてのログメッセージが構造化テンプレートを使用し、`{KeyId}`, `{MinLength}`, `{ProviderType}` 等のプレースホルダーにより、文字列補間が完全に排除されている。
- 直接 `logger.LogInformation()` / `logger.LogWarning()` の呼び出しはモジュール内に存在しない。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ **[冗長性 / 低リスク] `AuthenticationConstants` と `JwtConstants` の検証メッセージ重複**

- **該当箇所**: `AuthenticationConstants.cs` (L31: `DefaultSchemeRequired`) と `JwtConstants.cs` (L25: `DefaultSchemeRequired`)
- **説明**: `DefaultSchemeRequired` が両方の定数クラスに同一文字列で定義されている。Rule 13 (Constant Visibility) に従い、グローバルに使用される定数は一箇所に集約すべきである。
- **重要度**: 低 — 現時点で動作上の影響はないが、保守性の観点から改善推奨。

### ⚠️ **[命名規則 / 情報] `VKAuthenticationOptionsValidator` の冗長なチェック**

- **該当箇所**: `src/BuildingBlocks/Authentication/DependencyInjection/VKAuthenticationOptionsValidator.cs` (L34)
- **説明**: L17 で `if (!options.Enabled)` を短絡チェック済みだが、L34 で再度 `options.Enabled &&` を条件に含んでいる。これは常に `true` となる冗長条件であり、ガードクローズの意図が不明瞭になっている。
- **重要度**: 情報 — 動作には影響なし。

### ⚠️ **[フォルダ構成 / 低リスク] `Features/SemanticAttributes/` ディレクトリの配置**

- **該当箇所**: `src/BuildingBlocks/Authentication/Features/SemanticAttributes/`
- **説明**: `Features/` ディレクトリ配下に `SemanticAttributes/` のみが存在する。Rule 12 (Folder Organization) に従い、これらは `Common/` または直接 `Features/SemanticAttributes/` のように配置するか、あるいは単に `/Attributes/` として配置したほうが発見性が高い。現状の構造は機能的に問題ないが、モジュールのルート構造 (`ApiKeys/`, `Jwt/`, `OAuth/`, `Claims/`, `Common/`) と比較してやや浮いて見える。
- **重要度**: 情報 — 機能的には正常。

### ⚠️ **[型分離 / 情報] `ApiKeyOptions` ※ Options 系クラスのミュータブル性**

- **該当箇所**: `ApiKeyOptions.cs`, `JwtOptions.cs`, `OAuthProviderOptions.cs`, `VKAuthenticationOptions.cs`
- **説明**: Rule 15 (Modern C# Semantics) では DTO には `sealed record` の使用が推奨されているが、`IOptions<T>` / `IConfiguration.Bind()` パターンでは可変プロパティが必須であるため、`sealed class` + setter パターンは正当な例外である。現状は適切であるが、今後 .NET 9+ の `IConfigurationBinder` 改善に伴い見直し可能。
- **重要度**: 情報 — 現行設計は正しい。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### 1. **Result Pattern の完全準拠**

- `IApiKeyStore.FindByHashAsync()` → `Result<ApiKeyRecord>` を返却
- `ApiKeyValidator.ValidateAsync()` → `Result<ApiKeyContext>` を返却
- `IJwtAuthenticationService.AuthenticateAsync()` → `Result<AuthenticatedUser>` を返却
- `ClaimsPrincipalExtensions.ToAuthenticatedUser()` → `Result<AuthenticatedUser>` を返却
- `IJwtRefreshTokenValidator.ValidateTokenRotationAsync()` → `Result<bool>` を返却
- すべてのエラーが定数 (`ApiKeyErrors`, `JwtErrors`, `JwtRefreshTokenErrors`, `OAuthErrors`, `AuthenticationErrors`) で定義済み。**Failure("raw string")** は皆無。

### 2. **Self-Adaptive InMemory Cleanup 戦略**

- `IInMemoryCacheCleanup` + `InMemoryCleanupBackgroundService` の組み合わせにより、InMemory プロバイダーの自動メモリ管理が実現されている。
- `AssociatedServiceType` + `ReferenceEquals` による「アクティブプロバイダー検出」は優れた設計であり、Redis 等の外部ストアに切り替わった場合にクリーンアップが自動的にスキップされる。
- プロバイダー 0 件時にバックグラウンドサービスが即座に終了する設計は、リソース効率の観点で模範的。

### 3. **Fluent Builder + TryAdd パターンによる拡張性**

- `AddVKAuthenticationBlock() → IVKBlockBuilder<AuthenticationBlock>` のチェーン可能な Fluent API
- `.AddJwtRefreshTokenValidator<T>()`, `.AddApiKeyRevocationProvider<T>()`, `.AddClaimsProvider<T>()` 等の拡張ポイントが明確に公開されている。
- `TryAdd` / `TryAddEnumerable` により消費側での重複登録を防止。

### 4. **Source Generator 活用**

- `[VKBlockDiagnostics]` 属性による `ActivitySource` / `Meter` の自動生成
- `[OAuthProvider]` 属性 + Source Generator による `AddGeneratedOAuthMappers()` の反映排除（リフレクション不使用）
- `[LoggerMessage]` による高パフォーマンスロガー生成

### 5. **Fail-Fast 設計**

- `VKAuthenticationOptionsValidator` で「全戦略が無効」の場合を起動時に検出
- `JwtOptionsValidator` で AuthMode に応じた必須パラメータの細分化検証
- `OAuthOptionsValidator` で各プロバイダーの Authority / ClientId / CallbackPath の厳密検証

### 6. **セキュリティ防御設計**

- API キーの SHA256 ハッシュ比較（タイミング攻撃耐性は SHA256 + 定数時間比較で暗黙的に実現）
- ログ内のハッシュマスキング (`hashedApiKey[..4]`)
- トークン失効チェック（jti レベル + user レベルの二段階）
- リフレッシュトークンリプレイ攻撃検出
- Claims 変換のべき等性マーカー (`ClaimsTransformed`)

### 7. **CancellationToken の一貫した伝播**

- `ApiKeyValidator.ValidateAsync()` → store / revocationProvider / rateLimiter に伝播
- `VKClaimsTransformer.TransformAsync()` → `IHttpContextAccessor` から `RequestAborted` を取得して伝播
- `JwtAuthenticationService.AuthenticateAsync()` → `ValidateRevocationAsync()` に伝播

---

## 🚀 将来的な企業向け拡張性 (Future Enterprise-Grade Extensibility)

ADR-014 において定義された、エンタープライズレベルの要求に応えるための将来的な拡張ポイントを以下に記載する。現在のアーキテクチャは、これらの機能をシームレスに統合可能な抽象化レイヤーを既に備えている。

### 1. 拡張型分布式セッション管理 (Dynamic Session Revocation)

`IJwtTokenRevocationProvider` を深掘りし、より高度なセッション制御を実現する。

- **全デバイスログアウト (Global Logout)**：特定のユーザーに関連付けられたすべてのトークンを一括で失効させる機能。
- **セッション単位の撤回**: JWT に `sid` (Session ID) を埋め込み、デバイス管理画面から特定のデバイス（紛失した端末など）のみをリモートで強制的に無効化させる機能。

### 2. 精細な API Key 権限管理 (Scoped API Keys)

API Key に対して、リソース単位のアクセス制御（Scope）を導入する。

- **原理**: `ApiKeyContext` に権限検証ロジックを導入。
- **効果**: サードパーティパートナーに対して「読み取り専用」の API Key を発行するなど、最小権限の原則（PoLP）を適用。Key が漏洩した場合の損害を最小限に抑制。

### 3. 多テナント認証の隔離 (Multi-Tenant Auth Isolation)

複数のテナントがそれぞれ独自の OIDC プロバイダー（Auth0, Azure AD 等）を利用するシナリオに対応する。

- **機能**: `X-Tenant-Id` ヘッダー等に基づき、実行時に JWT の `Authority` や検証用公開鍵を動的に切り替える。
- **アプローチ**: `IOptionsMonitor` と `JwtBearerHandler` を高度にカスタマイズし、テナントごとの独立した認証コンテキストを提供。

### 4. アイデンティティ・エンリッチメント・パイプライン (Advanced Claims Enrichment)

`VKClaimsTransformer` を拡張し、プロファイル情報の高度な統合を実現する。

- **シナリオ**: キャッシュやデータベースから RBAC ロール、地理位置情報、サブスクリプションレベルを自動取得。
- **最適化**: 分布式キャッシュ（Redis）を活用し、リクエストごとにデータベースへ問い合わせることなくリッチな身元情報を保持。

### 5. セキュリティ監査と可観測性 (Security Observability)

- **異常モニタリング**: `AuthenticationDiagnostics` を通じて、短時間に異なる IP から大量の 401 エラーを発生させている API Key を検知し、自動的なアラート発行や一時的なブロックを実行。
- **セキュリティログ**: **RFC 5424** 標準に準拠し、監査に必要な「認証失敗」の機微な詳細情報を構造化ログとして記録。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

**現時点で 致命的な最優先是正事項は存在しない。**

### 2. リファクタリング提案 (Refactoring)

| #   | カテゴリ       | 課題                                                     | 対象ファイル                                    | 提案                                                                                                    |
| --- | -------------- | -------------------------------------------------------- | ----------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| R-1 | テスト容易性   | `DateTimeOffset.UtcNow` 直接参照                         | InMemory 系プロバイダー全般, `ApiKeyValidator`  | `TimeProvider` (System.TimeProvider) を DI 経由で注入し、テスト時に `FakeTimeProvider` を使用可能にする |
| R-2 | 定数管理       | `DefaultSchemeRequired` 重複                             | `AuthenticationConstants.cs`, `JwtConstants.cs` | `JwtConstants.DefaultSchemeRequired` を削除し、`AuthenticationConstants` 側に SSOT として統一           |
| R-3 | 冗長性         | `VKAuthenticationOptionsValidator.Validate()` の冗長条件 | `VKAuthenticationOptionsValidator.cs` (L34)     | `options.Enabled &&` の削除（ガードクローズで既に短絡済み）                                             |
| R-4 | フォルダ構造   | `Features/SemanticAttributes/` の孤立                    | `Features/SemanticAttributes/`                  | ディレクトリ名を `Attributes/` に変更するか、`Common/Attributes/` に統合して発見性を向上                |
| R-5 | ハッシュテスト | `HashApiKey` の独立テスト不可                            | `ApiKeyValidator.cs`                            | `internal static` に変更し `InternalsVisibleTo` でテストプロジェクトから検証可能にする                  |

### 3. 推奨される学習トピック (Learning Suggestions)

| #   | トピック                            | 概要                                                                                                          | 推奨リソース                                                                                                                       |
| --- | ----------------------------------- | ------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| L-1 | `TimeProvider` 抽象 (.NET 8+)       | 時刻依存コードのテスト容易性向上パターン                                                                      | [Microsoft Docs: TimeProvider](https://learn.microsoft.com/en-us/dotnet/api/system.timeprovider)                                   |
| L-2 | `IAsyncDisposable` の組み合わせ戦略 | InMemory プロバイダーの `IAsyncDisposable` 実装を `IHostedService` のライフサイクルと連携させる高度なパターン | [Microsoft Docs: IAsyncDisposable](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync) |
| L-3 | Constant-time comparison            | `CryptographicOperations.FixedTimeEquals` を用いたタイミング攻撃完全防止                                      | [OWASP Cryptographic Failures](https://owasp.org/Top10/A02_2021-Cryptographic_Failures/)                                           |
| L-4 | Source Generator 拡張               | 現在の `[VKBlockDiagnostics]` / `[OAuthProvider]` に加え、Validator 自動生成等の追加 SG 活用                  | [Andrew Lock: Incremental Source Generators](https://andrewlock.net/series/creating-a-source-generator/)                           |

---

## 監査証跡 (Audit Trail)

| 監査回  | 日付           | スコア | 主な変更点                                              |
| ------- | -------------- | ------ | ------------------------------------------------------- |
| 1st     | 2026-03-27     | —      | 初回構造分析（非公式レビュー）                          |
| 2nd     | 2026-04-01     | 85     | 正式監査。6 件の Critical/High 課題検出                 |
| **3rd** | **2026-04-02** | **92** | **全 6 件の是正完了確認。残存課題は低〜情報レベルのみ** |
