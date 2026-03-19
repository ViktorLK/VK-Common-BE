# ADR 003: Standardizing Error Responses with RFC 7807 ProblemDetails

- **Date**: 2026-03-17
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.ExceptionHandling

## 2. Context (背景)

分散システムやマイクロサービスアーキテクチャにおいて、各 Web API が独自のエラーレスポンスフォーマット（例: `{ "error": "Not Found" }` や `{ "message": "Item missing", "code": 404 }` 等）をバラバラに返却すると、API クライアント、フロントエンドアプリケーション、および可観測性（Observability）ツールの連携時に著しい摩擦が生じる。
また、本番環境でエラーが発生した際、クライアント画面で表示されるエラーコードとバックエンドのサーバーログ（TraceId）を紐付ける仕組みが標準化されていなければ、原因究明に多大な時間を要することになる。

## 3. Problem Statement (問題定義)

システム全体で標準化されたエラーフォーマットが存在しないことによる弊害は以下の通りである：

1. **運用負荷と MTTR の増大**:
   ユーザーからのエラー報告時に「どのようなエラーが発生したか」はわかっても、「どのリクエストで落ちたか」というサーバー側の実行コンテキスト（TraceId）がクライアントに提供されていない場合、ログの追跡が困難になる。
2. **クライアント側の実装負荷増大**:
   フロントエンドや外部 API コンシューマが、エンドポイントごとに異なるエラー形式のパースロジックを実装しなければならず、共通のエラーハンドリング基盤を構築できない。
3. **国際化 (i18n) 対応の困難**:
   サーバー側から返されるエラーのヒューマンリーダブルなメッセージ (`detail`) をそのまま画面に表示すると、多言語対応が困難になる。

## 4. Decision (決定事項)

VK.Blocks 内のすべての HTTP エラーレスポンスを標準仕様である **RFC 7807: Problem Details for HTTP APIs** に準拠させることを決定した。
具体的には、標準の `ProblemDetails` クラスを拡張した独自の `VKProblemDetails` 契約を定義し、全ハンドラで強制する。

1. **RFC 7807 構造の標準化**: `status` (HTTPステータスコード), `title` (概要), `detail` (詳細説明), `instance` (リクエストURI) の一貫した提供。
2. **`TraceId` の必須化**: ログ基盤（Serilog / OpenTelemetry）と完全に一致する `traceId` プロパティを JSON のルートに追加する。
3. **`ErrorCode` の必須化**: `code` プロパティ（例: `"ValidationErrors"`, `"UserNotFound"`）を機械可読な文字列として提供し、フロントエンドが翻訳キーとして利用できるようにする。
4. **`Extensions` 辞書の統合**: バリデーションエラーの詳細な辞書や（非本番環境でのみ表示する）スタックトレース等の拡張データは、RFC 7807 仕様に従ってトップレベルプロパティに展開する。

```csharp
// Error Response Contract
public sealed class VKProblemDetails : ProblemDetails
{
    [JsonPropertyName("code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 独自の Minimal JSON フォーマット**
  - **Approach**: 軽量な `{ "success": false, "error": { "code": ..., "message": ... } }` といった従来の OData ライクなレスポンス構造。
  - **Rejected Reason**: RFC 標準に準拠しないため、汎用的なクライアントライブラリや OpenAPI ドキュメントとの統合性が落ち、独自の実装ルールを永遠に管理・周知しなければならないため却下。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - **クライアントの一貫性**: Web API コンシューマは、RFC 7807 をパースできる単一のグローバルエラーハンドリング機構を実装するだけで済む。
  - **可観測性の劇的な向上**: エラー画面に `traceId` などの情報を表示させることで、障害発生から原因特定までの時間（MTTR）が劇的に短縮される。
  - **国際化 (i18n) サポート**: `detail` ではなく不変の `code` に依存して UI 側が翻訳メッセージをマッピングできるようになる。
- **Negative**:
  - 無駄を極限まで削ぎ落とした独自フォーマットに比べ、JSON ペイロードのサイズがわずかに増加する。
  - 既存のレガシーな API コンシューマがある場合、新しい `ProblemDetails` スキーマに合わせてパースロジックを移行してもらう必要がある。
- **Mitigation**:
  - ASP.NET Core のミドルウェアおよび `IProblemDetailsFactory` の実装により、開発者は意識せずに RFC 7807 の形式を生成できるような DX（開発者体験）を提供する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- エラー生成時には本番環境で機密情報（内部のスタックトレースや生 SQL エラーメッセージ）を `detail` や `Extensions` に出力しないよう、`ExceptionHandlingOptions.ExposeStackTrace` のフラグ評価に基づく一元的なマスキングポリシーを適用すること。
