# アーキテクチャ監査レポート — VK.Blocks.Observability.OpenTelemetry

| 項目                   | 値                                       |
| ---------------------- | ---------------------------------------- |
| **対象モジュール**     | `VK.Blocks.Observability.OpenTelemetry`  |
| **監査日**             | 2026-03-12                               |
| **監査対象ファイル数** | 9 (.cs) + 1 (.csproj)                    |
| **対象レイヤー判定**   | Infrastructure Layer / SDK Configuration |

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **82 / 100**
- **総評 (Executive Summary)**:

本モジュールは OpenTelemetry SDK の初期化・設定を `VkObservabilityBuilder` による Fluent API で統一的に提供しており、アーキテクチャ品質は概ね良好である。`VkObservabilityOptions` は `DataAnnotations` による宣言的バリデーションを採用し、`VkResourceBuilder` はクラウド環境の自動検出を実装するなど、エンタープライズ品質の設計がなされている。

一方、以下の点で改善の余地がある：

1. **レガシー API (`OpenTelemetryExtensions.cs`) の Type Segregation 違反**と潜在的セキュリティリスク
2. **`OtlpOptions.cs` における Rule 14 (一ファイル一型) 違反**
3. **環境変数への直接依存** によるテスト困難性
4. **マジックストリング** の散在

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ C-01: `OtlpOptions.cs` — Rule 14 (Type Segregation) 違反

**ファイル**: `OpenTelemetry/OtlpOptions.cs` (L8, L62)

`OtlpOptions` クラスと `ValidateOtlpOptions` クラスが同一ファイルに定義されている。VK.Blocks 規約「一ファイル一型」に違反する。

**影響**: ファイル検索時の発見困難性（`ValidateOtlpOptions` を探す際に `OtlpOptions.cs` を開く必要がある）。今後型が増加した場合、ファイルの肥大化を招く。

**修正案**: `ValidateOtlpOptions` を `OpenTelemetry/ValidateOtlpOptions.cs` として独立ファイルに分離する。

---

### ❌ C-02: `OtlpOptions` — Rule 15 (`sealed` 宣言の欠如) 違反

**ファイル**: `OpenTelemetry/OtlpOptions.cs` (L8), `OpenTelemetry/OtlpOptions.cs` (L62)

`OtlpOptions` および `ValidateOtlpOptions` は `sealed` 修飾子が付与されていない。VK.Blocks 規約ではポリモーフィズムが不要なクラスは `sealed class` であることを要求している。

**修正案**: 両クラスに `sealed` を付与する。

---

### ❌ C-03: レガシー API の名前空間不一致

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L9)

レガシー拡張メソッドが `VK.Blocks.Observability.Extensions` 名前空間に配置されているが、モジュールのルート名前空間は `VK.Blocks.Observability.OpenTelemetry` である。名前空間がモジュールフォルダ構造と一致しておらず、利用者の混乱を招く。

**影響**: この名前空間は `Observability.OpenTelemetry` プロジェクト内に存在するにもかかわらず、`Observability` 本体のような印象を与えるため、依存関係の理解を阻害する。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 S-01: OTLP ヘッダーへの認証情報の平文埋め込みリスク

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L96-L105)

レガシー API の `ConfigureOtlpExporter` メソッドにおいて、`OtlpOptions.Headers` ディクショナリの内容が文字列連結でそのまま OTLP ヘッダーに設定される。認証トークンや API キーが含まれている場合、ログ出力やデバッグ時に平文で露出するリスクがある。

```csharp
// L104: トークン値がそのままヘッダー文字列に含まれる
options.Headers += $"{header.Key}={header.Value}";
```

**推奨**: ヘッダー値のログ出力を `SensitiveDataProcessor` でマスク処理するか、認証ヘッダーの構築を専用クラスに委譲し、デバッグ出力時にマスクする機構を導入する。

---

### 🔒 S-02: `ConfigureOtlpExporter` メソッドの不要な `public` アクセス修飾子

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L89)

`ConfigureOtlpExporter` は内部ヘルパーメソッドであるにもかかわらず `public static` として公開されている。パブリック API サーフェスの不必要な拡大は、破壊的変更のリスクを増大させる。

**修正案**: `internal static` に変更する。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ T-01: `VkResourceBuilder` の `System.Environment` 直接依存

**ファイル**: `Resources/VkResourceBuilder.cs` (L44, L95, L100, L139-L140, L152)

`VkResourceBuilder.Build()` は `System.Environment.GetEnvironmentVariable()` を 6 箇所で直接呼び出している。この設計ではユニットテスト時に環境変数の注入・モック化が困難であり、テスト間の状態汚染リスクがある。

**推奨**: 環境変数読み取りを `IEnvironmentProvider` インターフェースで抽象化し、テスト時にモック可能にする。または、`VkObservabilityOptions` に対応するプロパティを追加して、環境変数のフォールバックとして扱う。

```csharp
// 理想的な設計例
public interface IEnvironmentProvider
{
    string? GetVariable(string name);
}
```

---

### ⚙️ T-02: `VkObservabilityBuilder` コンストラクタでの即時副作用

**ファイル**: `Builder/VkObservabilityBuilder.cs` (L57)

コンストラクタ内で `services.AddOpenTelemetry()` を呼び出しており、DI コンテナへの即時登録副作用が発生する。これはユニットテスト時に `IServiceCollection` のモック構築を必要とし、テストの複雑性を増す。

**影響**: Builder パターンの慣例に従い、設定の蓄積（構成段階）を先に行い、最終的な DI 登録は `Build()` メソッドで実施するのがより堅牢。ただし、OpenTelemetry SDK の `AddOpenTelemetry()` がビルダーパターン自体を返す設計であるため、現行の実装は許容範囲内と判断する。

**リスク**: 低（OpenTelemetry SDK の制約上の設計）

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 O-01: ヘルスチェックフィルターのマジックストリング

**ファイル**: `Builder/VkObservabilityBuilder.cs` (L178-L180)

ヘルスチェックパスが `"/health"`, `"/healthz"`, `"/ready"` としてハードコーディングされている。VK.Blocks 規約 Rule 13（定数の可視性）に基づき、これらを定数として抽出すべきである。

```csharp
// 現行: マジックストリング
o.Filter = context =>
    !context.Request.Path.StartsWithSegments("/health") &&
    !context.Request.Path.StartsWithSegments("/healthz") &&
    !context.Request.Path.StartsWithSegments("/ready");
```

**修正案**: `HealthCheckConstants` クラス、または `VkObservabilityOptions` に `ExcludedPaths` プロパティとして抽出する。

---

### 📡 O-02: ワイルドカードソース名のマジックストリング

**ファイル**: `Builder/VkObservabilityBuilder.cs` (L85, L133)

`"VK.Blocks.*"` がトレーシングとメトリクスの両方でリテラル文字列として使用されている。Rule 13 に従いクロスファイル定数として抽出すべきである。

---

### 📡 O-03: EF Core ActivitySource 名のマジックストリング

**ファイル**: `Builder/VkObservabilityBuilder.cs` (L228)

`"Microsoft.EntityFrameworkCore"` がリテラル文字列として使用されている。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ Q-01: `OtlpOptions.Endpoint` のバリデーションにおける不整合

**ファイル**: `OpenTelemetry/OtlpOptions.cs` (L24, L67, L93)

- `OtlpOptions.Endpoint` はデフォルト値 `"http://localhost:4317"` が設定されている。
- `ValidateOtlpOptions.Validate()` は `string.IsNullOrWhiteSpace` のみをチェックしている。
- レガシー API の `ConfigureOtlpExporter` (L93) は `new Uri(otlpOptions.Endpoint)` で無検証の URI パースを行っており、不正な URI が渡された場合に `UriFormatException` がスローされる。

**推奨**: `ValidateOtlpOptions` 内で `Uri.TryCreate()` による URI フォーマット検証を追加する。

---

### ⚠️ Q-02: レガシー API の `AddOpenTelemetry()` 重複呼び出しリスク

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L33, L66)

`AddAppTracing` と `AddAppMetrics` がそれぞれ独立して `services.AddOpenTelemetry()` を呼び出している。両方を呼ぶと `OpenTelemetryBuilder` が 2 回作成される。OpenTelemetry SDK は内部的に冪等処理を持つが、設計意図として単一のビルダーを共有すべきである（新 API では `VkObservabilityBuilder` で統一されており修正済み）。

**対応**: レガシー API は `[Obsolete]` でマークされているため、削除スケジュールの策定を推奨する。

---

### ⚠️ Q-03: `VkObservabilityOptions` のデフォルト値 `"Unknown"` のバリデーション不整合

**ファイル**: `Options/VkObservabilityOptions.cs` (L38)

`ServiceName` は `[Required, MinLength(1)]` バリデーション付きであるが、デフォルト値 `"Unknown"` がバリデーションを通過してしまう。一方、レガシー側の `ValidateOtlpOptions` (L67) は `"UnknownService"` を明示的に拒否する実装になっている。統一性に欠ける。

**推奨**: `VkObservabilityOptions.ServiceName` のデフォルト値を空文字 `""` にするか、カスタムバリデーションで `"Unknown"` を拒否する。

---

### ⚠️ Q-04: `OtlpOptions` に `sealed` 不足 / `sealed record` 推奨

レガシー `OtlpOptions` はミュータブルな `class` であり、イミュータブルな `sealed record` が推奨される（Rule 15）。ただし、レガシーであり `[Obsolete]` マーク済みのため、優先度は低い。

---

## ✅ 評価ポイント (Highlights / Good Practices)

### ✅ H-01: Fluent Builder パターンの洗練された実装

`VkObservabilityBuilder` は Fluent API のベストプラクティスに忠実に従っている：

- 各メソッドが `this` を返すことでメソッドチェーンを実現
- フラグ無効時のノーオペレーション（`if (!_options.EnableTracing) return this;`）
- 遅延構成パターン（`_tracingConfiguration`, `_metricsConfiguration`）によるインストゥルメンテーション登録の分離

### ✅ H-02: DataAnnotations による宣言的バリデーション

`VkObservabilityOptions` は `[Required]`, `[MinLength]`, `[Range]` 属性を活用し、`ValidateDataAnnotations()` / `ValidateOnStart()` と連携することで、起動時バリデーションを実現している。フェイルファスト原則に準拠した優れた設計。

### ✅ H-03: クラウド環境の自動検出

`VkResourceBuilder` は `CloudDetectionMode` 列挙型と Strategy パターンに基づくクラウド自動検出を実装しており、Azure App Service / Kubernetes の両方に対応する拡張性の高い設計となっている。

### ✅ H-04: `sealed class` の適用 (新 API)

新 API の中核クラス群（`VkObservabilityBuilder`, `VkObservabilityOptions`）は `sealed` 修飾子が正しく付与されており、Rule 15 に準拠している。

### ✅ H-05: レガシー API の `[Obsolete]` マーキング

旧 API (`OpenTelemetryExtensions`) は全メソッドに `[Obsolete]` 属性を付与し、新 API への移行パスを明示的に案内しており、後方互換性と漸進的移行の両立を実現している。

### ✅ H-06: 包括的な XML ドキュメント

全クラス・メソッドに XML ドキュメントが記述されており、`<example>` タグによる使用例も充実している。開発者エクスペリエンスへの配慮が見受けられる。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| #   | 対応内容                                                     | 根拠                      |
| --- | ------------------------------------------------------------ | ------------------------- |
| 1   | `OtlpOptions.cs` から `ValidateOtlpOptions` を分離           | C-01: Rule 14 違反        |
| 2   | `OtlpOptions`, `ValidateOtlpOptions` に `sealed` 付与        | C-02: Rule 15 違反        |
| 3   | `ConfigureOtlpExporter` のアクセス修飾子を `internal` に変更 | S-02: 不必要な public API |

### 2. リファクタリング提案 (Refactoring)

| #   | 対応内容                                                                                            | 根拠                                                                   |
| --- | --------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| 4   | ヘルスチェックパス・ワイルドカードソース名・EF Core ActivitySource 名を定数化                       | O-01〜O-03: Rule 13 マジックストリング                                 |
| 5   | `VkObservabilityOptions.ServiceName` のデフォルト値を空文字に変更、またはカスタムバリデーション追加 | Q-03: バリデーション不整合                                             |
| 6   | `ValidateOtlpOptions` に `Uri.TryCreate()` による Endpoint フォーマット検証を追加                   | Q-01: URI パース例外リスク                                             |
| 7   | `VkResourceBuilder` の環境変数読み取りを `IEnvironmentProvider` で抽象化                            | T-01: テスト容易性向上                                                 |
| 8   | レガシー API の名前空間を `VK.Blocks.Observability.OpenTelemetry.Extensions` に統一                 | C-03: 名前空間不一致（破壊的変更のためマイナーバージョンアップで対応） |

### 3. 推奨される学習トピック (Learning Suggestions)

| #   | トピック                                     | 理由                                                                                                           |
| --- | -------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 1   | **OpenTelemetry .NET — ILogger Integration** | `EnableLogging` フラグが定義済みだが未実装。OTLP Log Exporter の統合方法を理解し、3 シグナル統合を完成させる。 |
| 2   | **Options Pattern — Named Options**          | レガシーとの共存期間中に Named Options を活用することで、同一型の複数構成をクリーンに管理できる。              |
| 3   | **Testcontainers for OpenTelemetry**         | `VkResourceBuilder` や Fluent Builder の統合テストを Testcontainers + OTLP Collector で自動化する方法。        |
