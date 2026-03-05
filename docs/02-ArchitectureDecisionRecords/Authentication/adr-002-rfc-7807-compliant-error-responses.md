# ADR 002: RFC 7807 Compliant Error Responses in Authentication Pipeline

**Date**: 2026-03-03  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: `src/BuildingBlocks/Authentication`

## 1. Context (背景)

VK.Blocks の全体規約 (Rule 1) では、HTTP エラーレスポンスに対して RFC 7807 (Problem Details for HTTP APIs) 形式の統一フォーマットを要求している。また Rule 6 では、すべてのエラーレスポンスに `TraceId` を含めることが義務付けられている。

2026年3月のアーキテクチャ監査において、Authentication パイプラインの2箇所で RFC 7807 非準拠のエラーレスポンスが発見された。

## 2. Problem Statement (問題定義)

### 問題箇所 1: `ApiKeyAuthenticationHandler.HandleChallengeAsync`

```csharp
// ApiKeys/ApiKeyAuthenticationHandler.cs (L73-78)
protected override Task HandleChallengeAsync(AuthenticationProperties properties)
{
    Response.StatusCode = StatusCodes.Status401Unauthorized;
    Response.ContentType = "application/json";
    return Response.WriteAsync("""{"error":"API key is missing or invalid"}""");
}
```

### 問題箇所 2: `JwtBearerEventsFactory.OnChallenge`

```csharp
// Validation/JwtBearerEventsFactory.cs (L60-68)
OnChallenge = context =>
{
    context.HandleResponse();
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    context.Response.ContentType = "application/json";
    var result = JsonSerializer.Serialize(new { error = "You are not authorized." });
    return context.Response.WriteAsync(result);
}
```

**問題点**:

1. `type`, `title`, `status`, `detail` フィールドが欠如している（RFC 7807 非準拠）。
2. `TraceId` が含まれていないため、インシデント時のサーバーログとの相関分析が不可能（Rule 6 違反）。
3. `Content-Type` が `application/json` であり、RFC 7807 で推奨される `application/problem+json` ではない。

## 3. Decision (決定事項)

両箇所を `ProblemDetails` 形式に統一する。共通の `ProblemDetails` 生成ロジックを導入し、`TraceId` を自動付与する。

### 3.1 共通ヘルパーの導入

```csharp
namespace VK.Blocks.Authentication.Validation;

internal static class AuthenticationProblemDetailsHelper
{
    public static Task WriteUnauthorizedAsync(HttpResponse response, string detail)
    {
        response.StatusCode = StatusCodes.Status401Unauthorized;
        response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            title = "Unauthorized",
            status = StatusCodes.Status401Unauthorized,
            detail,
            traceId = System.Diagnostics.Activity.Current?.Id
                      ?? response.HttpContext.TraceIdentifier
        };

        return response.WriteAsJsonAsync(problemDetails);
    }
}
```

### 3.2 適用箇所

```csharp
// ApiKeyAuthenticationHandler.HandleChallengeAsync
protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    => AuthenticationProblemDetailsHelper.WriteUnauthorizedAsync(
        Response, "API key is missing or invalid.");

// JwtBearerEventsFactory.OnChallenge
OnChallenge = context =>
{
    context.HandleResponse();
    return AuthenticationProblemDetailsHelper.WriteUnauthorizedAsync(
        context.Response, "Bearer token is missing or invalid.");
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: ASP.NET Core 組み込みの `IProblemDetailsService` を利用

- **Approach**: `IProblemDetailsService` を DI 経由で解決し、`ProblemDetailsContext` を利用してレスポンスを生成。
- **Rejected Reason**: `AuthenticationHandler` のコンテキストでは `IProblemDetailsService` が常に DI 解決可能とは限らない（特に認証ミドルウェアの早い段階）。また、ビルディングブロックが `IProblemDetailsService` への依存を持つことは、ホストアプリケーションの構成を強制することになるため回避。

### Option 2: 現状維持（非準拠のまま）

- **Approach**: API クライアントが既に `{ "error": "..." }` 形式に適応しているため変更しない。
- **Rejected Reason**: VK.Blocks 規約 (Rule 1) への準拠義務があり、今後の新規 API クライアントの統合コストを削減するためにも標準化は必須。また、`TraceId` の欠如は運用監視の深刻な死角を生む。

### Option 3: グローバル Exception Middleware でのみ Problem Details を返す

- **Approach**: 認証レイヤーでは素の JSON を返し、グローバルミドルウェアで `ProblemDetails` に変換する。
- **Rejected Reason**: 認証ハンドラーが `context.HandleResponse()` でレスポンスを完了させるため、後段のミドルウェアで変換するのは困難。認証レイヤー自体が責務を持つべき。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - RFC 7807 準拠により、すべての API クライアントが統一的なエラーパース処理を実装可能。
    - `TraceId` 付与により、SRE チームが 401 レスポンスとサーバーログを即座に相関分析可能。
    - `Content-Type: application/problem+json` により、クライアントが自動的にエラーレスポンスを識別可能。
- **Negative**:
    - 既存クライアントが `{ "error": "..." }` 形式を前提にエラーハンドリングしている場合、破壊的変更となる。
- **Mitigation**:
    - API バージョニング / リリースノートにおいて変更点を明示。
    - 移行期間として、`detail` フィールドに従来の `error` メッセージと同一の文言を使用し、クライアントの移行負担を軽減。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **影響範囲**: `ApiKeyAuthenticationHandler.cs`, `JwtBearerEventsFactory.cs`, 新規 `AuthenticationProblemDetailsHelper.cs` の3ファイル。
- **セキュリティ考慮**:
    - `detail` フィールドにスタックトレースや内部実装情報を含めてはならない。必ずユーザー向けの一般的なメッセージに限定する。
    - `TraceId` はログ相関用であり、攻撃者に有用な情報を漏洩しないため安全に公開可能。
- **検証**: 統合テストで 401 レスポンスの JSON 構造が RFC 7807 に準拠していることを検証。`TraceId` の存在を `Assert.NotNull` で確認。
