# ADR 002: Provider-Specific Error Mapping Strategy

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI.SemanticKernel Resilience & Abstraction

## 1. Context (背景)

Semantic Kernel (SK) や、その内部で使用される OpenAI / Azure OpenAI の SDK は、`HttpOperationException` や `RequestFailedException` といったベンダー固有の例外をスローします。これらの例外をそのまま上位のアプリケーションレイヤーにリークさせると、VK.Blocks が掲げる「プロバイダー非依存（Provider-Agnostic）」の原則に反し、プロバイダーを切り替えた際にエラーハンドリングのコードが壊れる原因となります。

## 2. Problem Statement (問題定義)

ベンダー固有の例外を直接扱うことには、以下のリスクがあります。

- **Violation of Abstraction**: アプリケーションが `Azure.RequestFailedException` をキャッチする必要がある場合、そのアプリケーションは特定のベンダーに強く依存してしまいます。
- **Inconsistent Error Handling**: プロバイダーごとに異なる例外型がスローされるため、グローバルな例外フィルターやエラーマッピングの構築が困難になります。
- **Information Leakage**: ベンダー固有の例外メッセージには、内部のインフラ詳細や機密情報が含まれる可能性があり、そのままユーザーに返却するのはセキュリティ上好ましくありません。

## 3. Decision (決定事項)

`AISKErrorMapper` を用いた「中央集権型エラーマッピング戦略」を導入します。

1. **Provider Boundary Protection**:
   - SK ブロック内のすべての実行ロジック（Chat, Embeddings 等）は、プロバイダーとの境界において `try-catch` ブロックで保護します。
2. **Standardized Mapping**:
   - キャッチしたベンダー固有の例外を、VK.Blocks 共通のエラーコード（例：`VKChatErrors.RateLimitExceeded`, `VKChatErrors.InvalidRequest`）にマッピングします。
3. **Encapsulation**:
   - マッピングロジックは内部のスタティッククラス `AISKErrorMapper` に集約し、モジュール全体で一貫したエラー定義を使用します。
4. **Detailed Logging**:
   - `Result.Failure` を返却する直前に、ハイパフォーマンスなソース生成ロガーを使用して、元の例外詳細を内部ログに記録します。これにより、デバッグに必要な情報は保持しつつ、呼び出し元には抽象化されたエラーを返します。

### 核心的なマッピングロジック

```csharp
internal static class AISKErrorMapper
{
    public static VKError Map(Exception ex) => ex switch
    {
        HttpOperationException httpEx when httpEx.StatusCode == HttpStatusCode.TooManyRequests 
            => VKChatErrors.RateLimitExceeded,
        RequestFailedException rfEx when rfEx.Status == 401 
            => VKChatErrors.AuthenticationFailed,
        _ => VKChatErrors.ExecutionError
    };
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Global Exception Middleware
- **Approach**: アプリケーション全体のミドルウェアでベンダー例外をキャッチして変換する。
- **Rejected Reason**: BuildingBlock 自体が自己完結している必要があり、特定のミドルウェアの存在を前提とするのは汎用性を損なうため。

### Option 2: Rethrowing Custom Exceptions
- **Approach**: 独自の `VKAIException` を定義してスローし直す。
- **Rejected Reason**: VK.Blocks は「例外による制御（Control Flow via Exceptions）」を避け、`Result<T>` パターンを推奨しているため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Unified Error Contract**: どのプロバイダーを使用していても、呼び出し側は一貫した `VKError` を受け取ることができる。
- **Secure by Default**: 内部エラーの詳細が意図せずエンドユーザーに露出するのを防ぐ。
- **Improved Observability**: すべてのエラーがマッピングポイントを通過するため、エラーの統計情報やテレメトリの収集が容易になる。

### Negative
- **Loss of Specificity**: ベンダー固有の非常に詳細なエラー理由（例：特定の安全フィルターの警告）が、共通エラーに集約される過程で失われる可能性がある。

### Mitigation
- 元の例外メッセージを `VKError` の `Message` プロパティに（安全な範囲で）含める、またはログに詳細な `Context` を記録することで、トラブルシューティングの利便性を維持する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Auditability**: すべてのエラーマッピングは `Diagnostics` クラスを通じて記録され、監査可能な状態を保ちます。
- **Sanitization**: プロバイダーからのエラーメッセージに含まれる可能性のある PII や機密情報を、マッピングプロセス中にサニタイズします。

## 7. Status
✅ Accepted
