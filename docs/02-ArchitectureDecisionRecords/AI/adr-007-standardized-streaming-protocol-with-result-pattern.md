# ADR 007: Standardized Streaming Protocol with Result Pattern

- **Date**: 2026-05-10
- **Status**: 📝 Draft
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI / Chat

## 1. Context (背景)

LLM の応答生成には時間がかかるため、ユーザー体験（UX）向上のために生成されたテキストを逐次送信するストリーミング（Server-Sent Events: SSE）が一般的です。しかし、.NET の `IAsyncEnumerable<T>` を用いた非同期ストリームにおいて、エラーハンドリング（`VKResult` パターンの適用）をどのように一貫させるかが課題となります。

## 2. Problem Statement (問題定義)

1. **エラー通知の不整合**: ストリームの途中でプロバイダー側がエラー（タイムアウトや割込み）を返した場合、例外をスローするとストリームが唐突に終了し、コンシューマー側でのクリーンな処理が難しくなる。
2. **メタデータの欠落**: ストリームの最初または最後で送られるべきメタデータ（使用したモデル名、トークン使用量、停止理由など）を、どのチャンクに含めるかのルールがない。
3. **集約ロジックの重複**: ストリーミングをサポートしつつ、バッチ処理（すべての生成が終わってから一括で受け取る）も行いたい場合に、集約ロジックを個別に書く必要がある。

## 3. Decision (決定事項)

1. **ストリーミングメソッドのシグネチャ標準化**:
   すべてのストリーミング API は `IAsyncEnumerable<VKResult<VKChatStreamingResponse>>` を返すものとします。

2. **ストリーム内でのエラー伝播**:
   ストリーム中にエラーが発生した場合、例外をスローするのではなく、`VKResult.Failure` オブジェクトを `yield return` し、その直後にストリームを正常終了（`break`）させます。これにより、コンシューマーは `foreach` ループ内で安全にエラーを判定できます。

3. **メタデータの配置ルール**:
   - **最初のチャンク**: モデル ID やロール情報を可能な限り含めます。
   - **最後のチャンク**: `FinishReason` や最終的な `VKTokenUsage` を含めます。

4. **`.ToFullResultAsync()` 拡張メソッドの提供**:
   `IAsyncEnumerable<VKResult<VKChatStreamingResponse>>` を `Task<VKResult<VKChatMessage>>` に変換する標準的なアグリゲーターを提供し、非ストリーミングコンシューマーも透過的に扱えるようにします。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 例外をスローする**
  - **Rejected Reason**: `Result<T>` パターンの一貫性が損なわれ、`try-catch` と `IsSuccess` 判定が混在することになる。
- **Option 2: チャンネル (`System.Threading.Channels`) を使用する**
  - **Rejected Reason**: `IAsyncEnumerable` の方が C# 8.0 以降の `await foreach` との相性が良く、直感的である。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - コンシューマー側で `await foreach` を使いながら、常に `IsSuccess` をチェックする一貫したコードが書ける。
  - ストリーミング中の中断も「データの一部」として適切に処理できる。
- **Negative**:
  - メッセージの集約（テキストの連結）をどこで行うか（SDK 内かコンシューマー側か）の責任境界が曖昧になりやすい。
- **Mitigation**:
  - 標準の `VKChatSession` が集約を自動的に行い、履歴（History）に保存する責任を持つ。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: `VKChatStreamingResponse` に `TextDelta` プロパティを持たせ、累積テキストではなく「差分」のみを送ることで帯域を節約します。
- **セキュリティ考察**: ストリーミング中も Content Filtering（検閲）を継続的に行う必要があり、不適切なコンテンツが検知された瞬間にストリームを遮断し、エラーを返さなければなりません。

**Last Updated**: 2026-05-10
