# ADR 003: Adopt Wildcard Telemetry Registration and Remove Common Diagnostic Class

**Date**: 2026-03-11  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Observability Module Architecture Optimization

## 2. Context (背景)

以前のアーキテクチャでは、`VK.Blocks.Observability` モジュール内部に、フレームワーク全体を代表する共有の `ActivitySource` と `Meter`（元 `DiagnosticConfig.cs`）を一元的に定義し、OpenTelemetry拡張（`VkObservabilityBuilder.cs`）でその一意の名前（`"VK.Blocks.Observability"`）を監視対象として静的に登録していました。

## 3. Problem Statement (問題定義)

現在の `VK.Blocks` の設計指針では、各サブモジュール（例: Caching, Authentication, ExceptionHandling）が `[VKBlockDiagnostics]` 属性を利用し、ソースジェネレーターによって**自身の独立したテレメトリソースを発行する分散設計**となっています。

しかし、一元的な共有クラスが存在することで以下の問題が生じていました：

1. **Design Mismatch**: 分散ソース設計と中央集権ソース設計が混在し、開発者が「どちらの `ActivitySource` を使うべきか」混乱する原因になっていました。
2. **Scalability Bottleneck**: 新規モジュールを追加する際、個別の `AddSource()` や `AddMeter()` 呼び出しを中央の `VkObservabilityBuilder` に手動で追加しなければならず、OCP（開放閉鎖の原則）に違反していました。
3. **Dead Code**: 実際に `ObservabilityDiagnostics.Source` を利用してスパンを発行している箇所は皆無であり、無用な抽象化となっていました。

## 4. Decision (決定事項)

`VK.Blocks.Observability` 内の共通テレメトリソースクラス（`ObservabilityDiagnostics` 等）を**完全に廃止**します。

それに伴い、OpenTelemetry のソースおよびメーターの監視登録設定を静的な名前ベースから**ワイルドカード指定**（`tracing.AddSource("VK.Blocks.*")`, `metrics.AddMeter("VK.Blocks.*")`）へ変更します。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: リフレクションを用いてすべての `ActivitySource` を動的に登録する**
    - **Approach**: 起動時にロード済みアセンブリをスキャンし、すべての `ActivitySource` フィールドを見つけて登録する。
    - **Rejected Reason**: コールドスタート時のスキャンコストが高く、AOTコンパイルやトリミング（Trimming）との相性も悪いため。
- **Option 2: 各モジュールごとに専用の DI 拡張メソッド（`AddCachingObservability`等）を用意する**
    - **Approach**: 各モジュールが自分自身の `AddSource()` を呼び出す設定メソッドを提供する。
    - **Rejected Reason**: 設定が冗長になり、呼び出し漏れによってテレメトリが欠落するリスクがあるため。ワイルドカードのほうがシンプルで確実です。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 新しい `VK.Blocks` モジュールを追加しても、名前空間さえ一致していれば（`VK.Blocks.XXXX`）自動的にテレメトリ収集の対象となり、**完全なゼロコンフィグレーション**が実現されます。
    - OCPに準拠し、ビルダーのコードが変更に対して閉じ、拡張に対して開いた状態になります。
- **Negative**:
    - 意図せぬサードパーティ製ライブラリがたまたま `VK.Blocks.*` から始まるSource名を使用していた場合、意図せずテレメトリがキャプチャされてノイズになるリスクがあります（ただし、実質的に可能性はゼロに等しい）。
- **Mitigation**:
    - `FieldNames.cs` などの標準コンベンションに従い、ソース名のプレフィックスを厳格に管理する運用ルールを継続します。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Implementation details**:
    ```csharp
    // VkObservabilityBuilder.cs
    tracing.AddSource("VK.Blocks.*");
    metrics.AddMeter("VK.Blocks.*");
    ```
- **Security**:
  ワイルドカード指定により未承認のコンポーネントからのスパンが混入する可能性はありますが、Baggage等を通じて過剰な環境変数が漏洩する等の直接的なセキュリティリスクには繋がりません。
