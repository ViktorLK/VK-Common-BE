# ADR 016: Functional Options Transformation for Immutable Building Blocks

- **Date**: 2026-04-25
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Building Block Configuration Pattern

## Context (背景)

VK.Blocks の設計原則（Rule 20）では、ビルドブロックの Options クラスにおいて `init` アクセサを用いた不変性（Immutability）の確保が義務付けられています。

一方で、.NET 標準の `Options` パターン（`IServiceCollection.AddOptions<T>().Configure(Action<T>)`）は、既存のインスタンスをデリゲート内で直接書き換える「原地変異（In-place Mutation）」を前提としています。この標準パターンは `init` プロパティと技術的に競合しており、デリゲート内での値の代入がビルドエラー（CS8852）を引き起こします。

これまでは、コードからの動的な設定を優先するために `set` アクセサへと妥協するケースがありましたが、これは不変性を重視する VK.Blocks のコア思想に反するものでした。

## Problem Statement (問題定義)

不変オプション (`init`) と動的構成 (`Action<T>`) の衝突により、以下の問題が発生しています。

1.  **Immutability の喪失**: `set` を許容すると、DI コンテナに登録された後のオプションが意図せず変更されるリスク（Side-effects）が生じる。
2.  **アーキテクチャの一貫性の欠如**: Rule 20 と実装実態が乖離し、開発者によって `init` と `set` が混在する原因となる。
3.  **関数型プログラミングの利点の欠如**: C# 9.0 以降の強力な `record` 機能（`with` 式）を構成ロジックに活用できていない。

## Decision (決定事項)

オプションの不変性を 100% 維持しつつ動的構成をサポートするため、構成デリゲートを **`Action<TOptions>` から `Func<TOptions, TOptions>` へ移行** する「関数型オプション変換パターン」を採用します。

### 設計の詳細

1.  **`Func<T, T>` の採用**: 
    構成デリゲートを「値を書き換える命令」ではなく、「既存のインスタンスを受け取り、変換後の新しいインスタンスを返す関数」として定義します。
2.  **`with` 式の活用**:
    利用者は C# の `with` 式を使用して、不変性を保ったまま特定のプロパティだけを変更した新しいインスタンスを生成・返却します。
3.  **`VK.Blocks.Core` の拡張**:
    `AddVKBlockOptions` において、`IConfiguration` からバインドされた初期インスタンスを `Func` に渡し、その戻り値を最終的なシングルトンとして DI コンテナに登録します。

#### 核心的なコードスニペット

```csharp
// VK.Blocks.Core の実装イメージ
public static TOptions AddVKBlockOptions<TOptions>(
    this IServiceCollection services,
    IConfiguration configuration,
    Func<TOptions, TOptions>? transform = null) // Action ではなく Func
    where TOptions : class, IVKBlockOptions, new()
{
    var options = new TOptions();
    configuration.GetSection(TOptions.SectionName).Bind(options);

    if (transform != null)
    {
        // 関数型変換を適用し、新しいインスタンスを得る
        options = transform(options);
    }

    services.TryAddSingleton(options);
    return options;
}

// 利用側のイメージ
services.AddVKAuthenticationBlock(config, options => options with 
{ 
    Enabled = true, 
    DefaultScheme = "Bearer" 
});
```

## Alternatives Considered (代替案の検討)

- **Option 1: `set` アクセサの許容 (現状維持)**
  - **Approach**: 全プロパティを `set` に変更し、標準の `Action<T>` を使う。
  - **Rejected Reason**: 不変性が失われ、ランタイムでの予期せぬ変更や副作用を許容してしまうため。

- **Option 2: コンストラクタ注入による初期化**
  - **Approach**: `IConfiguration` のバインドを諦め、すべて手動で `new` する。
  - **Rejected Reason**: 構成ファイル (`appsettings.json`) との統合が非常に困難になり、利便性が著しく低下するため。

## Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な不変性**: DI コンテナ内のオプションが生成後に変更されないことが言語仕様レベルで保証される。
- **副作用の排除**: 構成ロジックが純粋関数化され、テストが容易になり、予期せぬ動作を防止できる。
- **モダンな構文**: C# の `record` 特性を最大限に活用できる。

### Negative
- **標準との乖離**: `Microsoft.Extensions.Options` の標準的な拡張メソッド（`Configure(Action<T>)`）を直接利用できない。
- **移行コスト**: 既存の Building Block のシグネチャを変更する必要がある。

### Mitigation (緩和策)
- `VK.Blocks.Core` にて、標準の `IOptions` パイプラインとの橋渡しを行うラッパーを提供し、利用者が低レイヤーの複雑さを意識しなくて済むようにする。

## Implementation & Security (実装詳細とセキュリティ考察)

- **スレッドセーフ**: 変換は Startup フェーズ（ConfigureServices）で行われ、その後は不変となるため、ランタイムでのスレッドセーフ性が完璧に確保される。
- **セキュリティ**: オプションが読み取り専用となることで、悪意のあるコードやバグによって実行時にセキュリティ設定（Enabled フラグなど）が書き換えられるリスクを完全に排除できる。
