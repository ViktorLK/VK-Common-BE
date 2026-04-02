# ADR 010: Dynamic Dictionary-Driven OAuth Configuration

**Date**: 2026-03-30  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Authentication

## 2. Context (背景)

The OAuth infrastructure hardcoded a fixed set of supported identity providers (AzureB2C, Google, GitHub) as explicit properties on the `OAuthOptions` class. Adding a new provider resulted in a violation of the Open/Closed Principle, requiring core module changes.

## 3. Problem Statement (問題定義)

システムが特定の OAuth プロバイダー（Google, GitHub 等）のプロパティを直接 `OAuthOptions` にハードコードして依存していました。
- **OCP（開閉原則）違反**: 新しい IDP (例: Auth0, LINE) を追加するたびに、設定クラス、DI 登録ロジック、バリデーションロジックの3箇所でソースコードを変更して再デプロイする必要があった。

## 4. Decision (決定事項)

`OAuthOptions` を**辞書（Dictionary）駆動型**にリファクタリングし、プロバイダーを動的に登録・検証できるアーキテクチャを採用しました。

- `appsettings.json` のキー名に基づいて自動的に構成を取り込み、.NET 8+ の **Keyed Services** を利用して動的に `IOAuthClaimsMapper` を DI に登録します。

```csharp
public sealed class OAuthOptions
{
    public Dictionary<string, OAuthProviderOptions> Providers { get; set; } = [];
}
```

```csharp
// DI 登録（動的）
foreach (var (providerName, providerOptions) in oauthOptions.Providers)
{
    if (providerOptions.Enabled) {
        services.AddKeyedScoped<IOAuthClaimsMapper, ...>(providerName);
    }
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: リフレクションによる Mapper の全自動スキャン**
  - **Approach**: アセンブリをスキャンして自動的にプロバイダー名を解決し登録する。
  - **Rejected Reason**: 認証レイヤーにおいては暗黙的な登録よりも、設定ファイルによる明示的（Explicit）なコントロールの方がセキュリティガバナンスの観点から推奨されるため。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: ビジネス要求の変化（新規 IDP 追加等）に対して、コードの変更なしに設定ファイル `appsettings.json` だけで柔軟に対応できる無限の拡張性を得た。
- **Negative**: `appsettings.json` の構造に対する**破壊的変更 (Breaking Change)** であり、古い設定ファイルを使用すると OAuth ログインが沈黙して失敗する。
- **Mitigation**: この変更はメジャーバージョンアップ（Phase 2）の一環とし、デプロイメントのドキュメントに確実なマイグレーション手順を記載する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- バリデーター (`OAuthOptionsValidator`) のループ処理において、`Enabled == true` のプロバイダーだけに対して `ClientId` と `Authority` の存在確認を行うため、無効化されたプロバイダーでシステムが起動失敗する事態を防ぎます。
