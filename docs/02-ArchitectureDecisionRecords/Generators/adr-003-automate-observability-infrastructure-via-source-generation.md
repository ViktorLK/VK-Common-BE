# ADR 003: Automate Observability Infrastructure via Source Generation

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Observability Module

## Context (背景)

エンタープライズシステムにおいて、安定した運用とトラブルシューティングを実現するためには、分散トレーシング（Traces）およびメトリクス（Metrics）の統合が不可欠です。
.NET では、標準の `System.Diagnostics.ActivitySource` と `System.Diagnostics.Metrics.Meter` を用いるのがベストプラクティスです。しかし、各機能モジュールごとに静的なインスタンスを正しく宣言し、名前付けの規則（命名規約とバージョン付け）に一貫性を持たせる作業は、単調なボイラープレートの繰り返しでした。

## Problem Statement (問題定義)

- **DX (開発体験) の低下**: トレーシングを導入したい全てのドメインサービスやハンドラーにおいて、わざわざ `public static readonly ActivitySource ...` 等をコピペして準備しなければならず、実装の負担が大きい。
- **一貫性の欠如**: 手動コピーに頼ることで、異なる開発者が異なる命名規則やハードコードされたバージョン（"1.0.0"など）を使ってしまい、Grafana や Datadog 等の可観測性基盤側でのデータ集計・相関分析が困難になるリスクがあります。

## Decision (決定事項)

可観測性のインフラストラクチャーをコンパイル時に自動生成する `VKBlockDiagnosticsGenerator` を作成し、開発者は `[VKBlockDiagnostics("BlockName")]` という宣言的な属性（Attribute）をクラスに付与するだけで済むアーキテクチャを決定しました。

ジェネレーターは、属性を持つ `partial` クラスをスキャンし、以下のような `ActivitySource` と `Meter` のボイラープレートをコンパイル時にバックグラウンドで自動付与します。

```csharp
public static readonly ActivitySource Source = new("BlockName", "1.0.0");
public static readonly Meter Meter = new("BlockName", "1.0.0");
```

## Alternatives Considered (代替案の検討)

### Option 1: DI (依存性注入) による ActivitySource/Meter の動的注入

- **Approach**: `IMeterFactory` などを経由して、ハンドラーのコンストラクタで ActivitySource や Meter を受け取る。
- **Rejected Reason**: .NET の `ActivitySource` の仕様・ベストプラクティスとしては、シングルトンかつ Static なフィールドとして保持することが推奨されており、DI 経由では過剰なオブジェクトアロケーションとパフォーマンス劣化を招くため。

### Option 2: 共通基底クラスの提供

- **Approach**: `DiagnosticsBase` のような基底クラスを用意し、すべてのサービスクラスにそれを継承させる。
- **Rejected Reason**: C# は単一継承（Single Inheritance）しかサポートしていないため、開発者がすでに別の基底クラス（例: `AuthorizationHandler<T>`）を継承している場合に詰んでしまうため（拡張性の限界）。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - `partial` キーワードと属性の付与だけで良いため、開発者はビジネスロジックに集中しつつ、完全な型安全と最適化されたパフォーマンスでトレーシングを導入できます。
    - テレメトリーの識別子（名前とバージョン）が中央でコントロールされ、運用監視プラットフォーム側でのデータ品質が劇的に向上します。
- **Negative**:
    - `partial class` であることが必須となるため、他のライブラリや規約で partial を禁じているケースとは競合する可能性があります。
- **Mitigation**:
    - プロジェクトのコーディング規約（Lint ルール等）において、可観測性を要求されるサービス層・インフラ層のクラスは `sealed partial class` とすることを推奨・標準化します。
