# アーキテクチャ監査レポート — VK.Blocks.Observability.OpenTelemetry

| 項目                   | 値                                       |
| ---------------------- | ---------------------------------------- |
| **対象モジュール**     | `VK.Blocks.Observability.OpenTelemetry`  |
| **監査日**             | 2026-03-15                               |
| **監査対象ファイル数** | 12 (.cs) + 1 (.csproj)                   |
| **対象レイヤー判定**   | Infrastructure Layer / SDK Configuration |

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **92 / 100**
- **総評 (Executive Summary)**:

前回監査 (2026-03-12: 82点) からの改善が顕著である。レガシー API (`AddAppTracing`, `AddAppMetrics`) の削除によりパブリック API サーフェスが大幅に縮小され、セキュリティリスク (S-01) とコード品質リスク (Q-02) が同時に解消された。`VkObservabilityBuilder` による Fluent Builder パターンが唯一のエントリポイントとなり、API の一貫性が向上している。

主な改善点:
1. **Type Segregation 完全準拠**: `ValidateOtlpOptions` の独立ファイル分離完了
2. **定数の統一管理**: `OpenTelemetryConstants` および `OtlpOptionsConstants` によるマジックストリング排除
3. **テスト容易性の向上**: `IEnvironmentProvider` インターフェース導入による `VkResourceBuilder` のモック可能化
4. **`sealed` 修飾子の適用**: 全クラス (ビルダー、オプション、バリデータ、プロバイダ) に適用済み

残存する改善項目は軽微であり、アーキテクチャの健全性は高い水準にある。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

該当なし。前回指摘の C-01〜C-03 は全て解消済み。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 S-01: `OpenTelemetryExtensions.cs` — 不要な using ディレクティブの残存

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L4-L6)

レガシーメソッド (`AddAppTracing`, `AddAppMetrics`) の削除後、以下の using ディレクティブが未使用となっている:

```csharp
using OpenTelemetry.Metrics;    // L4: MeterProviderBuilder 参照なし
using OpenTelemetry.Resources;  // L5: ResourceBuilder 参照なし
using OpenTelemetry.Trace;      // L6: TracerProviderBuilder 参照なし
```

**影響**: コンパイルには影響しないが、不要な名前空間依存がコードの意図を曖昧にする。

**修正案**: 未使用の 3 行を削除する。

---

### 🔒 S-02: `OpenTelemetryExtensions.ConfigureOtlpExporter` — `new Uri()` による未検証のパース

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L35)

```csharp
options.Endpoint = new Uri(otlpOptions.Endpoint);
```

`OtlpOptions.Endpoint` が不正な URI の場合、`UriFormatException` がスローされる。`ValidateOtlpOptions` が `Uri.TryCreate()` で検証しているが、`ValidateOtlpOptions` が DI に登録されていない場合（レガシー呼び出し経路）、バリデーションをバイパスする可能性がある。

**リスク**: 低（レガシーメソッド削除済みのため、この内部メソッドが直接呼ばれる経路は限定的）

**推奨**: 防御的プログラミングとして `Uri.TryCreate()` によるガード追加を検討する。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ T-01: ユニットテストプロジェクトの不在

本モジュールにはユニットテストプロジェクトが存在しない。以下の主要クラスにはユニットテストが必要である:

| テスト対象 | テスト内容 |
|-----------|-----------|
| `VkResourceBuilder` | クラウド検出モード別のリソース属性生成（`IEnvironmentProvider` モック利用） |
| `ValidateOtlpOptions` | ServiceName / Endpoint のバリデーションロジック |
| `VkObservabilityBuilder` | Fluent API のメソッドチェーン動作（EnableTracing=false 時のスキップ等） |

**推奨**: `tests/BuildingBlocks/Observability.OpenTelemetry.UnitTests` プロジェクトの新設。

---

### ⚙️ T-02: `VkObservabilityBuilder` コンストラクタの即時副作用

**ファイル**: `Builder/VkObservabilityBuilder.cs` (L59)

```csharp
_otelBuilder = services.AddOpenTelemetry();
```

コンストラクタ内で DI コンテナへの登録副作用が発生する。OpenTelemetry SDK の `AddOpenTelemetry()` がビルダーパターンを返す設計制約上、許容範囲内と判断する。

**リスク**: 低（SDK の設計制約）

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 O-01: `OpenTelemetryExtensions` クラスの XML ドキュメント — レガシー参照の残存

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L11-L17)

```csharp
/// <summary>
/// OpenTelemetry 設定の後方互換拡張メソッド。
/// </summary>
/// <remarks>
/// これらのメソッドは旧レガシー API です。
/// 新規実装には <c>services.AddVkObservability(...)</c> を使用してください。
/// </remarks>
```

レガシーメソッドが削除されたにもかかわらず、クラスの XML ドキュメントが「旧レガシー API」と記述されている。現在のクラスは内部ヘルパー (`ConfigureOtlpExporter`) と 1 つの `[Obsolete]` メソッド (`GetOtlpOptions`) のみを含むため、ドキュメントを更新すべきである。

**修正案**: クラスドキュメントを現在の実態に合わせて更新する。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ Q-01: `GetOtlpOptions` メソッドの存続必要性

**ファイル**: `Extensions/OpenTelemetryExtensions.cs` (L50-L63)

レガシーの `AddAppTracing`/`AddAppMetrics` が削除された今、`GetOtlpOptions` メソッドの存在理由が希薄である。このメソッドは `OtlpOptions` を `IConfiguration` から取得し DI に登録するが、新 API (`VkObservabilityOptions`) は独自の Options Pattern で管理されている。

**推奨**: `OtlpOptions` 関連コード全体（`OtlpOptions.cs`, `ValidateOtlpOptions.cs`, `OtlpOptionsConstants.cs`, `GetOtlpOptions` メソッド）の削除を検討する。外部利用者が存在する場合は、次のメジャーバージョン (v3.0) での削除を計画する。

---

### ⚠️ Q-02: `VkObservabilityOptions.Environment` のデフォルト値 `"Production"`

**ファイル**: `Options/VkObservabilityOptions.cs` (L55)

```csharp
public string Environment { get; set; } = "Production";
```

デフォルト値が `"Production"` に設定されているため、開発環境での設定漏れが `deployment.environment = Production` としてテレメトリに記録されるリスクがある。`ASPNETCORE_ENVIRONMENT` や `DOTNET_ENVIRONMENT` からの自動検出、またはバリデーションでの未設定検出を検討すべきである。

**リスク**: 低（テレメトリ属性の正確性への影響）

---

### ⚠️ Q-03: `VkResourceBuilder` のリソース属性キーにおけるマジックストリング

**ファイル**: `Resources/VkResourceBuilder.cs` (L57, L117-L122, L132-L138, L145)

`"deployment.environment"`, `"cloud.provider"`, `"azure.app_service.site_name"` などの OpenTelemetry セマンティック規約キーがリテラル文字列として使用されている。

```csharp
["deployment.environment"] = options.Environment    // L57
["cloud.provider"] = "azure"                        // L117
["cloud.provider"] = "kubernetes"                   // L134
```

これらは OpenTelemetry セマンティック規約で定義された標準属性名であり、定数化によるタイプセーフティ向上が望ましい。ただし、OpenTelemetry .NET SDK が公式定数を提供していないため、独自定数化の判断はチームに委ねる。

**リスク**: 低（標準規約に基づく文字列のため変更頻度は極めて低い）

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

`VkResourceBuilder` は `CloudDetectionMode` 列挙型と Strategy パターンに基づくクラウド自動検出を実装しており、Azure App Service / Kubernetes の両方に対応する拡張性の高い設計となっている。`IEnvironmentProvider` による抽象化でユニットテスト可能。

### ✅ H-04: `sealed class` の完全適用

全クラスに `sealed` 修飾子が正しく付与されており、Rule 15 に完全準拠している：
- `VkObservabilityBuilder`, `VkObservabilityOptions` (新 API)
- `OtlpOptions`, `ValidateOtlpOptions` (レガシー API)
- `DefaultEnvironmentProvider` (Infrastructure)

### ✅ H-05: Rule 14 (Type Segregation) の完全準拠

全ファイルが「一ファイル一型」原則を遵守している。`OtlpOptions` / `ValidateOtlpOptions` / `OtlpOptionsConstants` がそれぞれ独立ファイルに分離されている。

### ✅ H-06: 定数管理の徹底 (Rule 13)

- モジュール横断定数: `OpenTelemetryConstants`（ワイルドカードソース名、EF Core ActivitySource 名、ヘルスチェック除外パス）
- フィーチャー内定数: `OtlpOptionsConstants`（デフォルトサービス名）

### ✅ H-07: 包括的な XML ドキュメント

全クラス・メソッドに XML ドキュメントが記述されており、`<example>` タグによる使用例も充実している。`ConfigureOtlpExporter` にはセキュリティ注意書きが含まれている。

### ✅ H-08: レガシー API の計画的削除

`GetOtlpOptions` メソッドに `[Obsolete]` 属性で `v3.0` での削除をスケジュールしており、後方互換性と漸進的移行を両立している。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| #   | 対応内容                                                                 | 根拠                          |
| --- | ------------------------------------------------------------------------ | ----------------------------- |
| 1   | `OpenTelemetryExtensions.cs` の未使用 using ディレクティブを削除         | S-01: コード衛生              |
| 2   | `OpenTelemetryExtensions` クラスの XML ドキュメントを更新                | O-01: レガシー参照の残存      |

### 2. リファクタリング提案 (Refactoring)

| #   | 対応内容                                                                                   | 根拠                                     |
| --- | ------------------------------------------------------------------------------------------ | ---------------------------------------- |
| 3   | `OtlpOptions` 関連コード全体の削除（v3.0 スケジュール）                                   | Q-01: レガシーコードの存続必要性         |
| 4   | `VkObservabilityOptions.Environment` のデフォルト値を再検討                                 | Q-02: テレメトリ属性の正確性             |
| 5   | `VkResourceBuilder` のリソース属性キーを定数化（OpenTelemetry セマンティック規約準拠）     | Q-03: マジックストリング                 |

### 3. 推奨される学習トピック (Learning Suggestions)

| #   | トピック                                     | 理由                                                                                                           |
| --- | -------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 1   | **OpenTelemetry .NET — ILogger Integration** | `EnableLogging` フラグが定義済みだが未実装。OTLP Log Exporter の統合方法を理解し、3 シグナル統合を完成させる。 |
| 2   | **Testcontainers for OpenTelemetry**         | `VkResourceBuilder` や Fluent Builder の統合テストを Testcontainers + OTLP Collector で自動化する方法。        |
| 3   | **OpenTelemetry Semantic Conventions SDK**   | `OpenTelemetry.SemanticConventions` パッケージによる標準属性キーの型安全な参照。                               |
