# ADR 003: Consolidate Token Blacklist Check Responsibility

**Date**: 2026-03-03  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: `src/BuildingBlocks/Authentication`

## 1. Context (背景)

ADR 001（デッドコード除去）での基盤クリーンアップを前提として、本 ADR は認証パイプラインにおけるトークンブラックリストチェックの責務の重複に関する設計決定を記録する。

現在の実装では、JWT トークンの失効チェックが **2箇所** で独立に実行されている。

## 2. Problem Statement (問題定義)

### 二重チェックの現状

#### チェック箇所 1: `JwtBearerEventsFactory.OnTokenValidated`

```csharp
// Validation/JwtBearerEventsFactory.cs (L29-57)
OnTokenValidated = async context =>
{
    var blacklist = context.HttpContext.RequestServices.GetService<ITokenBlacklist>();
    if (blacklist != null)
    {
        // ① ユーザーセッション単位の失効チェック
        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId) && await blacklist.IsUserRevokedAsync(userId, ...))
        {
            context.Fail("User session has been revoked.");
            return;
        }

        // ② JTI 単位の失効チェック
        var jti = context.Principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (!string.IsNullOrEmpty(jti) && await blacklist.IsRevokedAsync(jti, ...))
        {
            context.Fail("Token has been revoked.");
        }
    }
};
```

#### チェック箇所 2: `JwtAuthenticationService.AuthenticateAsync`

```csharp
// Services/JwtAuthenticationService.cs (L71-78)
if (jwtToken.Id is { Length: > 0 } jti)
{
    if (await blacklist.IsRevokedAsync(jti, cancellationToken).ConfigureAwait(false))
    {
        return Result.Failure<AuthenticatedUser>(AuthenticationErrors.Jwt.Revoked);
    }
}
// 注意: ユーザーレベルの IsUserRevokedAsync チェックは実施していない
```

### 問題の本質

| 観点                               | 詳細                                                                                             |
| ---------------------------------- | ------------------------------------------------------------------------------------------------ |
| **責務の曖昧さ**                   | どちらが「正規の」失効チェックの責務を持つのか不明確                                             |
| **ユーザーレベルチェックの不一致** | `OnTokenValidated` は `IsUserRevokedAsync` を実行するが、`JwtAuthenticationService` は実行しない |
| **パフォーマンス**                 | 同一リクエスト内で `IDistributedCache` に対して冗長な I/O が発生                                 |
| **保守性**                         | 失効ロジックの変更時に2箇所の同期が必要                                                          |

## 3. Decision (決定事項)

**`JwtBearerEventsFactory.OnTokenValidated` を唯一のブラックリストチェック実行ポイントとし、`JwtAuthenticationService.AuthenticateAsync` からの重複チェックを削除する。**

### 理由

1. **ASP.NET Core パイプラインの自然な拡張ポイント**: `OnTokenValidated` は JWT 検証パイプラインの標準的なフックポイントであり、ASP.NET Core ミドルウェアの責務として適切。
2. **ユーザーレベルチェックの包含**: `OnTokenValidated` は `IsUserRevokedAsync` と `IsRevokedAsync` の両方を持ち、より包括的。
3. **`JwtAuthenticationService` の責務簡素化**: `AuthenticateAsync` はトークンの暗号検証と `AuthenticatedUser` マッピングに専念し、インフラストラクチャ横断の関心事（キャッシュアクセス）から解放される。

### 変更内容

```diff
// Services/JwtAuthenticationService.cs
  var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

  if (validatedToken is not JwtSecurityToken jwtToken)
  {
      return Result.Failure<AuthenticatedUser>(AuthenticationErrors.Jwt.InvalidFormat);
  }

- // Explicitly check for revocation
- if (jwtToken.Id is { Length: > 0 } jti)
- {
-     if (await blacklist.IsRevokedAsync(jti, cancellationToken).ConfigureAwait(false))
-     {
-         return Result.Failure<AuthenticatedUser>(AuthenticationErrors.Jwt.Revoked);
-     }
- }

  AuthenticationDiagnostics.RecordAuthAttempt("jwt", true);
```

`JwtAuthenticationService` のコンストラクタから `ITokenBlacklist` 依存も削除する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: `JwtAuthenticationService` に一元化（`OnTokenValidated` を削除）

- **Approach**: `OnTokenValidated` のブラックリストチェックを削除し、`JwtAuthenticationService.AuthenticateAsync` に `IsUserRevokedAsync` を追加して一本化。
- **Rejected Reason**: `JwtAuthenticationService` は `IAuthenticationService` インターフェースの実装であり、ASP.NET Core の認証パイプラインと独立して使用される可能性がある。一方、`OnTokenValidated` は HTTP リクエストパイプラインに組み込まれるため、Web API 経由の全アクセスに対して確実にチェックを適用できる。`JwtAuthenticationService` 単独では、ミドルウェア経由でのみ呼ばれるという保証がないため、多層防御の意味で不十分。

### Option 2: 両方にチェックを残す（多層防御 / Defense-in-Depth）

- **Approach**: 冗長性を許容し、2箇所でチェックを維持する。
- **Rejected Reason**: 同一リクエスト内で `IDistributedCache` に対して2-3回の不要な I/O が発生する。また、ロジックの同期維持コストが大きく、一方の変更漏れがセキュリティホールになるリスクがある。Defense-in-Depth は本来異なるレイヤー・異なるメカニズムでの防御を指し、同一キャッシュへの同一クエリの重複は「防御の多層化」とは言えない。

### Option 3: デコレーターパターンで共通化

- **Approach**: ブラックリストチェックを `IAuthenticationService` のデコレーターとして実装し、`JwtAuthenticationService` の前段に挟む。
- **Rejected Reason**: 既に `OnTokenValidated` が同等の機能を提供しており、デコレーターの導入は過剰設計。加えて、HTTP パイプライン外での `IAuthenticationService` 使用ケースが現時点で存在しないため、YAGNI 原則に反する。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 失効チェックの責務が単一箇所に集約され、保守性が向上。
    - `JwtAuthenticationService` から `ITokenBlacklist` 依存が除去され、コンストラクタが簡素化。
    - 分散キャッシュへの冗長な I/O が排除され、パフォーマンス改善。
- **Negative**:
    - `JwtAuthenticationService.AuthenticateAsync` を HTTP パイプライン経由以外でスタンドアロンに使用する場合、ブラックリストチェックが行われない。
- **Mitigation**:
    - `JwtAuthenticationService` の XML ドキュメントに「トークン失効チェックは ASP.NET Core 認証パイプライン (`OnTokenValidated`) で実施される」旨を明記。
    - 将来スタンドアロン使用が必要になった場合は、デコレーターパターンの導入を再検討（ADR として追加）。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **影響範囲**: `JwtAuthenticationService.cs`（コンストラクタからの `ITokenBlacklist` 除去 + ブラックリストチェック削除）。
- **セキュリティ考慮**:
    - `OnTokenValidated` は ASP.NET Core の認証ミドルウェアがトークンを暗号的に検証した **後** に呼び出されるため、検証済みトークンに対してのみブラックリストチェックが実行される。セキュリティ上のリグレッションリスクはない。
    - `[Authorize]` 属性を持つ全エンドポイントに対して自動的に適用されるため、チェック漏れのリスクはゼロ。
- **検証**: 既存の統合テスト（失効トークンでの 401 レスポンス検証）がパスすることを確認。
