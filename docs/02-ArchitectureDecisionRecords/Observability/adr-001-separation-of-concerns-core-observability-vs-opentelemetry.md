# ADR 001: Separation of Concerns - Core Observability vs. OpenTelemetry Integration

**Date**: 2026-03-11  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Initial Observability Module Design

## 2. Context (背景)

システム全体の可観測性（Observability）を向上させるために、ログ・メトリクス・分散トレースを導入する必要があります。
.NETにおけるオブザーバビリティの実装としては、`System.Diagnostics` API（`Activity`, `Meter`）を利用する標準的手法と、データの収集・送信を担う `OpenTelemetry` などの外部SDKを利用する手法があります。
これらを1つのモジュール内で密結合させるか、それとも責務を分離して複数モジュールにするかがアーキテクチャ上の課題となりました。

## 3. Problem Statement (問題定義)

オブザーバビリティのAPI（インターフェース層）とOpenTelemetry SDK（インフラストラクチャ層）を同一プロジェクトに混在させた場合、以下の問題が発生します：

1. **Vendor Lock-in**: アプリケーション（Domain / Application / Presentation層）がOpenTelemetry固有のSDKや型（`TracerProvider`, `OtlpExporter` など）に直接依存してしまい、将来別のテレメトリシステムへ移行する際のコストが跳ね上がります。
2. **Violation of Clean Architecture**: インフラストラクチャの関心事がビジネスロジックやコア抽象と混ざり合い、依存関係の逆転原則（DIP）に違反します。

## 4. Decision (決定事項)

オブザーバビリティ機能を提供するモジュールを、抽象・計装層とインフラストラクチャ層の2つに物理的に分離（C# プロジェクトとして分割）することを決定しました。

1. **`VK.Blocks.Observability` (Core/Abstraction Layer)**
    - `System.Diagnostics`（組み込みAPI）のみに依存します。
    - アプリケーションが実際にトレース（Span）やメトリクスを記録し、ログエンリッチメント（UserContext, TraceId等）を行うための機能（`ActivityExtensions` 等）を提供します。
    - OpenTelemetryのパッケージ（SDK）には **一切依存しません**。

2. **`VK.Blocks.Observability.OpenTelemetry` (Infrastructure Layer)**
    - オープンテレメリのSDK（`OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Exporter.OpenTelemetryProtocol` 等）に依存します。
    - `VkObservabilityBuilder` などを通じて、生成されたテレメトリデータをどこへ送るか、どうサンプリングするかといったインフラ設定のみを担います。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 1つのプロジェクト（`VK.Blocks.Observability`）にOpenTelemetryパッケージを全て組み込む**
    - **Approach**: アプリケーション開発者が1つのNuGetパッケージを参照するだけで全てが完結する設計。
    - **Rejected Reason**: アプリケーションレイヤーにインフラの依存（OTLPエクスポーター等）が強制され、テスト性や依存関係のクリーンさが失われるため却下。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - Application層や各サブモジュールは、軽量で標準的な `VK.Blocks.Observability` にのみ依存すればよく、OpenTelemetryの重いSDKを持ち込む必要がなくなります。
    - 将来、監視基盤がOpenTelemetryから別のもの（例: Application Insights特化SDKなど）に変わっても、Core層のコードを変更することなくInfrastructure層の差し替えのみで対応可能です。
- **Negative**:
    - 構成のセットアップ時に、2つのプロジェクトを正しく設定・紐付ける手間が生じます。
- **Mitigation**:
    - ホスト（Web API等）側でのセットアップを簡素化するため、`AddVkObservability()` のような拡張メソッドを提供し、複雑さをカプセル化します。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Implementation**:
  `VK.Blocks.Observability.csproj` には `System.Diagnostics` および `Microsoft.Extensions` 系のパッケージのみを含め、`VK.Blocks.Observability.OpenTelemetry.csproj` は `OpenTelemetry` SDK群を持った上でCore側のプロジェクトを参照（`ProjectReference`）する一方向の依存関係を構築します。
