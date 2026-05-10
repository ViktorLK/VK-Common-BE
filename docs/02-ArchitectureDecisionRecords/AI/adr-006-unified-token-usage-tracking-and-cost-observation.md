# ADR 006: Unified Token Usage Tracking and Cost Observation

- **Date**: 2026-05-10
- **Status**: 📝 Draft
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI / Tokenics

## 1. Context (背景)

LLM（Large Language Models）の利用において、トークン消費量はコスト管理、クォータ制限、およびパフォーマンス監視の核心となる指標です。現在、OpenAI、Azure OpenAI、Google Gemini、Anthropic などの各プロバイダーは、独自のトークンカウント方式、コスト構造、およびレスポンス形式を持っています。

エンタープライズアプリケーションにおいて、複数の機能（Chat, Embeddings, Audio 等）や複数のプロバイダーを横断して一貫した方法でトークン使用量を追跡・監査・制限できる仕組みが不可欠です。

## 2. Problem Statement (問題定義)

1. **プロバイダー間の非互換性**: 各プロバイダーでレスポンスに含まれるメタデータの構造が異なり、共通のトラッキングロジックが書けない。
2. **コストの不透明性**: リアルタイムで使用量を確認する手段がないため、予期せぬコスト超過のリスクがある。
3. **集計の困難さ**: リクエスト単位の使用量は分かっても、テナント単位やユーザー単位での累積使用量を把握する標準的な仕組みがない。
4. **ハードリミットの欠如**: 特定のトークン予算を超えた際に、自動的にサーキットブレーカーを働かせる仕組みが欠如している。

## 3. Decision (決定事項)

1. **標準化された `VKTokenUsage` レコードの定義**:
   `AI.Common`（または基盤層）に、プロバイダーに依存しない統一的なトークン使用量モデルを定義します。

   ```csharp
   public sealed record VKTokenUsage
   {
       public int PromptTokens { get; init; }
       public int CompletionTokens { get; init; }
       public int TotalTokens => PromptTokens + CompletionTokens;
       public string? ModelId { get; init; }
       public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
   }
   ```

2. **レスポンスへの強制適用**:
   すべての `IVKChatEngine` や `IVKEmbeddingEngine` の戻り値（`Result<T>`）には、この `VKTokenUsage` が含まれることを必須とします（または、レスポンスオブジェクトの共通インターフェースに含める）。

3. **`VKTokenics` フィーチャーの導入**:
   トークン集計と予算管理を専門に扱う `Tokenics` モジュールを実装し、以下のインターフェースを提供します。
   - `IVKTokenUsageAggregator`: リクエストごとの使用量を収集・保存する。
   - `IVKTokenBudgetManager`: 予算超過を検知し、リクエストをブロックする。

4. **サーキットブレーカーの統合**:
   トークン予算が設定されている場合、`VKAIOptions` の閾値を超えた時点で `Result.Failure(AIErrors.QuotaExceeded)` を返す仕組みを構築します。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: プロバイダー固有のメタデータをそのまま利用する**
  - **Rejected Reason**: コンシューマー側がすべてのプロバイダーの形式を解釈する必要があり、疎結合の原則（ADR-001）に違反する。
- **Option 2: インフラ層のみでログを出力する**
  - **Rejected Reason**: アプリケーション層でリアルタイムな予算制御（ハードリミット）を行うことができない。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - プロバイダーを問わず、一貫したダッシュボードやレポートの作成が可能になる。
  - トークン予算に基づくコスト制御が容易になる。
- **Negative**:
  - すべてのプロバイダーの実装において、独自のメタデータを `VKTokenUsage` にマッピングする手間が発生する。
- **Mitigation**:
  - 主要なプロバイダー向けのマッピングユーティリティを `AI.Infrastructure` で提供し、実装コストを削減する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: `VKResult<T>` のメタデータプロパティとして `VKTokenUsage` を保持するか、`IVKAIResponse` インターフェースを定義して一貫性を持たせます。
- **セキュリティ考察**: トークン使用量データは、機密性の高いプロンプト内容とは分離して管理されるべきですが、テナント ID（TenantId）との紐付けを確実に行い、コストデータの漏洩（Side-channel attack）を防ぐ必要があります。

**Last Updated**: 2026-05-10
