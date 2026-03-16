# ADR 001: Eliminate BuildServiceProvider Anti-pattern in DI Configuration

**Date**: 2026-03-15  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: Observability.Serilog

## Context (背景)

以前の `AddSerilog` 拡張メソッドでは、Serilog の初期化時に `IOptions<SerilogOptions>` の設定値を反映するため、依存性の注入 (DI) の登録フェーズ内において `services.BuildServiceProvider()` を呼び出していました。これは、DI コンテナが完全に構築される前に中間状態の ServiceProvider を生成してしまう既知のアンチパターンです。

## Problem Statement (問題定義)

- **メモリリークとパフォーマンス低下**: 中間 ServiceProvider の破棄が行われない場合、メモリ上にシングルトンインスタンスが複数生成される等、アプリケーション起動時のパフォーマンス劣化やメモリリークの原因となります。
- **状態の不整合**: 最終的な `WebApplication` が使用するコンテナと異なるインスタンスが生成されるため、意図しないバグ（例：後から追加された HostedService が解決されない等）を引き起こすリスクがあります。

## Decision (決定事項)

DI フェーズにおける `BuildServiceProvider()` の呼び出しを完全に排除し、Serilog の構成には遅延評価を行うファクトリオーバーロード `(IServiceProvider, LoggerConfiguration) => { ... }` パターンを採用しました。
これにより、コンテナ構築が完全に終了し、アプリケーションが起動した実行フェーズにおいて必要な Options や外部依存が安全に解決されるようになります。

### 設計詳細 (Design Details)

**旧実装（アンチパターン）**:

```csharp
public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
{
    // ❌ Anti-pattern
    var sp = services.BuildServiceProvider();
    var options = sp.GetRequiredService<IOptions<SerilogOptions>>().Value;

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Is(options.MinimumLevel)
        .CreateLogger();

    services.AddSerilog();
    return services;
}
```

**新実装（ファクトリパターンの採用）**:

```csharp
public static IServiceCollection AddSerilogObservability(this IServiceCollection services)
{
    // ✅ サービスの登録のみ行い、解決は実行時に遅延させる
    services.AddSerilog((serviceProvider, loggerConfiguration) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<SerilogOptions>>().Value;

        loggerConfiguration
            .ReadFrom.Configuration(serviceProvider.GetRequiredService<IConfiguration>())
            .Enrich.WithProperty("Application", options.ServiceName);
            // Sinkの設定など
    });

    return services;
}
```

## Alternatives Considered (代替案の検討)

- **Option 1: IConfiguration を直接読み取る (Read directly from IConfiguration)**
    - _Approach_: DI から `IOptions` を解決するのではなく、引数で渡された `IConfiguration` から `Get<SerilogOptions>()` でバインドする。
    - _Rejected Reason_: DataAnnotations による検証がスキップされるため、設定値のフェイルセーフな検証（Fail-Fast）を保証できないため却下。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - アプリケーションのスタートアップがより安全になり、シングルトンの複製といった予期せぬ不具合を根絶できました。
    - DI のベストプラクティスに準拠し、テスト用意性が向上しました。
- **Negative**:
    - 設定の例外（Validation エラー等）が、`AddSerilog` 呼び出し時ではなく、アプリケーション起動直後（最初のログ出力イベント時）に遅延して発生する可能性があります。
- **Mitigation**:
    - `IValidateOptions<T>` や [OptionsValidator] Source Generator による Eager Validation を ASP.NET Core のスタートアップフローに追加統合することで起動時のフェイルファストを担保します。

## Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ重点**: ログ基盤自体の初期化に失敗した場合に備え、起動時のフォールバック先として `Console` 標準出力を利用するセーフティネットを実装しています。
