# ADR 001: Provider-Agnostic Engine Abstraction

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI Core Architecture

## 1. Context (背景)

AI業界は現在、OpenAI、Anthropic、Azure OpenAI、Google Gemini、およびローカルLLM（Llama等）など、複数のプロバイダーが乱立する極めて流動的な状況にあります。また、Semantic KernelやLangChainといったオーケストレーターSDKも頻繁にアップデートされており、特定のSDKやプロバイダーのAPIにBuildingBlockのインターフェースが直接依存することは、将来的な技術負債の増大と柔軟性の欠如を招くリスクがあります。

## 2. Problem Statement (問題定義)

特定のプロバイダー（例：Azure OpenAI）やSDK（例：Semantic Kernel）に強く依存した設計（Tight Coupling）には、以下の問題があります。

- **Vendor Lock-in**: 他のプロバイダー（例：AWS Bedrock）への移行が困難になる。
- **Breaking Changes**: 外部SDKのバージョンアップに伴う破壊的変更の影響が、BuildingBlockを利用する全てのアプリケーションに波及する。
- **Testing Complexity**: プロバイダー固有の複雑なSDKをモック化する必要があり、ユニットテストの維持コストが高くなる。
- **Contract Leakage**: プロバイダー固有の例外クラス（`RequestFailedException`等）がアプリケーションレイヤーまでリークし、共通の例外処理を困難にする。

## 3. Decision (決定事項)

AI BuildingBlockのコア設計において「Provider-Agnostic Engine」パターンを採用し、以下の設計指針を強制します。

1. **Vendor-Neutral API**:
   - 外部に公開する API（`VKChatSession`, `IVKChatEngine`）は、特定のベンダーや SDK に依存しない純粋なドメインモデルおよびインターフェースのみを使用する。
2. **Engine Abstraction**:
   - すべての AI 操作は `IVKChatEngine` または `IVKEmbeddingEngine` を通じて実行する。
   - 具体的な実装（OpenAI 用、Azure 用等）は `Internal` 名前空間、または独立したプロバイダー用 BuildingBlock（`VK.Blocks.AI.OpenAI` 等）に分離する。
3. **Result Pattern Flow**:
   - ビジネスロジックの戻り値には常に `VKResult<T>` を使用する。
   - プロバイダー固有の例外は Engine 実装内でキャッチし、ドメイン共通の `VKChatErrors` 等にマッピングして返却する。

### 核心的なインターフェース設計

```csharp
namespace VK.Blocks.AI;

// プロバイダーに依存しない抽象定義
public interface IVKChatEngine
{
    Task<VKResult<VKChatMessage>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default);
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Direct Integration with Semantic Kernel
- **Approach**: Semantic Kernel (SK) を BuildingBlock のコアエンジンとして採用し、その抽象化層をそのまま公開する。
- **Rejected Reason**: SK 自体が非常に巨大な依存関係を持ち、かつ API が頻繁に変更されるため。軽量な BuildingBlock を求めるユーザーにとってオーバーヘッドが大きすぎる。

### Option 2: Provider-Specific Blocks Only
- **Approach**: 共通の AI Block を作らず、`VK.Blocks.AI.AzureOpenAI` のようにプロバイダーごとの Block のみを提供する。
- **Rejected Reason**: プロバイダーを切り替えるたびにアプリケーションコード（DI 登録やインターフェースの差し替え）を大規模に修正する必要があり、ポータビリティが著しく低下する。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **High Portability**: アプリケーションコードを変更せずに `appsettings.json` の変更だけでプロバイダーの切り替えが可能になる。
- **Testability**: 軽量なドメインインターフェースにより、モック作成が容易になりテストカバレッジが向上する。
- **Stability**: 外部 SDK の破壊的変更の影響を Engine 実装内に封じ込めることができる。

### Negative
- **Abstraction Overhead**: プロバイダー固有の高度な機能（例：OpenAI の特定の Beta 機能）を共通インターフェースで表現するのが難しくなる場合がある。

### Mitigation
- プロバイダー固有の拡張機能が必要な場合は、`IVKAIArgs` の `Context` プロパティを通じてメタデータを渡す、または Provider 固有の拡張メソッドを定義することで対応する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Error Mapping**: Engine 内での例外捕捉は `Diagnostics` ロガーを通じて詳細を記録しつつ、呼び出し元には PII（個人情報）を含まない安全なエラーコードのみを返却する。
- **API Key Safety**: プロバイダーの API キーやエンドポイント情報は BuildingBlock の Options パターンを通じて管理し、ログへの漏洩を `[PIIMasked]` 等で防止する。

## 7. Status
✅ Accepted
