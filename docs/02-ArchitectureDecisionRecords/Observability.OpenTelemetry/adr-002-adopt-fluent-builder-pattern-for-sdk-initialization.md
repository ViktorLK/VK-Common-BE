# ADR 002: Adopt Fluent Builder Pattern for SDK Initialization

**Date**: 2026-03-12  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: Observability.OpenTelemetry  

## Context (背景)

以前のアーキテクチャでは、OpenTelemetry (OTel) の初期化設定は、`OtlpOptions` をバインドした単純な `IServiceCollection` の拡張メソッド (`AddAppTracing`, `AddAppMetrics`) を通じて行われていました。しかし、このアプローチは硬直化（Inflexible）を招き、例えば "ASP.NET Core と EF Core のインストゥルメンテーションを個別にオプトイン・オプトアウトする" といった、シグナル構成ごとのきめ細やかな制御が困難でした。また、サンプラーやプロセッサーの複雑なロジックを整合性をもって管理する上で、単純な構成バインディングでは限界がありました。

## Problem Statement (問題定義)

- **拡張性の欠如 (Lack of Extensibility)**: 新しいインストゥルメンテーション（例：Redis、gRPC）を追加するたびに、巨大な設定用メソッドにフラグ引数を追加せざるを得ず、Open-Closed Principle (OCP) に違反していました。
- **カプセル化の破壊 (Leaking Abstractions)**: OTel 内部の複雑な `TracerProviderBuilder` や `MeterProviderBuilder` の構築ロジックが、アプリケーション側のスタートアップコードに漏れ出していました。
- **可読性の低下**: モノリシックな設定ブロックにより、どの機能が有効化されているのかが一目でわかりにくい状態でした。

## Decision (決定事項)

OpenTelemetry SDK の初期化を統合的かつ柔軟に行うため、**Fluent Builder パターン** を採用し、`AddVkObservability().AddTracing().AddMetrics()` のような連鎖的なオプトイン型 API (`VkObservabilityBuilder`) を導入しました。
設定状態の構築（Building）と実行（Execution）を分離し、旧来の拡張メソッドは `[Obsolete]` として非推奨化しました。

### 設計詳細 (Design Details)

```csharp
namespace VK.Blocks.Observability.OpenTelemetry.Builder;

public sealed class VkObservabilityBuilder
{
    private readonly IServiceCollection _services;
    private readonly VkObservabilityOptions _options;
    private readonly OpenTelemetryBuilder _otelBuilder;

    // Fluent method chaining
    public VkObservabilityBuilder AddTracing(Action<TracerProviderBuilder>? configure = null)
    {
        if (!_options.EnableTracing) return this;
        // ... (Tracing Setup logic)
        return this;
    }

    public VkObservabilityBuilder AddMetrics(Action<MeterProviderBuilder>? configure = null)
    {
        if (!_options.EnableMetrics) return this;
        // ... (Metrics Setup logic)
        return this;
    }
}
```

**DI 登録ロジックの例**:
```csharp
services.AddVkObservability(o =>
{
    o.ServiceName = "OrderService";
    o.SamplerStrategy = SamplerStrategy.ParentBasedAlwaysOn;
})
.AddTracing()
.AddMetrics()
.AddAspNetCoreInstrumentation();
```

## Alternatives Considered (代替案の検討)

- **Option 1: IConfiguration Binding の強化 (Enhancing `IConfiguration` bind model)**
  - *Approach*: `appsettings.json` の階層を深くし、`EnableAspNetCore`, `EnableEfCore` などのブール値フラグを `OtlpOptions` に列挙する。
  - *Rejected Reason*: 設定ファイルが肥大化し、型安全な構成コールバック（`Action<TracerProviderBuilder>` のようなカスタムデリゲート）を注入できないため却下。

## Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - インストゥルメンテーションごとのオプトインが明示的になり、可読性が劇的に向上しました。
  - OTel の複雑な内部 API を隠蔽し、使いやすい SDK を提供できました。
- **Negative**:
  - `VkObservabilityBuilder` クラス自体が複雑になり、各種ビルダーアクションを遅延評価するための内部状態（Action delegate の蓄積）を管理する必要があります。
- **Mitigation**:
  - `VkObservabilityBuilder` を `sealed` とし、単一責任の原則に従って構築ロジックのみに集中させることで複雑性を制御します。

## Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: ビルダー内で各種インストゥルメンテーションが登録される際、`VkObservabilityOptions` の内容（`EnableTracing`, `EnableMetrics`）が安全に評価されます。
- **セキュリティ重点**: 外部からの不正な設定注入（オーバーライド）を防ぐため、オプション構成には `[Required]` や DataAnnotations によるバリデーションをかませ、異常な設定（空のサービス名など）での起動自体をブロック（Fail-Fast）するフェイルセーフな機構を担保しています。
