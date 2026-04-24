# ADR 021: Zero-Reflection Auto-Discovery of Authentication Extensions via Source Generators

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks Performance & AOT Readiness

## 2. Context (背景)

VK.Blocks は、プラグイン方式で認証プロバイダー（OAuth プロバイダーやカスタムクレームマッパー等）を追加できる柔軟性を提供しています。しかし、これらの拡張ポイントを自動的に検出するために、従来のアプローチでは実行時にアセンブリを走査（Assembly Scanning）し、リフレクションを用いて DI コンテナに登録していました。

## 3. Problem Statement (問題定義)

1. **起動パフォーマンス**: アセンブリのスキャンは、特に大規模なアプリケーションにおいて起動時間を著しく増大させる。
2. **Native AOT 互換性**: リフレクションを多用する動的な型解決は、.NET の Native AOT (Ahead-of-Time) コンパイルと相性が悪く、トリミング（不要コード削除）によって必要なクラスが削除されるリスクがある。
3. **ランタイムエラー**: 属性の付与ミスなどが実行時まで発覚しない。

## 4. Decision (決定事項)

`VK.Blocks.Generators` (C# Source Generators) を活用し、コンパイル時に認証拡張ポイントを自動検出し、静的な登録コードを生成する「Zero-Reflection」パターンを採用しました。

### 実装の柱
1. **マーカー属性**: `[VKOAuthProvider]` などの属性をトリガーとして、ソースジェネレーターが対象クラスを抽出。
2. **静的コード生成**: 発見されたクラスを DI コンテナに追加する C# コード（例：`AddGeneratedClaimsProviders()`）をビルド時に生成。
3. **リフレクションの排除**: 実行時にはジェネレーターが生成した確定済みのリストを呼び出すだけでよいため、型走査が不要。

```csharp
// 生成されるコードのイメージ
public static partial class GeneratedExtensions
{
    public static IServiceCollection AddGeneratedClaimsProviders(this IServiceCollection services)
    {
        services.AddKeyedScoped<IOAuthClaimsMapper, GitHubClaimsMapper>("GitHub");
        services.AddKeyedScoped<IOAuthClaimsMapper, GoogleClaimsMapper>("Google");
        return services;
    }
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: Scrutor 等の外部ライブラリ**: `Scrutor` を用いて実行時にスキャンする。
    - **Rejected Reason**: AOT 対応が困難であり、起動パフォーマンスの課題も解決されない。
- **Option 2: 手動登録**: 全てのプロバイダーを開発者が手動で `services.AddXxx` する。
    - **Rejected Reason**: 開発体験 (DX) が著しく低下し、登録漏れによるバグが発生しやすい。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 起動パフォーマンスの劇的な向上（スキャンコストがゼロ）。
    - Native AOT コンパイルへの完全な対応。
    - コンパイル時に重複登録などのエラーを検出可能（VK0001 警告等）。
- **Negative**:
    - ソースジェネレーター自体の複雑性が増し、デバッグには「生成されたファイルを表示」する手順が必要。
- **Mitigation**:
    - 生成されたコードには必ず適切な `[GeneratedCode]` 属性を付与し、デバッガでステップ実行可能な状態を維持する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **型安全**: 生成コードは強い型付けがなされており、実行時の `InvalidCastException` を防止。
- **セキュリティ**: 不正なアセンブリがランタイムにロードされても、コンパイル時に検証されていないプロバイダーが勝手に登録されることはない。

**Last Updated**: 2026-04-24
