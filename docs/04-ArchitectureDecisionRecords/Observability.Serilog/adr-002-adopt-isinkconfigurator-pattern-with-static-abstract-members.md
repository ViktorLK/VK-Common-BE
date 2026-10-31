# ADR 002: Adopt ISinkConfigurator Pattern with Static Abstract Members

**Date**: 2026-03-15  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: Observability.Serilog

## Context (背景)

Serilog のログ出力先（Sink）である Console、File、将来的には Seq や OpenTelemetry などの設定ロジックは多岐にわたります。これらを単一の中央拡張メソッド（`AddSerilog` の中）にハードコードすると、ファイルが肥大化するだけでなく、新しい Sink を追加するたびにコアロジックを修正する必要があり、Open-Closed Principle (OCP) に違反していました。

## Problem Statement (問題定義)

- **モジュール性の欠如**: 各 Sink 特有の構成処理（例：ファイルのローテーション条件、コンソールテーマの設定）が密結合になり、単一責任の原則が損なわれていました。
- **テスト・拡張の困難さ**: 特定の Sink のみを有効化した隔離テストが困難であり、サードパーティ製 Sink をシームレスにプラグインする枠組みがありませんでした。

## Decision (決定事項)

C# 12 の**静的抽象 (static abstract) メンバー**を活用した汎用拡張インターフェース `ISinkConfigurator<TOptions>` を導入しました。各 Sink (Console, File) はこのインターフェースを実装する `internal sealed class` として隔離し、コアの登録メソッドを一切変更することなく、コンパイル時に型安全なプラグインアーキテクチャを実現します。

### 設計詳細 (Design Details)

**インターフェース定義**:

```csharp
public interface ISinkConfigurator<in TOptions>
{
    static abstract void Configure(
        LoggerConfiguration loggerConfiguration,
        TOptions options,
        IServiceProvider serviceProvider);
}
```

**実装（File Sink の例）**:

```csharp
internal sealed class FileSinkConfigurator : ISinkConfigurator<SerilogOptions>
{
    public static void Configure(LoggerConfiguration loggerConfiguration, SerilogOptions options, IServiceProvider provider)
    {
        if (!options.File.Enabled) return;

        loggerConfiguration.WriteTo.Async(a => a.File(
            path: options.File.Path,
            rollingInterval: RollingInterval.Day,
            formatter: new CompactJsonFormatter()
        ));
    }
}
```

**利用側（コア）**:

```csharp
// ISinkConfigurator を実装する型を呼び出すだけで適用完了
ConsoleSinkConfigurator.Configure(loggerConfiguration, options, serviceProvider);
FileSinkConfigurator.Configure(loggerConfiguration, options, serviceProvider);
```

## Alternatives Considered (代替案の検討)

- **Option 1: Delegate/Action の DI 配列登録 (`IEnumerable<Action<LoggerConfiguration>>`)**
    - _Approach_: DI に Action を複数登録し、ランタイムにすべて実行する手法。
    - _Rejected Reason_: Serilog は DI 構築フェーズで初期化されるため、ランタイムコレクションの解決と相性が悪く、ライフサイクル管理が複雑化するため。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - コアメソッドを変更することなく未知の Sink 設定を追加できるようになり、高いモジュール性を獲得しました。
    - C# の静的型チェックにより、設定不備（Configure メソッドの未実装等）がコンパイル時に検出されます。
- **Negative**:
    - `static abstract` メンバーはインスタンスを生成しないため、DI を通じたランタイムでの動的な Sink モジュールのロード（実行中の Sink 追加等）には向いていません。
- **Mitigation**:
    - 現行の要件ではアプリケーション起動時の静的バインディングで十分であるため、インターフェースを `internal` 系にとどめ、必要に応じて将来的にインスタンスベースへ移行できる余地を残しています。
