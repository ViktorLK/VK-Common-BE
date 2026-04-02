# ADR 008: Centralized Identity Claims Resolution as SSOT

**Date**: 2026-03-30  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Authentication

## 2. Context (背景)

Previously, the logic to extract identity attributes (like `UserId`, `TenantId`, `Roles`) from a `ClaimsPrincipal` was scattered across multiple services (e.g., `JwtAuthenticationService`, OAuth mappers, `VKClaimsTransformer`). This led to inconsistent claim type usage (mixing `ClaimTypes.NameIdentifier` and `VKClaimTypes.UserId`) and redundant code.

## 3. Problem Statement (問題定義)

システム内で認証済みの属性を取得するロジックが散在しており、以下の問題を引き起こしていました：
- **SSOT (Single Source of Truth) 違反**: コントローラーと内部サービスで参照する Claim Type (`ClaimTypes` vs `VKClaimTypes`) が一貫しておらず、バグの温床になっていた。
- **DRY 原則の欠如**: 各クラスで `principal.FindFirst(...)` と Null チェックを都度記述していた。

## 4. Decision (決定事項)

`ClaimsPrincipalExtensions` を SSOT として確立し、すべての ID 属性の抽出ロジックを粒度の細かい拡張メソッド（例：`GetUserId()`, `GetRoles()`）にカプセル化することを決定しました。

```csharp
public static string? GetUserId(this ClaimsPrincipal? principal)
{
    return principal?.FindFirst(VKClaimTypes.UserId)?.Value 
        ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: DI注入型の `ICurrentUserService` を実装する**
  - **Approach**: インターフェース経由でユーザー情報を解決する。
  - **Rejected Reason**: `ClaimsPrincipal` は既に HTTP コンテキスト等から標準的に引き回されるオブジェクトであり、わざわざ専用のラッパーサービスをインジェクトするオーバーヘッドは不要と判断した。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: 認証データの抽出ロジックが一箇所に集約され、修正や監査が極めて容易になった。
- **Negative**: 静的メソッドであるため、単体テスト時にモックすることが難しい。
- **Mitigation**: `ToAuthenticatedUser()` を介してインターフェースや DTO にマッピングすることで、ドメイン層では副作用のない純粋なオブジェクトとして扱えるようにする。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- `VKClaimTypes` を第一候補として検索し、フォールバックとして `ClaimTypes.*` を併用することで、外部プロバイダー由来のトークンと安全に互換性を保ちます。
