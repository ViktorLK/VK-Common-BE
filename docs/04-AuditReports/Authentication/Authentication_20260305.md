# 🏛️ アーキテクチャ監査レポート: Authentication Module

**モジュール**: `VK.Blocks.Authentication`
**監査日**: 2026-03-05
**監査対象**: `src/BuildingBlocks/Authentication` (40ファイル / 10サブディレクトリ)
**対象フレームワーク**: .NET 10 / C# 12+

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **88 / 100**
- **対象レイヤー判定**: Infrastructure Building Block (Cross-Cutting Authentication Concern)
- **総評 (Executive Summary)**:

本モジュールは、VK.Blocks アーキテクチャ規約に対して高い準拠率を示しており、成熟した設計を実現している。`Result<T>` パターンの一貫した適用、`AuthenticationErrors` による構造化エラー定数の集約、`ITokenBlacklist` / `IApiKeyBlacklist` / `ITokenRevocationService` といった抽象化による責務の分離は、設計品質の高さを反映している。`AuthenticationDiagnostics` による Source Generator ベースの可観測性統合も優れている。

一方で、いくつかの改善可能な領域が識別された。特に、`VKClaimsTransformer` における `CancellationToken.None` のハードコード、`DistributedCacheApiKeyRateLimiter` の Read-Then-Write 競合状態、および `HandleChallengeAsync` における RFC 7807 未準拠のエラーレスポンスは、対処が望ましい。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ レートリミッターの競合状態 (Race Condition)

**ファイル**: `ApiKeys/DistributedCacheApiKeyRateLimiter.cs` (L24–L41)

`IsAllowedAsync` メソッドは、分散キャッシュに対して Read-Then-Write（読み取り後に書き込み）の非アトミックな操作を実行している。高スループット環境では、複数のリクエストが同時にカウンターを読み取り、それぞれがインクリメントして書き込むため、設定されたレートリミットを超過するリクエストが通過する可能性がある。

```
// 現状: Read → Increment → Write (非アトミック)
var countStr = await cache.GetStringAsync(windowKey, ct);
var count = countStr != null && int.TryParse(countStr, out var c) ? c : 0;
if (count >= limitPerMinute) return false;
count++;
await cache.SetStringAsync(windowKey, count.ToString(), options, ct);
```

**推奨**: Redis の `INCR` コマンドを利用するか、`IDistributedCache` を超えた `IConnectionMultiplexer` (StackExchange.Redis) のアトミック操作を検討すべき。代替として、`Lua Script` によるアトミックなインクリメントと閾値チェックが最も堅牢な解決策となる。

**影響度**: 🔴 高 — DDoS やブルートフォース攻撃下でレートリミットが機能しない可能性がある。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 `CancellationToken.None` のハードコード

**ファイル**: `Claims/VKClaimsTransformer.cs` (L45)

```csharp
var dynamicClaims = await claimsProvider.GetUserClaimsAsync(userId, CancellationToken.None);
```

`IClaimsTransformation.TransformAsync` は `CancellationToken` を提供しないため、技術的な制約ではあるが、`HttpContext.RequestAborted` を `IHttpContextAccessor` 経由で取得し渡すオプションがある。**Rule 3 (Async)** の精神に基づき、可能な限り伝播が望ましい。

**影響度**: 🟡 中 — 長時間のデータベースクエリが実行される場合、リクエストがキャンセルされても処理が継続される。

---

### 🔒 RFC 7807 非準拠のエラーレスポンス

**ファイル**: `ApiKeys/ApiKeyAuthenticationHandler.cs` (L77)

```csharp
return Response.WriteAsync("""{"error":"API key is missing or invalid"}""");
```

**ファイル**: `Validation/JwtBearerEventsFactory.cs` (L66)

```csharp
var result = JsonSerializer.Serialize(new { error = "You are not authorized." });
```

これらのレスポンスは VK.Blocks 規約 (**Rule 1 — RFC 7807**) に準拠していない。`ProblemDetails` 形式 (`type`, `title`, `status`, `detail`, `traceId`) で返すべきである。

**影響度**: 🟡 中 — API クライアントがエラーレスポンスを統一的にパースできない。

---

### 🔒 対称鍵 (SymmetricSecurityKey) の使用

**ファイル**: `Factory/TokenValidationParametersFactory.cs` (L30)

```csharp
IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey))
```

HMAC ベースの対称鍵署名は、SharedSecret の漏洩リスクが高い。マイクロサービス環境やゼロトラスト環境では、非対称鍵 (RSA / ECDSA) を検討すべきである。ただし、これは現在の設計判断として許容される場合がある。

**影響度**: 🟡 中 — 鍵がコンフィグレーション経由で注入される場合、Azure Key Vault 等のシークレット管理との統合が前提。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ 高いテスト容易性

本モジュールは全体として優れたテスト容易性を持つ。

| 評価項目               | 状態 | 備考                                                                           |
| ---------------------- | ---- | ------------------------------------------------------------------------------ |
| DI 経由の依存注入      | ✅   | 全クラスでコンストラクタインジェクション適用                                   |
| インターフェース抽象化 | ✅   | `ITokenBlacklist`, `IApiKeyStore`, `IApiKeyBlacklist`, `IApiKeyRateLimiter` 等 |
| `new` の直接使用       | ✅   | `JwtSecurityTokenHandler` のみ (テスト時に `override` 可能な設計推奨)          |
| 静的メソッドの分離     | ✅   | `HashApiKey`, `MapToAuthUser` 等、副作用なしの純粋関数                         |

### ⚙️ 軽微な密結合

**ファイル**: `DependencyInjection/AuthenticationBlockExtensions.cs` (L59)

```csharp
options.Events = Validation.JwtBearerEventsFactory.CreateEvents();
```

`JwtBearerEventsFactory` は `internal static` クラスであり、DI コンテナ経由で注入されていない。カスタマイズが必要な場合は、インターフェース化と DI 登録が望ましいが、現時点ではビルディングブロック内部の実装詳細として許容範囲。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 優秀な統合

| 評価項目                                  | 状態 | 備考                                                                                                         |
| ----------------------------------------- | ---- | ------------------------------------------------------------------------------------------------------------ |
| `ActivitySource` (トレーシング)           | ✅   | `AuthenticationDiagnostics.Source.StartActivity()` で `ApiKeyValidator`, `JwtAuthenticationService` をカバー |
| `Meter` (メトリクス)                      | ✅   | `authentication.requests`, `authentication.rate_limit_exceeded` カウンターで計装済み                         |
| Source Generator (`[VKBlockDiagnostics]`) | ✅   | `AuthenticationDiagnostics` が partial class + Attribute で自動生成                                          |
| 構造化ログテンプレート                    | ✅   | `"{KeyId}"`, `"{Hash}"` 等のプレースホルダーを一貫して使用                                                   |
| `Result<T>` パターン                      | ✅   | `JwtAuthenticationService`, `ApiKeyValidator`, `DistributedRefreshTokenValidator` で一貫適用                 |
| TraceId 伝播                              | ⚠️   | エラーレスポンス (`HandleChallengeAsync`, `OnChallenge`) に `TraceId` が含まれていない (**Rule 6**)          |

**`TraceId` 不在の影響**: インシデント発生時に、401 レスポンスとサーバーサイドログの相関分析が困難になる。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ `UserSession` チェックの二重実行

**ファイル**: `Validation/JwtBearerEventsFactory.cs` (L31–L57) と `Services/JwtAuthenticationService.cs` (L71–L78)

JWT Bearer パイプラインの `OnTokenValidated` イベントにおけるブラックリストチェックと、`JwtAuthenticationService.AuthenticateAsync` 内のブラックリストチェックが冗長に存在する。

- `OnTokenValidated`: `IsUserRevokedAsync` + `IsRevokedAsync(jti)`
- `JwtAuthenticationService`: `IsRevokedAsync(jti)` のみ（ユーザーレベルのチェックなし）

このニ重チェックは意図的（多層防御）である可能性があるが、責務の所在が曖昧であり、保守性に影響を与える。どちらか一方に集約するか、ADR で明確な意図を記録すべきである。

---

### ⚠️ コメントアウトされたコード

**ファイル**: `DependencyInjection/AuthenticationBlockExtensions.cs` (L62, L69)

```csharp
// services.AddSingleton<ITokenProvider, JwtTokenProvider>();
// services.AddScoped<ApiKeyProvider>();
```

コメントアウトされたコードが残存している。将来的に必要であれば TODO として追跡し、不要であれば削除すべき（**Rule 10** の精神）。

---

### ⚠️ `AuthResult` クラスの未使用

**ファイル**: `Abstractions/Contracts/AuthResult.cs`

`AuthResult` クラスは定義されているが、モジュール内のどの箇所でも使用されていない。`IAuthenticationService.AuthenticateAsync` は `Result<AuthUser>` を返しており、`AuthResult` はデッドコードとなっている。削除またはリファクタリングを検討すべき。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### 1. 構造化エラー定数の完全な集約 (**Rule 1 + Rule 6**)

`AuthenticationErrors` クラスが `Jwt`, `RefreshToken`, `ApiKey` の全エラーケースを `static readonly Error` フィールドとしてネストされた静的クラスに整理。`Result.Failure<T>("raw string")` の使用はゼロ。

### 2. 堅牢なトークン失効アーキテクチャ

- `ITokenBlacklist` (JTI 単位 + ユーザー単位の失効)
- `IApiKeyBlacklist` (APIキー単位の失効)
- `ITokenRevocationService` (失効操作のファサード)
- `IRefreshTokenValidator` (トークンローテーション検証 / リプレイ攻撃検知)

これらのインターフェースが明確な責務を持ち、`DistributedCache` を基盤とした実装で一貫している。

### 3. DIP (依存性逆転原則) の厳密な適用 (**Rule 2**)

Core/Abstractions レイヤーは `VK.Blocks.Core.Results` 以外のインフラストラクチャ依存を持たない。`IDistributedCache`, `IOptionsMonitor`, `ILogger` 等はすべて実装クラスでのみ参照されている。

### 4. Source Generator ベースの計装 (**Rule 6**)

`[VKBlockDiagnostics("VK.Blocks.Authentication")]` 属性による `ActivitySource` および `Meter` の自動生成は、ボイラープレートを排除し、計装漏れを防ぐ優れたアプローチ。

### 5. パフォーマンス意識の高い実装 (**Rule 4**)

`ApiKeyValidator.HashApiKey` における `stackalloc` の使用は、頻繁呼び出しのホットパスでのヒープアロケーション回避として適切。

### 6. コンフィグレーション検証 (`ValidateOnStart`)

`VKAuthenticationOptionsValidator` により、起動時にコンフィグレーションエラーを早期検出。Fail-Fast 原則の実践。

### 7. キーベースの DI 登録 (Keyed Services)

`AddKeyedScoped<IOAuthClaimsMapper>` によるプロバイダー別 Claims Mapper の登録は、Strategy パターンの Clean な実装。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| #   | 課題                                                                         | 対象ファイル                                                  | 重要度 |
| --- | ---------------------------------------------------------------------------- | ------------------------------------------------------------- | ------ |
| 1   | **レートリミッターのアトミック化**: Read-Then-Write を Redis `INCR` 等に置換 | `DistributedCacheApiKeyRateLimiter.cs`                        | 🔴 高  |
| 2   | **RFC 7807 準拠エラーレスポンス**: `ProblemDetails` 形式 + `TraceId` 付与    | `ApiKeyAuthenticationHandler.cs`, `JwtBearerEventsFactory.cs` | 🟡 中  |
| 3   | **デッドコード削除**: `AuthResult.cs` とコメントアウト行の除去               | `AuthResult.cs`, `AuthenticationBlockExtensions.cs`           | 🟢 低  |

### 2. リファクタリング提案 (Refactoring)

| #   | 提案                                   | 詳細                                                                                                                                                           |
| --- | -------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **ブラックリストチェックの責務一元化** | `JwtBearerEventsFactory.OnTokenValidated` と `JwtAuthenticationService.AuthenticateAsync` のブラックリストチェック重複を解消。ADR でどちらに集約するかを決定。 |
| 2   | **`CancellationToken` の伝播改善**     | `VKClaimsTransformer` で `IHttpContextAccessor.HttpContext.RequestAborted` を利用可能に。                                                                      |
| 3   | **非対称鍵サポートの追加**             | `TokenValidationParametersFactory` で RSA/ECDSA 署名鍵の選択肢を設けるオプション追加。`JwtValidationOptions` にキータイプ設定を追加。                          |

### 3. 推奨される学習トピック (Learning Suggestions)

| #   | トピック                                 | 概要                                                                                     |
| --- | ---------------------------------------- | ---------------------------------------------------------------------------------------- |
| 1   | **Redis Lua Scripting**                  | アトミックなレートリミットカウンター実装のため、`EVAL` コマンドと Lua スクリプトの理解。 |
| 2   | **RFC 7807 (Problem Details)**           | ASP.NET Core の `IProblemDetailsService` と `ProblemDetailsOptions` の活用方法。         |
| 3   | **Asymmetric JWT Signing (RS256/ES256)** | マイクロサービス間での公開鍵検証アーキテクチャ。JWKS エンドポイント運用。                |

---

## VK.Blocks ルール準拠サマリー

| ルール                      | 状態 | 所見                                                                                                           |
| --------------------------- | ---- | -------------------------------------------------------------------------------------------------------------- |
| Rule 1 — Result Pattern     | ✅   | `Result<T>` + `AuthenticationErrors` 定数を一貫使用。RFC 7807 はエラーレスポンス部分のみ未準拠。               |
| Rule 2 — Layer Dependencies | ✅   | Abstractions レイヤーにインフラ依存なし。                                                                      |
| Rule 3 — Async/CT           | ⚠️   | `VKClaimsTransformer` で `CancellationToken.None` がハードコードされている。                                   |
| Rule 4 — Performance        | ✅   | `stackalloc` 活用。ループ内 DB クエリなし。                                                                    |
| Rule 5 — Automation         | ✅   | 該当なし（本モジュールに `IAuditable` / `ISoftDelete` 対象なし）。                                             |
| Rule 6 — Observability      | ⚠️   | 構造化ログ・メトリクス・トレーシングは優秀。エラーレスポンスに `TraceId` 未付与。                              |
| Rule 7 — Security           | ✅   | `TenantId` は `IMultiTenant` / クレーム経由で伝播。グローバルクエリフィルターは DB 層の責務。                  |
| Rule 8 — Resiliency         | ✅   | 該当なし（外部 HTTP 呼び出しなし。`IDistributedCache` は SDK 組み込みのリトライを前提）。                      |
| Rule 9 — Testing            | ⚠️   | テスト容易性は高いが、テストファイルが本モジュール内に存在しない（別プロジェクトで管理されている可能性あり）。 |
| Rule 10 — Code Generation   | ⚠️   | コメントアウト行 (L62, L69) および未使用クラス `AuthResult` が残存。                                           |

---

_本レポートは VK.Blocks Architecture Audit Guidelines (v1.0) に基づき自動生成されました。_
