# ADR 009: Strict Eager Validation for Authentication Configuration

**Date**: 2026-03-30  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Authentication

## 2. Context (背景)

Authentication configuration classes previously relied on nullable properties and lacked comprehensive validation during startup. This delayed the detection of misconfigurations (like short or missing secret keys) until runtime, posing security and stability risks.

## 3. Problem Statement (問題定義)

以前の設定クラス（`JwtValidationOptions` 等）は、未設定のプロパティを許容し、初期化時に `null` やデフォルト値を保持していました：
- **Fail-Late**: 設定ミスがある状態でアプリケーションが起動できてしまい、本番環境でユーザーがアクセスした瞬間に初めて例外（NullReferenceException等）が発生する。
- **防御的プログラミングの強制**: 消費側で `if (option.SecretKey != null)` のような冗長なチェックが必須となり、コードの可読性が低下していた。

## 4. Decision (決定事項)

すべての必須設定クラスに対して **Eager Validation（事前検証）**と**非 Null デフォルト値**を強制する方針を決定しました。

- `string?` -> `string = string.Empty;` に変更。
- `IValidateOptions<T>` を実装し、DI コンテナへの登録時に `.ValidateOnStart()` を付与する。

```csharp
services.AddOptions<JwtValidationOptions>()
    .Bind(configuration.GetSection("Jwt"))
    .ValidateOnStart(); // 起動時に検証を強制
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: DataAnnotations による検証**
  - **Approach**: `[Required]` や `[MinLength]` 属性を設定プロパティに付与し `.ValidateDataAnnotations()` を使用する。
  - **Rejected Reason**: OIDC モードと Symmetric モードで必須要件が動的に変化するなど、条件付き複雑なビジネスロジックが存在するため、静的な属性ベースの限界を超えており、カスタムの `IValidateOptions<T>` 実装が最適と判断した。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: アプリケーションはセキュアかつ完全な設定状態でしか起動できなくなり（Fail-Fast）、運用時の設定ミスによる障害がゼロになる。
- **Negative**: CI/CD 環境などで、環境変数が不完全な状態で `dotnet run` などを実行すると直ちにクラッシュする。
- **Mitigation**: ローカル開発用の `appsettings.Development.json` に適正なモック設定を確実に用意する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- セキュリティ上最もクリティカルな `SecretKey` は最低長（32文字以上）を検証し、弱い鍵でシステムが起動してしまう脆弱性を根絶します。
