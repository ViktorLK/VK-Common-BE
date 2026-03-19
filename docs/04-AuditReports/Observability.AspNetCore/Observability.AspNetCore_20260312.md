# Architecture Audit Report: Observability.AspNetCore (2026-03-12)

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 85/100
- **対象レイヤー判定**: Infrastructure Layer (Cross-cutting Concerns / ASP.NET Core Middleware)
- **総評 (Executive Summary)**:
  `VK.Blocks.Observability.AspNetCore` モジュールは、ASP.NET Core パイプラインに対する HTTP リクエストロギング、トレースコンテキスト伝播、メトリクス収集を提供する Infrastructure ブロックです。ミドルウェアパイプラインの責務分離（`TraceContextMiddleware` / `RequestLoggingMiddleware`）、`LoggerMessage.Define` によるゼロアロケーション構造化ログ、正規表現ベースの `SensitiveDataRedactor`、および OpenTelemetry Semantic Conventions 準拠の `HttpMetricsCollector` など、運用品質を意識した実装が多数確認されました。一方で、いくつかの設計上の懸念が検出されています: `AspNetCoreExtensions` における再帰呼び出し（メソッド名の衝突）、`RequestLoggingOptions` の `class` 宣言（`sealed record` 推奨）、`HttpLogEnricher` のスコープ辞書におけるマジックストリング、およびレスポンスボディキャプチャ時の `MemoryStream` リソース管理に関する改善余地があります。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[再帰呼び出し — AspNetCoreExtensions.AddAspNetCoreInstrumentation]**: [AspNetCoreExtensions.cs:8-11](/src/BuildingBlocks/Observability.AspNetCore/Extensions/AspNetCoreExtensions.cs#L8-L11)
    - `TracerProviderBuilder.AddAspNetCoreInstrumentation()` 拡張メソッドが、その内部で `builder.AddAspNetCoreInstrumentation()` を呼び出しています。これは **無限再帰** を引き起こし、`StackOverflowException` でアプリケーションがクラッシュします。同名の OpenTelemetry 公式拡張メソッドと名前が衝突しており、自分自身を呼び出しています。
    - **同様の問題**: `MeterProviderBuilder.AddAspNetCoreInstrumentation()` (L14-17) も同一の再帰パターンです。
    - **深刻度**: 🔴 致命的 — このメソッドが呼び出されると即座にアプリケーションがクラッシュします。

- ❌ **[名前空間の不整合]**: [AspNetCoreExtensions.cs:4](/src/BuildingBlocks/Observability.AspNetCore/Extensions/AspNetCoreExtensions.cs#L4)
    - `namespace VK.Blocks.Observability.Extensions` と宣言されていますが、このファイルは `VK.Blocks.Observability.AspNetCore` プロジェクト内にあります。プロジェクトの `RootNamespace` は `VK.Blocks.Observability.AspNetCore` であり、名前空間が一致しません。
    - **影響**: 名前空間の混乱により、`VK.Blocks.Observability` (基盤モジュール) の名前空間と衝突する可能性があります。Observability 基盤側にも同名の拡張メソッドが存在する場合、コンパイル時のあいまいな参照エラーの原因となります。
    - **深刻度**: 🔴 高

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[PII マスキング — 正規表現ベース] ✅**: [SensitiveDataRedactor.cs:59-77](/src/BuildingBlocks/Observability.AspNetCore/Logging/SensitiveDataRedactor.cs#L59-L77)
    - `RegexOptions.Compiled` と `TimeSpan.FromSeconds(1)` タイムアウトによる安全な正規表現パターン構築が行われています。ReDoS 攻撃に対する防御が適切です。✅
    - **残存リスク (低)**: 正規表現パターンは JSON 文字列値 (`"key": "value"`) のみを対象としています。数値・boolean 値 (`"ssn": 123456789`) はマスキングされません。パターンコメント (L64-65) に「数値/boolean への対応」と記載されていますが、実装には未反映です。

- 🔒 **[ClientIp のログ出力]**: [HttpLogEnricher.cs:45](/src/BuildingBlocks/Observability.AspNetCore/Logging/HttpLogEnricher.cs#L45) / [HttpLogEntry.cs:25](/src/BuildingBlocks/Observability.AspNetCore/Logging/HttpLogEntry.cs#L25)
    - `ClientIp` (`RemoteIpAddress`) がログに記録されます。GDPR/個人情報保護法の管轄下では IP アドレスは PII に該当する場合があります。IP アドレスのマスキングまたはオプトアウト機能の追加を推奨します。
    - **深刻度**: 🟡 中

- 🔒 **[レスポンスボディキャプチャ — MemoryStream の二重読み取り]**: [RequestLoggingMiddleware.cs:108-113, 141-144](/src/BuildingBlocks/Observability.AspNetCore/Middleware/RequestLoggingMiddleware.cs#L108-L113)
    - `responseBodyBuffer` の内容を `originalResponseBody` にコピーした後 (L110-111)、`BuildLogEntryAsync` 内で再度 `Seek(0)` して読み取り (L143-144) を行っています。`finally` ブロック内での実行順序は正しいですが、**大容量レスポンスの場合にメモリ圧迫**が発生する可能性があります。`MaxBodySizeBytes` 制限は `ReadBodyAsync` に適用されますが、`MemoryStream` 自体のキャパシティは無制限です。
    - **深刻度**: 🟡 中

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[テスト容易性 — RequestLoggingMiddleware] ✅**: コンストラクタで `ILogger`, `IOptions<T>`, `PathFilter`, `SensitiveDataRedactor`, `HttpLogEnricher`, `HttpMetricsCollector` を注入。各依存を個別にモック可能であり、単体テストが記述可能です。

- ⚙️ **[テスト容易性 — PathFilter] ✅**: `IReadOnlyList<string>` を直接受け取るコンストラクタオーバーロードにより、テスト時のオプション差し替えが容易です。

- ⚙️ **[テスト容易性 — SensitiveDataRedactor] ✅**: `IReadOnlyList<string>` を直接受け取るコンストラクタオーバーロードにより、テスト時のフィールドリスト差し替えが容易です。

- ⚙️ **[テスト容易性 — HttpMetricsCollector]**: `IDisposable` を正しく実装し、`Meter` のライフタイムを管理しています。ただし、`HttpMetricsCollector` は具象クラスとして DI 登録されており、メトリクス記録のモック差し替えにはインターフェース抽象化が必要です。
    - **提案**: `IHttpMetricsCollector` インターフェースの導入により、テスト時のメトリクス無効化が容易になります。

- ⚙️ **[密結合 — HttpLogEnricher]**: `ILogger<HttpLogEnricher>` に依存していますが、`BeginScope` メソッドの返り値は `IDisposable?` であり、テスト時にスコープの開始/終了を検証するには `ILogger` のモックが必要です。現在の設計は適切です。✅

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[TraceId 伝播 — 完全準拠] ✅**: `TraceContextMiddleware` が W3C `traceparent` ヘッダーからの読み取りとレスポンスヘッダーへの `x-trace-id` / `x-request-id` 付与を実行。OpenTelemetry SDK との統合も考慮されています。

- 📡 **[構造化ログ — ゼロアロケーション] ✅**: [RequestLoggingMiddleware.cs:29-39](/src/BuildingBlocks/Observability.AspNetCore/Middleware/RequestLoggingMiddleware.cs#L29-L39)
    - `LoggerMessage.Define<T1, T2, T3, T4>` によるコンパイル時テンプレートキャッシュ。高スループット環境でのパフォーマンスに配慮された実装です。

- 📡 **[メトリクス — OpenTelemetry Semantic Conventions 準拠] ✅**: [HttpMetricsCollector.cs:37-50](/src/BuildingBlocks/Observability.AspNetCore/Metrics/HttpMetricsCollector.cs#L37-L50)
    - `http.server.request.duration`, `http.server.request.count`, `http.server.error.count` のメトリクス名が OpenTelemetry 仕様に準拠。タグも `http.request.method`, `url.path`, `http.response.status_code` を使用。

- 📡 **[ミドルウェア順序の明示] ✅**: `MiddlewareOrder` 定数クラスにより、`TraceContext (-2000)` → `RequestLogging (-1000)` の順序が明示されています。

- 📡 **[LogLevel の動的制御]**: `RequestLoggingOptions.LogLevel` / `ErrorLogLevel` プロパティが定義されていますが、`RequestLoggingMiddleware.LogEntry()` 内では使用されていません。`LoggerMessage.Define` のログレベルがハードコードされています (`Information` / `Warning`)。
    - **影響**: オプションで設定したログレベルが無視されます。Dead Code に近い状態です。
    - **深刻度**: 🟡 中

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[Options クラスの型宣言 — class vs sealed record]**: [RequestLoggingOptions.cs:6](/src/BuildingBlocks/Observability.AspNetCore/Options/RequestLoggingOptions.cs#L6) / [TraceContextOptions.cs:6](/src/BuildingBlocks/Observability.AspNetCore/Options/TraceContextOptions.cs#L6)
    - 両 Options クラスが `public sealed class` + `set` プロパティで宣言されています。VK.Blocks Rule 15 では Options/DTO は `sealed record` + `init` プロパティが推奨されます。ただし、`IOptions<T>` パターンの `Configure<T>(Action<T>)` は `set` プロパティを必要とするため、**Options Pattern との互換性のために `class` + `set` を使用するのは合理的** です。
    - **深刻度**: 🟢 情報レベル — Options Pattern の制約上、現在の設計は許容されます。

- ⚠️ **[マジックストリング — HttpLogEnricher スコープキー]**: [HttpLogEnricher.cs:38-45](/src/BuildingBlocks/Observability.AspNetCore/Logging/HttpLogEnricher.cs#L38-L45)
    - `"Http.Method"`, `"Http.Path"`, `"Http.Scheme"`, `"Http.Host"`, `"TraceId"`, `"SpanId"`, `"RequestId"`, `"ClientIp"` が文字列リテラルとして使用されています。
    - **VK.Blocks Rule 13 違反**: これらは定数クラス（例: `HttpLogPropertyNames`）に集約すべきです。
    - **深刻度**: 🟡 中

- ⚠️ **[マジックストリング — HttpMetricsCollector タグキー]**: [HttpMetricsCollector.cs:66-68](/src/BuildingBlocks/Observability.AspNetCore/Metrics/HttpMetricsCollector.cs#L66-L68)
    - `"http.request.method"`, `"url.path"`, `"http.response.status_code"`, `"error.type"` がリテラルとして使用されています。OpenTelemetry Semantic Conventions のキー名は SDK 側の定数を参照するか、ローカル定数クラスへの集約が望ましいです。
    - **深刻度**: 🟡 低

- ⚠️ **[XML ドキュメント不足 — AspNetCoreExtensions]**: [AspNetCoreExtensions.cs:6-8](/src/BuildingBlocks/Observability.AspNetCore/Extensions/AspNetCoreExtensions.cs#L6-L8)
    - クラス・メソッドレベルの XML ドキュメントコメントが一切ありません。公開 API として他の開発者が使用する可能性があるため、`/// <summary>` の追加が必要です。
    - **深刻度**: 🟡 低

- ⚠️ **[LogLevel / ErrorLogLevel の未使用]**: [RequestLoggingOptions.cs:66-73](/src/BuildingBlocks/Observability.AspNetCore/Options/RequestLoggingOptions.cs#L66-L73)
    - `LogLevel` / `ErrorLogLevel` プロパティが定義されていますが、`RequestLoggingMiddleware` の `LoggerMessage.Define` ではログレベルがハードコードされており、これらの Options 値は参照されていません。Dead Code です。
    - **深刻度**: 🟡 中

---

## ✅ 評価ポイント (Highlights / Good Practices)

- ✅ **LoggerMessage.Define パターン**: `RequestLoggingMiddleware` でゼロアロケーション構造化ログを実現。高スループット環境での GC 圧力を最小化。
- ✅ **SensitiveDataRedactor**: `RegexOptions.Compiled` + タイムアウト付き正規表現により、ReDoS 耐性とパフォーマンスを両立。フィールド名リストは設定可能。
- ✅ **PathFilter**: 除外パスの Prefix マッチングにより、ヘルスチェック・Swagger 等のノイズログを効率的に除外。
- ✅ **HttpMetricsCollector**: OpenTelemetry Semantic Conventions 準拠のメトリクス名・タグ。`IDisposable` による `Meter` ライフタイム管理も適切。
- ✅ **MiddlewareOrder 定数クラス**: パイプライン順序を数値定数で明示し、Rule 13 に準拠。
- ✅ **TryAddSingleton による DI 登録**: 多重登録を防止する防御的な DI パターン。
- ✅ **HttpLogEntry の sealed record**: 不変データモデルとして適切。`required` プロパティによる必須フィールドの強制。
- ✅ **TraceContextMiddleware のレスポンスヘッダーガード**: `context.Response.HasStarted` チェックにより、ヘッダー送信後の `InvalidOperationException` を防止。
- ✅ **Sealed Classes**: 全ミドルウェア・サービスクラスに `sealed` を適用。Rule 15 準拠。
- ✅ **ファイルスコープ名前空間**: 全ファイルで一貫して `namespace X;` 構文を使用。
- ✅ **XML ドキュメントコメント**: `AspNetCoreExtensions` を除く全クラスで充実したドキュメントが整備。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| #   | 課題                                           | 対応方針                                                                                                                                                                              | 影響ファイル                                              |
| --- | ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| 1   | `AspNetCoreExtensions` の再帰呼び出し          | メソッド名を OpenTelemetry 拡張と衝突しない名前に変更（例: `AddVKAspNetCoreTracing` / `AddVKAspNetCoreMetrics`）、または OpenTelemetry の拡張メソッドを明示的に名前空間修飾で呼び出す | `AspNetCoreExtensions.cs`                                 |
| 2   | `AspNetCoreExtensions` の名前空間修正          | `VK.Blocks.Observability.Extensions` → `VK.Blocks.Observability.AspNetCore.Extensions` に変更                                                                                         | `AspNetCoreExtensions.cs`                                 |
| 3   | `LogLevel` / `ErrorLogLevel` の Dead Code 解消 | `LoggerMessage.Define` のハードコードレベルを Options 値に寄せるか、未使用プロパティを削除                                                                                            | `RequestLoggingOptions.cs`, `RequestLoggingMiddleware.cs` |

### 2. リファクタリング提案 (Refactoring)

| #   | 課題                                             | 対応方針                                                                                         | 優先度 |
| --- | ------------------------------------------------ | ------------------------------------------------------------------------------------------------ | ------ |
| 1   | `HttpLogEnricher` スコープキーの定数化           | `HttpLogPropertyNames` 定数クラスの導入 (Rule 13)                                                | 中     |
| 2   | `HttpMetricsCollector` タグキーの定数化          | OpenTelemetry SDK の `SemanticConventions` 定数を使用、または `MetricsTagNames` 定数クラスの導入 | 低     |
| 3   | `SensitiveDataRedactor` の数値/boolean 対応      | 正規表現パターンを拡張し、`"field": 123` / `"field": true` 形式もマスキング対象にする            | 低     |
| 4   | `ClientIp` ログのオプトアウト機能                | `RequestLoggingOptions` に `LogClientIp` フラグを追加し、GDPR 準拠を容易にする                   | 中     |
| 5   | `IHttpMetricsCollector` インターフェースの導入   | テスト時のメトリクス無効化・モック差し替えを可能にする                                           | 低     |
| 6   | `AspNetCoreExtensions` への XML ドキュメント追加 | 公開 API としてのドキュメント整備                                                                | 低     |

### 3. 推奨される学習トピック (Learning Suggestions)

- **ASP.NET Core Middleware Pipeline**: ミドルウェアの登録順序と `UseMiddleware<T>` の解決メカニズム。`MiddlewareOrder` をフレームワークレベルで強制する `IStartupFilter` パターン。
- **OpenTelemetry Semantic Conventions**: `OpenTelemetry.SemanticConventions` NuGet パッケージが提供する定数クラスの活用。マジックストリングの排除。
- **LoggerMessage Source Generator**: .NET 6+ の `[LoggerMessage]` 属性による `partial` メソッドベースのログ生成。`LoggerMessage.Define` に代わるモダンなアプローチ。
- **IOptionsMonitor\<T\> によるホットリロード**: `IOptions<T>` ではなく `IOptionsMonitor<T>` を使用することで、ランタイム中の設定変更を検知するパターン。

---

**Audit Status**: ⚠️ CONDITIONALLY PASSED
**Compliance Score**: 85/100
**Auditor**: VK.Blocks Architect (Automated)
**Date**: 2026-03-12
**Previous Audit**: なし（初回監査）
