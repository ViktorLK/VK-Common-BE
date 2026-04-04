# ADR 001: Remove Dead Code and Unused Abstractions

**Date**: 2026-03-03  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: `src/BuildingBlocks/Authentication`

## 1. Context (背景)

2026年3月のアーキテクチャ監査において、`VK.Blocks.Authentication` モジュール内にデッドコードおよびコメントアウトされた未使用コードが発見された。これらは技術的負債として蓄積しており、開発者の認知的負荷を増加させている。

## 2. Problem Statement (問題定義)

以下の2つの問題が識別された：

### 問題 A: 未使用クラス `AuthResult`

`Abstractions/Contracts/AuthResult.cs` に定義された `AuthResult` クラスは、`Result` を継承しているが、モジュール内のどの箇所でも一切使用されていない。`IAuthenticationService.AuthenticateAsync` は `Result<AuthenticatedUser>` を返しており、`AuthResult` は完全なデッドコードとなっている。

```csharp
// 未使用のクラス — モジュール内での参照なし
public class AuthResult : Result
{
    protected AuthResult(bool isSuccess, Error error) : base(isSuccess, error) { }
}
```

### 問題 B: コメントアウトされた DI 登録

`DependencyInjection/AuthenticationBlockExtensions.cs` の L62 および L69 に、コメントアウトされたサービス登録が残存している。

```csharp
// services.AddSingleton<ITokenProvider, JwtTokenProvider>();  // L62
// services.AddScoped<ApiKeyProvider>();                        // L69
```

これらは **Rule 10（コード生成品質）** の精神に反しており、将来の開発者に「この機能は未実装なのか、意図的に削除されたのか」という混乱を招く。

## 3. Decision (決定事項)

以下のクリーンアップを実施する：

1. **`AuthResult.cs` の削除**: ファイルごと完全に削除する。既存のコードベースで参照がないため、破壊的変更のリスクはゼロである。
2. **コメントアウト行の削除**: `AuthenticationBlockExtensions.cs` から L62 および L69 のコメント行を削除する。

```diff
// AuthenticationBlockExtensions.cs
  authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
  {
      options.TokenValidationParameters = TokenValidationParametersFactory.Create(authOptions.Jwt);
      options.Events = Validation.JwtBearerEventsFactory.CreateEvents();
  });
-
- // services.AddSingleton<ITokenProvider, JwtTokenProvider>();

  // 3. API Key Configuration
  authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authOptions.ApiKeySchemeName, options =>
  {
  });
- // services.AddScoped<ApiKeyProvider>();
  services.AddScoped<ApiKeyValidator>();
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: コメントアウト行を TODO に変換

- **Approach**: コメントアウト行を `// TODO: Implement ITokenProvider registration` のような形式に変換し、将来のタスクとして追跡する。
- **Rejected Reason**: `ITokenProvider` および `ApiKeyProvider` は現在の設計にもロードマップにも存在しない。不要なインターフェースへの将来的な依存を示唆するコメントは誤解を招く。

### Option 2: `AuthResult` を活用するリファクタリング

- **Approach**: `IAuthenticationService` の戻り値を `Result<AuthenticatedUser>` から `AuthResult` に変更し、認証固有のメソッド（例えば `AuthResult.TokenExpired()` ファクトリ）を追加する。
- **Rejected Reason**: `Result<T>` パターンはプロジェクト全体で統一的に使用されており、認証固有の派生型を導入するメリットよりも一貫性維持のデメリットが上回る。`AuthenticationErrors` クラスによるエラー定数の集約で十分に表現力を確保できている。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - コードベースのノイズ除去による可読性向上。
    - Rule 10 への準拠。
    - 新規開発者の onboarding 時の混乱防止。
- **Negative**:
    - `AuthResult` を外部パッケージとして参照している NuGet コンシューマーがいた場合、破壊的変更となる。
- **Mitigation**:
    - リリース前に NuGet パッケージ互換性を確認。`AuthResult` が `public` であるため、メジャーバージョンアップ時に実施するのが最も安全。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **影響範囲**: ファイル2件のみ。ビジネスロジックへの影響なし。
- **セキュリティへの影響**: なし。デッドコードの除去であり、認証フローに変更はない。
- **検証**: `dotnet build` による全プロジェクトのコンパイル確認で十分。
