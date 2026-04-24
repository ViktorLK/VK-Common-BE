# ADR 023: Modular Feature-Sliced Registration Pattern for Authentication Sub-Blocks

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Authentication Architecture & Maintainability

## 2. Context (背景)

Authentication ブロックは、JWT Bearer, API Key, OAuth, Cookie 認証など、多岐にわたる独立した機能を提供します。これまでの設計では、一つの巨大な登録クラス（`AuthenticationBlockRegistration`）に全ての DI 登録ロジックが詰め込まれる傾向があり、特定の機能を修正・拡張する際の副作用の特定や、コードの可読性に課題がありました。

## 3. Problem Statement (問題定義)

1. **低い凝集度**: JWT に関連するコードと API Key に関連するコードが混在し、単一責任原則 (SRP) に反している。
2. **保守の困難さ**: 特定の認証方式だけをカスタマイズしたり、条件付きで無効化したりするロジックが複雑な `if` 文の連鎖になりやすい。
3. **DI 順序の依存**: コンポーネント間の登録順序が暗黙的であり、修正によって認証パイプラインが壊れるリスクがある。

## 4. Decision (決定事項)

各認証機能（Feature）を垂直スライス（Vertical Slice）として扱い、それぞれが自身の内部登録ロジックを持つ「機能スライス型登録パターン」を採用しました。

### 構成要素
1. **Feature Registration**: 各ドメイン（ApiKeys, Jwt, OAuth 等）の `Internal/` フォルダ内に `XxxFeatureRegistration.cs` を配置。
2. **AuthenticationBlockBuilder**: Fluent API を提供し、`services.AddAuthenticationBlock(config).AddJwt().AddApiKey()` という形式で機能をオプトイン可能にする。
3. **Orchestration**: メインの `AuthenticationBlockRegistration` は、共通基盤（HttpContextAccessor 等）と各 Feature の呼び出しのみを担当。

```csharp
// 内部的なオーケストレーションの例
internal static class AuthenticationBlockRegistration
{
    public static IVKAuthenticationBuilder Register(...)
    {
        // 共通基盤の登録
        services.TryAddTransient<IClaimsTransformation, ClaimsTransformer>();
        
        // ビルダーの返却
        return new AuthenticationBlockBuilder(services, configuration, authBuilder);
    }
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: モノリシック登録**: 全てを一つのファイルに書く。
    - **Rejected Reason**: ファイルサイズが数千行に達し、保守不能になることが予見されるため。
- **Option 2: 外部モジュール方式**: JWT や API Key を別々のアセンブリ（Nuget）に分ける。
    - **Rejected Reason**: 管理コストが増大し、VK.Blocks.Authentication としての統合されたユーザー体験が損なわれるため。まずは一つのアセンブリ内でフォルダによる論理分離を行う。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - **高い凝集度**: 各認証方式のロジックが完全に分離され、変更の影響範囲が明確。
    - **宣言的な DI**: 利用側（App）が必要な機能だけを選択して有効化できる。
    - **テスト容易性**: 各 Feature 単体でのユニットテストや統合テストが書きやすくなる。
- **Negative**:
    - ファイル数が増え、DI 登録の流れを追う際に複数のフォルダを跨ぐ必要がある。
- **Mitigation**:
    - 標準化された命名規則（`XxxFeatureRegistration`）とフォルダ構造（`FeatureName/Internal/`）を Rule 16 で規定し、探索を容易にする。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Idempotency**: 各 Feature の登録メソッド内でも `IsVKBlockRegistered` パターンを使用し、多重登録を防止。
- **Validation**: 各 Feature は独自の `IValidateOptions` を持ち、有効化された機能に対してのみ厳格なバリデーションを行う。

**Last Updated**: 2026-04-24
