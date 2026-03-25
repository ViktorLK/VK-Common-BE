# ADR 004: Integration of Standard Observability Patterns using VKBlockDiagnostics

- **Date**: 2026-03-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Blob Module Optimization

## 2. Context (背景)

Blob ストレージ操作（アップロード、ダウンロード、削除、ディレクトリ操作、リース等）のパフォーマンス監視と分散トレーシングの可視化は、分散システムにおけるエラー調査やボトルネック特定に不可欠です。VK.Blocks では、すべての基盤モジュールにおいて標準化された OpenTelemetry (OTLP) パターンを採用しています。

## 3. Problem Statement (問題定義)

各サービスが独自の手法でログやトレースを出力すると、以下の問題が生じます：
1.  **分離の欠如**: 抽象（Contract）と実装（Azure SDK）が同じソース名で出力されると、エラーの発生箇所（インターフェース呼び出しのミスか、クラウドプロバイダーの内部エラーか）の切り分けが難しくなる。
2.  **一貫性の欠如**: トレースタグの名前や形式がモジュールごとに異なり、ダッシュボードでの分析が困難になる。
2.  **ボイラープレートの増加**: すべてのメソッドに `Activity` や `Meter` の管理コードを手動で書く必要があり、実装ミス（Activity の Dispose 漏れ等）が発生しやすい。

## 4. Decision (決定事項)

`VK.Blocks.Core` で提供されている `[VKBlockDiagnostics]` 属性とソースジェネレーターを活用した計装を決定しました：

1.  **ActivitySource の分離**: `VK.Blocks.Blob` (Facade / 抽象) と `VK.Blocks.Blob.Azure` (実装) の 2 つのソースを定義し、役割を分担。
2.  **自動生成**: 各プロジェクトで `[VKBlockDiagnostics]` ソースジェネレーターを活用。
3.  **標準計装**: `BlobFileService`, `BlobDirectoryService`, `BlobLeaseService` の全主要メソッドに実装プロジェクト用の `Activity` を計装。
4.  **セマンティックタグの付与**: `blob.name`, `blob.container`, `blob.directory` などの標準的なタグを付与。

```csharp
[assembly: VKBlockDiagnostics("VK.Blocks.Blob")]
```

## 5. Alternatives Considered (代替案の検討)

### Option 1: 手動計装 (Manual ActivitySource)
*   **Approach**: 各クラスで `static readonly ActivitySource` を手動定義する。
*   **Rejected Reason**: プロジェクト全体での標準化が難しく、ボイラープレートが増えるため。

### Option 2: ログベースの監視のみ
*   **Approach**: `ILogger` によるログ出力のみに頼る。
*   **Rejected Reason**: サービス間の呼び出し連鎖（分散トレース）を追跡できず、現代的なクラウドネイティブ環境の要件を満たさないため。

## 6. Consequences & Mitigation (結果と緩和策)

*   **Positive**: コンテキスト保持型の診断情報が自動的に収集され、Aspire Dashboard や Jaeger 等で即座に可視化可能になった。
*   **Negative**: 非常に微小なパフォーマンスオーバーヘッドが発生する。
*   **Mitigation**: OpenTelemetry のサンプリング設定および `Activity.IsAllDataRequested` チェックにより、サンプリングされていないトレースの負荷を最小限に抑える。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

PII (個人情報) やシークレット（SAS Token 等）はトレースタグには含めず、デバッグに必要なリソース名のみをメタデータとして付与します。
