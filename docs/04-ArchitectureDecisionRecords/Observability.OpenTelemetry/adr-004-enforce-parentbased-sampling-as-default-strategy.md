# ADR 004: Enforce ParentBased Sampling as Default Strategy

**Date**: 2026-03-12  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: Observability.OpenTelemetry  

## Context (背景)

分散マイクロサービスアーキテクチャでは、多数のサービスを経由してリクエストが処理されます。すべてのサービスでトレースを全量記録する `AlwaysOn` サンプリング戦略を適用すると、OpenTelemetry Collector やバックエンド領域（Elasticsearch, Jaeger等）に過剰なデータ量（Excessive Data Volume）が送信され、パフォーマンスの低下やストレージコストの増大を引き起こします。一方で、各サービスが個別にランダムサンプリング（例：10%）を行うと、「Aサービスは記録したがBサービスは記録しなかった」という状態が発生し、分断された不完全なトレースグラフ（Fragmented Trace Graphs）が生成されてしまいます。

## Problem Statement (問題定義)

- **データ量とコストの肥大化**: `AlwaysOn` は高トラフィックな本番環境では許容できないほどのリソース消費を招く。
- **トレースの連続性喪失**: 独立した確率的サンプリング（`TraceIdRatioBased` 等）を各ノードでバラバラに適用すると、相関関係の調査が不可能になる。

## Decision (決定事項)

データ量を制御しつつトレースの連続性を担保するため、設定オプション `VkObservabilityOptions` 上での**デフォルトサンプリング戦略を `ParentBasedAlwaysOn` に強制（Enforce）** することを決定しました。
これにより、サンプリングの決定権は常に「ルートスパン（API Gateway やインバウンドの初期サービス）」が持ち、下流のすべてのサービスはその決定を厳密に継承（Inherit）します。

### 設計詳細 (Design Details)

```csharp
public enum SamplerStrategy
{
    AlwaysOn = 0,
    AlwaysOff = 1,
    TraceIdRatioBased = 2,
    ParentBasedAlwaysOn = 3
}

public sealed class VkObservabilityOptions
{
    // ParentBasedAlwaysOn を既定値として設定
    public SamplerStrategy SamplerStrategy { get; set; } = SamplerStrategy.ParentBasedAlwaysOn;
}
```

ビルダー内での解決ロジック:
```csharp
case SamplerStrategy.ParentBasedAlwaysOn:
default:
    // 親のサンプリングフラグを優先。親がないルートの場合は AlwaysOn。
    tracing.SetSampler(new ParentBasedSampler(new AlwaysOnSampler()));
    break;
```

## Alternatives Considered (代替案の検討)

- **Option 1: 各サービスでの独立した `TraceIdRatioBased` の適用**
  - *Approach*: ルートかどうかに関わらず、独自の乱数ベースで 10% サンプリングを実施する。
  - *Rejected Reason*: 前述の通り、分散トレーシングの最大の価値である「一連の処理の可視化」が破壊されるため却下。

## Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - ルートノードでサンプリングを調整するだけで、システム全体のログ量を安全かつ一元的に制御できるようになりました。
  - サンプリングされたトレースは必ずEnd-to-Endで完全なグラフを形成するため、トラブルシューティング時のノイズが排除されます。
- **Negative**:
  - インフラ全体として「ルートノード（Gateway等）で正しいサンプリング設定（例：RatioBased や特定パスの除外など）が行われていること」に強く依存します。ルート設定を誤ると、全下流サービスにその誤りが伝播します。
- **Mitigation**:
  - インフラコード（API Gateway の構成や W3C TraceContext の生成部分）でデフォルトのサンプリングレートを厳格に管理するポリシーを別途策定します。

## Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: `ParentBasedSampler` は W3C TraceContext の `traceflags` (sampled flag) を読み取り、後続の OTel インストゥルメンテーションに伝播させます。
- **セキュリティ重点**: 外部の悪意あるクライアントが意図的に `traceflags=01` (サンプリング強制) を付与した悪意あるヘッダーを送信し、内部リソースを枯渇させる「Denial of Service via Over-tracing」攻撃を防ぐ必要があります。この対策として、VK.Blocks の Ingress 層（または API Gateway）では、信頼できないパブリックネットワークからの W3C ヘッダーを受け入れず、内部生成したトレースIDのみを信頼する（Trust Boundary の境界制御）運用を前提とします。
