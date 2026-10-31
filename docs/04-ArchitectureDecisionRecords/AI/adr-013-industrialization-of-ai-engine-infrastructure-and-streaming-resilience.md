# ADR 013: Industrialization of AI Engine Infrastructure and Streaming Resilience

**Date**: 2026-05-11  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: VK.Blocks.AI Industrialization

## 1. Context (背景)

DeepSeek R1 や GPT-o1 シリーズに代表される推理型（Reasoning）モデルの台頭により、AI 応答における「思考プロセス（Reasoning Content/CoT）」の重要性が増している。また、**PWP (PersonaWeavePulsar)** のような高頻度パルス（Pulsar）駆動型システムにおいて、AI のストリーミング応答はシステム全体の滑らかさを左右するクリティカルなパスとなっている。

従来の Pull 型 `IAsyncEnumerable` 実装では、ネットワークの微小な遅延（Jitter）が直接消費側のスレッドをブロックし、PWP の実行リズムを乱すリスクがあった。さらに、AI プロバイダーごとのエラーコードの不一致が、上位レイヤーでのレジリエンス設計（Retry/Circuit Breaker）を困難にしていた。

## 2. Problem Statement (問題定義)

1. **データ構造の欠如**: 思考プロセス（Reasoning Content）を格納する標準的なフィールドがなく、プロバイダーごとのメタデータに埋もれていた。
2. **ストリーミングのブロッキングリスク**: `await foreach` による逐次消費モデルでは、プロバイダー側のパケット到着待ちがそのまま消費側の待機時間となり、PWP のパルス精度を低下させる。
3. **エラーハンドリングの非一貫性**: 429 (Rate Limit) や 401 (Auth Error) がプロバイダー固有の例外としてスローされ、VK 共通の `Result` パターンとの親和性が低かった。
4. **オブザーバビリティの断絶**: TokenUsage などの統計データが `Result` オブジェクトから直接取得できず、デバッグやコスト管理に支障をきたしていた。

## 3. Decision (決定事項)

AI モジュールの「工業 DNA」を強化するため、以下の設計変更を採用する。

### 3.1 推理データの第一級市民化 (Reasoning as First-class Citizen)
`VKChatMessage` に `ReasoningContent` プロパティを追加し、内部的に `VKReasoningPart` を用いて多模态メッセージの一部として管理する。

```csharp
public sealed class VKChatMessage
{
    public string? ReasoningContent { get; set; } // CoT (Chain of Thought)
    public IList<IVKChatMessagePart> Parts { get; } = new List<IVKChatMessagePart>();
}
```

### 3.2 Channel ベースの高性能ストリーミング (Decoupled Streaming)
`SendStreamingAsync` 内部で `System.Threading.Channels` を導入し、**「生産者（Provider）- 消費者（PWP）」** を完全に解耦する。

```csharp
var channel = Channel.CreateUnbounded<VKChatStreamingResponse>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = true,
    AllowSynchronousContinuations = true
});

// バックグラウンドタスクでプロバイダーから Channel へ Push
_ = Task.Run(async () => { ... channel.Writer.WriteAsync(chunk); ... });

// 呼び出し側は Channel から Pull
while (await channel.Reader.WaitToReadAsync(ct)) { ... }
```

### 3.3 統一エラーマッパー (Unified Error Mapping)
`VKAIErrorMapper` を導入し、プロバイダー固有の例外を `VKAIErrors.QuotaExceeded` や `AuthenticationFailed` 等の VK 標準エラーに変換する。

### 3.4 メタデータのリアルタイム透過 (Tokenics Visibility)
`Core` の `VKResult` への影響を最小限に抑えつつ、`VKChatMessage.Metadata` や `VKChatStreamingResponse.Metadata` に `TokenUsage` を自動注入し、呼び出し側が即座にコストを確認できるようにする。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: Core.VKResult への Metadata プロパティ追加**
    - **Rejected Reason**: Core 契約の変更は影響範囲が広すぎる。AI モジュール固有のメタデータは、メッセージオブジェクトの Metadata 辞書を利用することで、Core を汚染せずに目的を達成できる。
- **Option 2: 従来の Pull 型 IAsyncEnumerable の継続**
    - **Rejected Reason**: PWP のようなリアルタイム性の高いシステムでは、ネットワーク Jitter による微小な停止が致命的となるため、バッファリング（Channel）による平滑化が不可欠。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **PWP の安定性向上**: AI の応答が Channel にバッファリングされるため、ネットワークの揺らぎがシステムのメインループに影響を与えなくなる。
- **次世代モデル対応**: DeepSeek R1 等の推理プロセスを標準的な API で扱えるようになる。
- **弾力的なエラー処理**: 標準化されたエラーコードにより、上位レイヤーでのポリシー適用が容易になる。

### Negative
- **メモリ消費**: `UnboundedChannel` を使用するため、極端に長い応答や消費が遅い場合にメモリ使用量が増加する可能性がある。
- **複雑性**: 内部的なバックグラウンドタスクと Channel の管理が必要。

### Mitigation
- **緩和策**: タイムアウト（`CancellationToken`）を厳格に適用し、タスクのリークとメモリの無限増殖を防止する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装**: `AISKChatEngine.cs` において、`ExecuteStreamingAsync` テンプレートを Channel 対応版に更新。
- **セキュリティ**: プロバイダーから返される `Usage` メタデータは動的にパース（`dynamic`）するが、型安全な `VKTokenUsage` に即座に変換し、異常なデータによるランタイムエラーを防止する。
- **ガバナンス**: `VKAIErrorMapper` を `public` に公開（Internal 名前空間）し、他の AI インフラ（VectorStore, Text 等）でも一貫したエラー表現を強制する。

---
**Last Updated**: 2026-05-11
