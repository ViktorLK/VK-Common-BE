# ADR 002: Chain of Responsibility Pattern for Exception Handling

- **Date**: 2026-03-17
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.ExceptionHandling

## 2. Context (背景)

ASP.NET Core アプリケーションにおける例外ハンドリングは、歴史的にミドルウェア内に巨大な `try-catch` ブロックを設けるか、`ExceptionHandlerMiddleware` 内で巨大な `switch` 文を用いて例外の型ごとに処理を分岐させるアプローチが取られがちである。
機能が拡張され、ドメイン固有の例外や外部システム連携に伴う例外のバリエーションが増えるたびに、中央のハンドリングロジックが肥大化し続けていた。

## 3. Problem Statement (問題定義)

単一のクラス・メソッドで全例外の振り分けとレスポンス生成を行うアプローチには、以下の問題が存在する：

1. **SRP (単一責任の原則) および OCP (開放/閉鎖の原則) の違反**:
   新しい種類の例外処理を追加するたびに既存のコアミドルウェアのコードを修正し、再テストする必要がある。
2. **保守性の低下とコードの肥大化**:
   各例外に対応する HTTP ステータスコードのマッピングやログ出力方針が1箇所に集中し、可読性が著しく低下する。

## 4. Decision (決定事項)

例外ハンドリングのアーキテクチャとして、**Chain of Responsibility (責任連鎖) パターン** を採用する。

1. **抽出と抽象化**: 例外処理の責務を `IExceptionHandler` インターフェースに分割し、例外の種類ごとにクラスを設ける（`NotFoundExceptionHandler`, `ValidationExceptionHandler` 等）。
2. **パイプラインの構築**: ミドルウェアは例外を捉えた際、`IExceptionHandlerPipeline` に処理を委譲する。パイプラインは登録されたハンドラ群を順にループし、`CanHandle()` が `true` を返した最初のハンドラに処理 (`HandleAsync`) を実行させる。

```csharp
// Pipeline Core Logic
foreach (var handler in handlers)
{
    if (handler.CanHandle(context))
    {
        await handler.HandleAsync(context, ct);
        context.Handled = true;
        break;
    }
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: .NET 8 標準の `IExceptionHandler` の直接利用**
  - **Approach**: ASP.NET Core 8.0 から導入された標準の `IExceptionHandler` をそのまま利用する。
  - **Rejected Reason**: VK.Blocks 独自の `ExceptionContext` 拡張 (トレースIDやイベントログへの柔軟な対応) や、将来のフレームワークへのロックインを避けるため、独自のインターフェースラップによるパイプラインを構築する道を選択した。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - **高拡張性 (OCP準拠)**: 今後、外部プロバイダ専用の例外ハンドラ等を開発・追加する際、既存コードを一切変更せずに DI の追加のみで対応可能となる。
  - **保守性 (SRP準拠)**: 各ハンドラは自分自身の対象となる例外フォーマットとログ出力方針だけに集中できる。
- **Negative**:
  - ハンドラを登録する順序に依存する。広範な例外 (`BaseException` など) を捕捉するハンドラが先に登録されると、具体的なハンドラに到達しないリスク (Shadowing) がある。
- **Mitigation**:
  - `IExceptionHandlingBuilder` において、詳細度の高いハンドラから順に登録されるようにデフォルトの登録順序をシステム側で統制する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- パイプラインの最後には必ず `DefaultExceptionHandler` を配置し、どのような未知の例外（例：`NullReferenceException` や OOM例外など）が発生しても、確実に 500 Internal Server Error として JSON を返し、スタックトレースがプレーンテキストなどで漏出しないように担保する。
