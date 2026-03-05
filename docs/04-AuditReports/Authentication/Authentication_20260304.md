# 📊 アーキテクチャ監査レポート: Authentication Building Block

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 88/100点
- **対象レイヤー判定**: Cross-Cutting Concerns / Infrastructure Layer (Building Block)
- **総評 (Executive Summary)**:
  `VK.Blocks.Authentication` は、現代的な C# 12 の機能を活用した高品質な認証基盤モジュールである。`Result<T>` パターンによる例外フリーの制御フロー、Keyed DI による Strategy パターン、`stackalloc` を用いたホットパスの最適化など、シニアレベルの設計判断が随所に見られる。`audit_vk_blocks_code` ツールによる自動監査においても **Pass（VK.Blocks Rule 1 & Rule 3 完全準拠）** の判定を取得済み。一方で、`TokenRevocationService` のインターフェイス不在、リフレッシュトークン TTL のハードコード、レートリミッターの非原子的更新など、エンタープライズ運用スケールに向けた改善の余地が残されている。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[レートリミット — Get/Increment の非原子性]**: `ApiKeys/DistributedCacheApiKeyRateLimiter.cs`
    - `GetStringAsync` でカウントを取得し、その後 `SetStringAsync` でインクリメントする実装は、2つの操作の間に原子性がない。高並行アクセス下では複数のリクエストが同時に「カウント < リミット」を読み取り、リミットを超えて通過する **TOCTOU (Time-of-Check-Time-of-Use) 競合** が発生しうる。認証モジュールのレートリミットとして、セキュリティ保護の実効性に疑問が生じる重大な設計上のリスクである。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[セキュリティ — API キーブラックリストのキープレフィックス不統一]**: `ApiKeys/ApiKeyValidator.cs` (L51)
    - `var blacklistKey = $"apikey:{apiKey.Id}";` と生成しているが、`DistributedCacheTokenBlacklist` 内のキープレフィックスは `"revoked_jti:"` である。APIキーのブラックリストチェックは JTI ではなく Key ID に対して行われており、この責務の混在が将来の混乱を招くリスクがある。`IApiKeyBlacklist` として分離することが望ましい。

- 🔒 **[パフォーマンス — クレーム変換の N+1 リスク]**: `Claims/VKClaimsTransformer.cs`
    - `IClaimsTransformation` は認証済みリクエストごとに呼び出される。現在は `IVKClaimsProvider` が未登録の場合はスキップされるが、将来 DB アクセスが実装された際にキャッシュ機構なしでは N+1 的なDB呼び出しが頻発する。`IMemoryCache` または `IDistributedCache` ベースのキャッシュ層の設計を先行して行うべき。

- 🔒 **[堅牢性 — CancellationToken 非伝播]**: `Claims/VKClaimsTransformer.cs` (L45)
    - `GetUserClaimsAsync(userId, CancellationToken.None)` と呼び出しており、HTTP リクエストのキャンセルシグナルが伝播されていない。ASP.NET Core の `IClaimsTransformation` インターフェイス自体に `CancellationToken` がないためすぐに対処はできないが、将来的なインターフェイス拡張時の考慮点として認識しておくべき。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[テスト容易性 — 良好]**: 全体的に単体テストを意識した設計が徹底されている。`IAuthenticationService`、`IApiKeyStore`、`ITokenBlacklist`、`IApiKeyRateLimiter`、`IRefreshTokenValidator` など、すべての外部 I/O 依存コンポーネントがインターフェイスとして抽象化されており（DIP 準拠）、Mock/Stub による置換が容易。`TokenValidationParametersFactory` や `JwtBearerEventsFactory` を `static` クラスとして分離している点も、テストセットアップを簡素化するアプローチとして評価できる。

- ⚙️ **[テスト容易性 — 改善余地]**: `Security/TokenRevocationService.cs`
    - `sealed` でなく、かつ DI インターフェイスなしで `AddScoped<TokenRevocationService>()` として直接登録。これにより消費側のテスト時に `TokenRevocationService` 全体の具体実装が必要となり、モック化が困難。`ITokenRevocationService` インターフェイスの導入が必要。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[運用監視 — 良好]**: `ApiKeyValidator.cs` および `JwtBearerEventsFactory.cs` にて `ILogger` が適切に活用されており、構造化ログのプレースホルダ（`{Hash}`、`{KeyId}`）を正しく使用。生のAPIキーや秘密情報をログに残さずハッシュ値の先頭 8文字や ID のみを記録する実装は、セキュリティと可観測性のバランスが取れた優れた実践例である。

- 📡 **[エラーレスポンス — 部分的懸念]**: `ApiKeys/ApiKeyAuthenticationHandler.cs` (L77) および `Validation/JwtBearerEventsFactory.cs` (L66)
    - 認証失敗時のレスポンスがハードコードされた JSON 文字列 (`{"error": "..."}`) であり、RFC 7807 (Problem Details for HTTP APIs) に準拠していない。システム全体で `IProblemDetailsService` を採用している場合、エラーレスポンス形式の不一致がAPIクライアント側のエラーハンドリングを複雑化させる。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[ハードコード — リフレッシュトークン TTL]**: `Security/DistributedRefreshTokenValidator.cs` (L41)
    - `TimeSpan.FromDays(30)` がコード内に直接埋め込まれており、ポリシー変更時にコード修正とデプロイが必要になる。`VKAuthenticationOptions` に `RefreshTokenLifetimeDays` プロパティを追加し、`IOptionsMonitor<VKAuthenticationOptions>` 経由で注入する設計に変更すべき。

- ⚠️ **[YAGNI — `AuthResult` クラスが未完成]**: `Abstractions/Contracts/AuthResult.cs`
    - `Result` を継承しているが `Success` / `Failure` ファクトリメソッドが定義されておらず、コードベース内で実際に使用されていない。完成させて活用するか、不要であれば削除を検討（YAGNI 原則）。

- ⚠️ **[YAGNI — OAuth Mapper の実装が空]**: `OAuth/Mappers/{Google|GitHub|AzureB2C}ClaimsMapper.cs`
    - 各 Mapper クラスはベースクラスを継承し独自ロジックを持っているが、スキームとして使用するには OAuth ハンドラー側の実装（`AddGoogleAuthentication` 等）が DI に存在しない。Keyed DI デモとしての意味はあるが、実際に稼働しない機能が登録されている点はデッドコードリスクとなりうる。

---

## ✅ 評価ポイント (Highlights / Good Practices)

- **Result パターンによる例外フリー制御フロー**: `Result<T>` / `Error` を全面採用し、認証というホットパスで高コストな `throw` を排除。Railway-Oriented Programming の思想が一貫して実装されている。
- **`stackalloc` によるGC負荷最小化**: `ApiKeyValidator.HashApiKey` 内で堅牢な stackalloc ガード (`maxByteCount <= 256`) を設けつつヒープ割り当てを回避。認証頻出パスにおけるパフォーマンスへの配慮が優れている。
- **Keyed Services による Strategy パターンの現代的実現**: .NET 8 の `AddKeyedScoped<IOAuthClaimsMapper, T>("Provider")` を活用し、プロバイダー解決をコンパイル時に型安全に行う設計。
- **セキュリティの多層防衛**: JTI ブラックリスト（JTI 単位 + ユーザー単位）、トークンリプレイ検出（Token Family ID ベース）、固定ウィンドウレートリミットの3層が、それぞれ抽象化に基づいて疎結合に実装されている。
- **起動時設定検証**: `VKAuthenticationOptionsValidator` + `ValidateOnStart()` により、設定ミスを本番投入前に早期検出できる堅牢な起動シーケンスを確立している。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

1. **最優先対応 (Immediate Action)**:
    - `DistributedCacheApiKeyRateLimiter` の Get→Increment を原子的操作に切り替える。Redis を `IDistributedCache` の裏側で使用する場合は `INCR` + `EXPIREAT` による Lua スクリプトまたは `RedLock` パターンへ移行し、TOCTOU 競合を排除すること。

2. **リファクタリング提案 (Refactoring)**:
    - `TokenRevocationService` に `ITokenRevocationService` インターフェイスを導入し、DI 登録を `AddScoped<ITokenRevocationService, TokenRevocationService>()` に変更してテスタビリティを確保する。
    - `AuthenticateAsync` 失敗時のレスポンスを `IProblemDetailsService`（`Microsoft.AspNetCore.Http.Results.Problem()`）経由で RFC 7807 準拠に統一し、クライアント側エラーハンドリングを標準化する。
    - `DistributedRefreshTokenValidator` の TTL `TimeSpan.FromDays(30)` を `VKAuthenticationOptions.RefreshTokenLifetimeDays` に外部化する。
    - `ApiKeyValidator` における API キーのブラックリストキー生成 (`"apikey:{id}"`) を `IApiKeyBlacklist` としてブラックリスト実装から切り離し、責務を明確化する。

3. **推奨される学習トピック (Learning Suggestions)**:
    - **Distributed Locking & Atomic Counter**: Redis の `INCR`/`EXPIREAT` と `Lua Script` を用いた原子的レートリミット実装（StackExchange.Redis と `RedLock.net`）。
    - **RFC 7807 Problem Details**: .NET 8 の `IProblemDetailsService` を用いたミドルウェアレベルでの横断的エラーハンドリング標準化（`services.AddProblemDetails()`）。
    - **Options Pattern の深化**: `IOptionsMonitor<T>` / `IOptionsSnapshot<T>` の使い分けと、ホットリロード対応の設定設計。
