# Task: アーキテクチャ監査レポート (Observability Audit)

**監査日**: 2026-03-10
**前回監査日**: 2026-03-06 (スコア: 92/100)

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 88/100点 (前回比 -4pt ※評価基準をVK.Blocks全15ルールに拡張)
- **対象レイヤー判定**: Infrastructure Layer / Observability Block (Core Abstraction)
- **対象ファイル数**: 11ファイル (.cs)
- **テストカバレッジ**: 7テストファイル (FieldNames, DiagnosticConfig, ActivityLogContextEnricher, ApplicationEnricher, TraceContextEnricher, UserContextEnricher, ActivityExtensions)
- **総評 (Executive Summary)**:
  本モジュールは OpenTelemetry と Serilog を基盤とした計装抽象レイヤーとして高い完成度を持ち、Strategy パターンによるエンリッチャー設計、`IResult` パターンとの透過的なブリッジ（`ActivityExtensions`）、PII 制御（`IncludeUserName`）など、エンタープライズグレードの設計思想が随所に反映されている。
  しかし、前回レポート（2026-03-06）で指摘された **`Source12` の残存** と **重複定数** が **未修正のまま放置** されており、加えて今回の全15ルールベースの深掘りにより、**型分離違反**、**Options クラスの `sealed record` 非準拠**、**`ObservabilityBlockExtensions` の IConfiguration バインディング欠如** など、新たな改善点が浮上した。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ CS-01: デバッグコードの残存 — `Source12` (前回未修正 / 最優先)

- **ファイル**: `DiagnosticConfig.cs` L19
- **内容**: `public static readonly ActivitySource Source12 = CachingDiagnostics.Source;`
- **影響**: `CachingDiagnostics` はソースジェネレーターにより `Source` と `Meter` を自動生成する設計だが、`Source12` というフィールドは明らかにデバッグ時の一時コードであり、**意味のないパブリック API サーフェスの汚染** を引き起こしている。また、`CachingDiagnostics` クラス自体が Observability モジュール内に定義されている理由が不明確であり、Caching モジュール側に配置すべきである。
- **修正案**:
    1. `Source12` フィールドを即座に削除
    2. `CachingDiagnostics` クラスを Caching モジュールに移動、もしくは削除

### ❌ CS-02: 型分離違反 (Rule 14) — 1ファイル複数型

- **ファイル**: `DiagnosticConfig.cs`
- **内容**: `CachingDiagnostics` (internal) と `DiagnosticConfig` (public) の2つの主要型が同一ファイルに定義されている。
- **ルール違反**: Rule 14「One File, One Type」に違反。`CachingDiagnostics` は Observability の責務外であり、SRP にも抵触する。
- **修正案**: `CachingDiagnostics` を別ファイル（理想的には Caching モジュール）に分離。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 SEC-01: PII保護 ✅

- `UserContextEnricher` において、`ObservabilityOptions.IncludeUserName` によりユーザー名のログ制御がデフォルト無効化されている。GDPR/個人情報保護法への設計レベルの配慮が確認済み。

### ⚡ PERF-01: ゼロアロケーションガード ✅

- `ActivityExtensions.RecordResult` にて、`activity is null` ガードによりリスナー不在時のタグ生成がスキップされる。
- `ActivityLogContextEnricher` にて、`NullScope.Instance` シングルトンにより Activity 不在時のアロケーションを抑制。

### ⚡ PERF-02: `ActivitySource` / `Meter` の Disposal 管理

- **ファイル**: `DiagnosticConfig.cs` L72, L78
- **リスク**: `ActivitySource` と `Meter` はいずれも `IDisposable` を実装しているが、`static readonly` フィールドとして保持されているため、DI コンテナを通じた明示的 Dispose ができない。`.AddSingleton(DiagnosticConfig.ActivitySource)` で登録されているが、`ActivitySource` の寿命管理は DI コンテナの責務外になる可能性がある。
- **推奨**: XML ドキュメントで既に「DI container at application shutdown」と言及されているが、実際には `static readonly` のため DI コンテナは Dispose しない。アプリケーション終了時の Dispose を `IHostApplicationLifetime` 経由で明示的にフックするか、ドキュメントを実態に合わせて修正することを推奨。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ TEST-01: インターフェース設計 ✅

- `ILogEnricher` / `ILogContextEnricher` による抽象化で、ロギングプロバイダーとの疎結合が確保されている。`ApplicationEnricher`, `UserContextEnricher`, `TraceContextEnricher` すべてに対応するユニットテストが存在。

### ⚙️ TEST-02: テストカバレッジの網羅性 ✅

- 主要クラスごとにテストファイルが存在（7/11ファイル）。Options クラスとインターフェースはテスト対象外として妥当。

### ⚙️ TEST-03: `ObservabilityBlockExtensions` のテスト欠如 ⚠️

- DI 登録の正確性（サービスの型・ライフタイム検証）に対するテストが存在しない。`AddObservabilityBlock` メソッドの登録内容をアサートする Integration Test の追加を推奨。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 OBS-01: セマンティックコンベンション準拠 ✅

- `FieldNames` クラスにより OpenTelemetry セマンティックコンベンション（`service.name`, `deployment.environment`, `http.request.method` 等）に準拠。VK 独自プレフィックス（`vk.user.id`, `vk.tenant.id`）も一貫して管理されている。

### 📡 OBS-02: Result ↔ Span ブリッジ ✅

- `ActivityExtensions.RecordResult` により、`IResult` のSuccess/Failure が自動的に OpenTelemetry スパンにマッピングされる優れた設計。エラーコード・メッセージ・タイプの自動記録と、`ActivityEvent` の付与まで完結している。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ CQ-01: 重複定数の未整理 (前回未修正)

- **ファイル**: `FieldNames.cs` L11, L29
- `DeploymentEnvironment` と `Environment` が同一の `"deployment.environment"` を指している。XML コメントに「deprecated」と記載されているが、`[Obsolete]` 属性が付与されていないため、コンパイル時の警告が発生せず、利用者が誤って `Environment` を使い続ける可能性がある。
- **修正案**: `Environment` に `[Obsolete("Use DeploymentEnvironment instead.")]` を付与する。

### ⚠️ CQ-02: `ObservabilityOptions` が `sealed class` のまま (Rule 15)

- **ファイル**: `Options/ObservabilityOptions.cs`
- VK.Blocks Rule 15 では、すべての DTO/設定は `sealed record` を使用すべきとされている。ただし、`ObservabilityOptions` は DI の `IOptions<T>` パターンと `Configure()` デリゲートによるミュータブル設定を前提としているため、`record` への変換は適用不可。`sealed class` は妥当であるが、プロパティの `init` セッターへの変更を検討すべき。
- **修正案**: `set` を `init` に変更し、`Configure()` パターンとの互換性を検証した上で適用。互換性がない場合は現状維持で妥当。

### ⚠️ CQ-03: `IConfiguration` バインディングの未提供

- **ファイル**: `DependencyInjection/ObservabilityBlockExtensions.cs`
- 現在の `AddObservabilityBlock` は `Action<ObservabilityOptions> configure` のみを受け付けている。`IConfiguration` セクションからのバインディングオーバーロードが存在しないため、`appsettings.json` からの設定読み込みには利用者側でボイラープレートコードが必要になる。
- **修正案**: 以下のオーバーロードを追加
    ```csharp
    public static IServiceCollection AddObservabilityBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    ```

### ⚠️ CQ-04: `FieldNames` の `ResultSuccess` / `ResultFailure` のプレフィックス不整合

- **ファイル**: `FieldNames.cs` L58-61
- `ResultCode` = `"result.code"`, `ResultMessage` = `"result.message"` に対して、`ResultSuccess` = `"result.success"`, `ResultFailure` = `"result.failure"` は一貫性があるが、`ActivityExtensions` のXML ドキュメントでは `vblocks.result.success` というプレフィックスが使用されている（L50: `vblocks.result.success=true`）。ただし、実際のタグ名は `FieldNames.ResultSuccess` = `"result.success"`（`vblocks.` プレフィックスなし）。XML ドキュメントと実装の不整合が存在する。
- **修正案**: XML ドキュメントの `vblocks.` プレフィックスを削除し、実際の `FieldNames` 定数に合わせる。

### ⚠️ CQ-05: `TraceContextEnricher` の `is not null` スタイル

- **ファイル**: `TraceContextEnricher.cs` L18
- `activity != null` が使用されているが、他のファイル（`ActivityLogContextEnricher.cs` L49, `ActivityExtensions.cs` L42）では `activity is null` パターンが使用されている。スタイルの統一が望ましい。
- **修正案**: `activity != null` → `activity is not null` に統一。

---

## ✅ 評価ポイント (Highlights / Good Practices)

| 項目                             | 説明                                                                                                                                |
| -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **Result Pattern Bridge**        | `ActivityExtensions.RecordResult` は `IResult` を透過的に OTel スパンへマッピング。エラーイベントの自動記録まで完結する設計は秀悦。 |
| **Strategy Pattern**             | `ILogEnricher` による Strategy パターン採用により、エンリッチャーの追加・削除が DI 変更のみで完結。OCP に準拠。                     |
| **Source Generator Integration** | `[VKBlockDiagnostics]` + `partial class` による `ActivitySource` / `Meter` 自動生成パターンが確立されている。                       |
| **Null Object Pattern**          | `NullScope.Instance` によるゼロアロケーション設計は、高頻度呼び出しにおけるGC圧力低減に有効。                                       |
| **PII by Design**                | `IncludeUserName` のデフォルト `false` はプライバシーバイデザインの好例。                                                           |
| **DataAnnotation Validation**    | `ValidateDataAnnotations()` + `ValidateOnStart()` により、起動時の設定検証が確保されている。                                        |
| **Defensive Coding**             | 全拡張メソッドが `null` チェック付きで、安全にスキップする設計。                                                                    |

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| #   | 対象ファイル          | 修正内容                                                                 | 重要度      |
| --- | --------------------- | ------------------------------------------------------------------------ | ----------- |
| 1   | `DiagnosticConfig.cs` | `Source12` フィールドを削除                                              | 🔴 Critical |
| 2   | `DiagnosticConfig.cs` | `CachingDiagnostics` クラスを別ファイル/モジュールに分離（Rule 14 準拠） | 🔴 Critical |
| 3   | `FieldNames.cs`       | `Environment` 定数に `[Obsolete]` 属性を付与                             | 🟡 High     |

### 2. リファクタリング提案 (Refactoring)

| #   | 対象ファイル                      | 修正内容                                                          | 重要度    |
| --- | --------------------------------- | ----------------------------------------------------------------- | --------- |
| 4   | `ObservabilityBlockExtensions.cs` | `IConfiguration` セクションバインディングのオーバーロード追加     | 🟡 High   |
| 5   | `ActivityExtensions.cs`           | XML ドキュメントの `vblocks.` プレフィックスを削除し実装と整合    | 🟢 Medium |
| 6   | `TraceContextEnricher.cs`         | `!= null` → `is not null` にスタイル統一                          | 🟢 Low    |
| 7   | `DiagnosticConfig.cs`             | `ActivitySource` / `Meter` の Disposal 管理方針をドキュメント更新 | 🟢 Medium |

### 3. テスト強化 (Test Enhancement)

| #   | 対象                           | 修正内容                                | 重要度  |
| --- | ------------------------------ | --------------------------------------- | ------- |
| 8   | `ObservabilityBlockExtensions` | DI 登録の型・ライフタイム検証テスト追加 | 🟡 High |

### 4. 推奨される学習トピック (Learning Suggestions)

- **OpenTelemetry Propagator**: B3 / W3C TraceContext の動作原理と、Message Broker 等を跨ぐ際のトレース継続性確保手法
- **`IOptionsFactory<T>` のカスタマイズ**: 複合ソース（コード + JSON + 環境変数）からの Options 構築パターン
- **Metrics Best Practices**: `Meter` を用いた Custom Histogram / Counter の命名規約と、カーディナリティ爆発の回避手法

---

## 📝 VK.Blocks チェックリスト監査 (15-Rule Compliance)

| Rule | 項目                | 判定 | 所見                                                                                                  |
| ---- | ------------------- | ---- | ----------------------------------------------------------------------------------------------------- |
| R1   | Result Pattern      | ✅   | `ActivityExtensions.RecordResult` で `IResult` パターンを正しくブリッジ                               |
| R2   | Layer Dependencies  | ✅   | Core + Abstractions のみ参照。EF Core / Redis 等の依存なし                                            |
| R3   | Async               | N/A  | 本モジュールに非同期 I/O 操作なし                                                                     |
| R4   | Performance         | ✅   | ゼロアロケーションガード + NullScope シングルトン                                                     |
| R5   | Automation (Audit)  | N/A  | 本モジュールに IAuditable / ISoftDelete なし                                                          |
| R6   | Observability       | ✅   | 構造化ログテンプレート準拠、TraceId 必須化                                                            |
| R7   | Security (Tenant)   | N/A  | テナント分離は本モジュールの責務外                                                                    |
| R8   | Resiliency          | N/A  | 外部呼び出しなし                                                                                      |
| R9   | Testing             | ⚠️   | 主要クラスのテストあり。DI 登録テスト欠如                                                             |
| R10  | Code Generation     | ✅   | 未完成コード・TODO なし                                                                               |
| R11  | ADR Trigger         | N/A  | 新しいインターフェース変更なし                                                                        |
| R12  | Folder Organization | ✅   | 機能ドリブン: Conventions / Enrichment / Extensions / Options                                         |
| R13  | Constant Visibility | ✅   | `FieldNames` (public cross-feature), `DiagnosticConfig` (public global)                               |
| R14  | Type Segregation    | ❌   | `DiagnosticConfig.cs` に2型定義 (CQ-02)                                                               |
| R15  | Modern C# Semantics | ⚠️   | `sealed class` は準拠。`ObservabilityOptions` の `record` 化は Options パターンとの互換性上見送り妥当 |
