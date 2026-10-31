# ADR 001: Expanding Global ErrorType for Enterprise-Grade Error Handling

- **Date**: 2026-03-26
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core / Error System Refactoring

## 2. Context (背景)

現在、システムの `ErrorType` は基礎的な `Failure`, `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden` のみをカバーしている。これらの基本タイプは CRUD クラスのアプリケーション要件を満たすには十分であるが、分散システムやサードパーティの統合、および本番環境の可観測性（Observability）を処理する際には、セマンティックの粒度が著しく不足している。

具体的なシナリオ：
1. **レート制限 (Rate Limiting)**: API キーの制限を超えた場合、通常は HTTP 429 を返却する必要がある。
2. **インフラ利用不可 (Service Unavailable)**: Redis やデータベースが一時的に過負荷状態になった場合、フロントエンドのバックオフメカニズムをトリガーするために HTTP 503 を返却する必要がある。
3. **外部サービス障害 (External Error)**: Google OAuth などの外部サービス呼び出しが失敗した際、内部のバグ (500) なのか、外部の依存関係 (502/504) の問題なのかを区別する必要がある。

## 3. Problem Statement (問題定義)

1. **曖昧な HTTP マッピング**: 現在、レート制限などのエラーは `Failure (500)` または `Validation (400)` にしかマッピングできず、これは RESTful のベストプラクティスに反し、クライアント側での正確なエラーハンドリングも困難にしている。
2. **可観測性の欠如**: 監視ダッシュボードにおいて、サードパーティサービスに起因するエラーの発生割合を `ErrorType` から直接集計・把握することができない。
3. **開発者体験 (DX)**: 開発者がビジネスエラーを定義する際、ステータスコードを手動でハードコーディングするか、エラーコードの文字列に暗黙的にステータスを含める必要があり、メンテナンスコストが増加している。

## 4. Decision (決定事項)

`VK.Blocks.Core` の `ErrorType` 列挙型を拡張し、以下のタイプを新規追加する：

1. **`TooManyRequests` (HTTP 429)**: API キーのレート制限ロジック専用。
2. **`ServiceUnavailable` (HTTP 503)**: インフラレベルまたはシステムの一時的な過負荷用。
3. **`Timeout` (HTTP 504/408)**: バックエンド依存処理のタイムアウト専用。
4. **`ExternalError` (HTTP 502/504)**: 外部統合（OAuth、Blob Storage API など）から返された異常用。

同時に以下を行う：
- `VK.Blocks.ExceptionHandling` の `ProblemDetailsFactory` を更新し、新しいステータスコードに対応する標準タイトルをサポートする。
- `VK.Blocks.Web` の `ErrorTypeExtensions.ToStatusCode()` メソッドを更新し、1:1 のマッピングロジックを実装する。

## 5. Alternatives Considered (代替案の検討)

### Option 1: Error レコードに直接 StatusCode を定義する
- **Approach**: `Error` に `int StatusCode` プロパティを追加する。
- **Rejected Reason**: これにより、インフラストラクチャ層（HTTP）の概念が Core/Domain 層に漏洩することになる。`ErrorType` はセマンティック（意味論的）なものであるべきであり、それが HTTP または gRPC のどちらのステータスコードにマッピングされるかは、表現層が決めるべきである。

### Option 2: ErrorType は変更せず、エラーの Code で解析する
- **Approach**: プレフィックスで識別する（例：`ApiKey.TooManyRequests` -> 429）。
- **Rejected Reason**: ロジックが散乱し、`VKApiController` での自動マッピングが難しくなる上、リフレクションや文字列マッチングによるパフォーマンス上のオーバーヘッドが増加するため。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: システムの Production-Ready（本番稼働対応）レベルが大幅に向上する。API エラーのセマンティクスがグローバルな工業標準レベルに到達する。
- **Negative**: 列挙型定義の複雑性がわずかに増加する。
- **Mitigation**: `Error.cs` に詳細な XML コメントを追加し、`VK.Blocks.Web` で統一された変換拡張を提供することで、低レイヤーの詳細をカプセル化する。

## 7. Implementation & Security (実装詳細とセキュリティ考慮)

### Implementation Note
```csharp
public enum ErrorType
{
    // ... 既存の項目
    TooManyRequests = 6,     // 429
    ServiceUnavailable = 7,  // 503
    Timeout = 8,             // 504/408
    ExternalError = 9        // 502/504
}
```

### Security Observation
詳細化されたエラータイプは、セキュリティチームが監視システムにおいて、ブルートフォース攻撃（429 統計に反映）や外部攻撃に起因するシステムの異常を特定するのに役立つ。

---
**Last Updated**: 2026-03-26  
**Status**: ✅ Accepted
