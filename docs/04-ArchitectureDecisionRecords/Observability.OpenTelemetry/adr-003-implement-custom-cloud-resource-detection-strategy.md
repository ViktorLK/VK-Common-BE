# ADR 003: Implement Custom Cloud Resource Detection Strategy

**Date**: 2026-03-12  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: Observability.OpenTelemetry  

## Context (背景)

OpenTelemetry には Azure や AWS、GCP といった様々なクラウドプラットフォームに対応する標準の Resource Detector が用意されています。しかし、それらに完全に依存すると、VK.Blocks 独自の要件（例えば `SERVICE_INSTANCE_ID` の明示的な管理や、`cloud.provider` などの独自に標準化されたセマンティックタグの付与）を Azure App Service と Kubernetes の両環境で一貫して適用することが困難でした。また、標準 Detector のために大量の外部依存（NuGet パッケージ）を引き込む必要が生じていました。

## Problem Statement (問題定義)

- **標準タグの不整合**: Azure 版 Detector と K8s 版 Detector で生成されるリソース属性の構造に差異があり、一貫したクエリが書けない。
- **外部依存の肥大化**: 各環境専用の Detector パッケージをインストールすることで、モジュールの依存関係が重くなり、ビルドサイズや依存関係解決の複雑さが増す。
- **独自属性の欠落**: `deployment.environment` 等、我々のプラットフォーム特有の必須タグが標準 Detector では自動生成されない。

## Decision (決定事項)

外部パッケージに依存する標準 Detector に頼るのではなく、**カスタムリソースジェネレータ（`VkResourceBuilder`）と宣言的な `CloudDetectionMode`** を実装することを決定しました。これにより、ホスティング環境（環境変数 `WEBSITE_SITE_NAME` や `K8S_CLUSTER_NAME` 等）を明示的に検査し、全マイクロサービスで統一されたセマンティック属性を `ResourceBuilder` に適用します。

### 設計詳細 (Design Details)

```csharp
public enum CloudDetectionMode
{
    Disabled = 0,
    Auto = 1,
    Azure = 2,
    Container = 3
}

public static class VkResourceBuilder
{
    public static ResourceBuilder Build(VkObservabilityOptions options)
    {
        var builder = ResourceBuilder.CreateDefault()
            .AddService(options.ServiceName, options.ServiceVersion, options.ServiceInstanceId)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = options.Environment
            });

        // カスタム環境検出の実行
        AppendCloudAttributes(builder, options.CloudDetection);
        
        return builder;
    }
}
```

## Alternatives Considered (代替案の検討)

- **Option 1: 公式 `OpenTelemetry.ResourceDetectors.Azure` 等の採用**
  - *Approach*: 公式のパッケージをそのまま利用する。
  - *Rejected Reason*: パッケージのバージョンアップに追従するメンテナンスコストがかかる上、タグの命名規則（Semantic Conventions）が我々の内規と異なる場合があり、柔軟な構成ができないため却下。

## Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - 環境変数をフックするだけのシンプルで軽量な実装となり、不要な NuGet 依存関係を排除できました。
  - メトリクスやトレースにおけるリソース属性の完全な標準化（Standardization）が保証されました。
- **Negative**:
  - インフラ構成（環境変数の名称）が変更された場合、コード側（`VkResourceBuilder`）の追従修正が必要となります。
- **Mitigation**:
  - 対象の環境変数名を `private const string` としてクラス内に隔離し、将来的な変更箇所を特定しやすくしました。また、ADR-001 に基づき、将来的には `IEnvironmentProvider` で抽象化することでテスト容易性と保守性をさらに向上させます。

## Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: Kubernetes では `K8S_CLUSTER_NAME` や `K8S_POD_NAME` を、Azure では `WEBSITE_SITE_NAME` 等を読み取り、`cloud.provider` をそれぞれ `kubernetes`, `azure` として付与します。
- **セキュリティ重点**: 環境変数からの読み取り値は基本的に信用できない入力（Untrusted Input）として扱うため、文字列のパースや辞書への登録時に不正な長文字やエスケープシーケンスが含まれていないか、またそれらがログバックエンドのバッファオーバーフローを引き起こさないよう、適切なサニタイズ（将来的には Regex 制限等）を考慮したセキュアな設計を志向します。
