# ADR 004: Standardized Execution Arguments (Args Pattern)

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI Execution Logic

## 1. Context (背景)

AI の実行操作（Chat や Embeddings）において、ユーザーはリクエストごとに Temperature、MaxTokens、ModelName といった挙動制御パラメータを微調整（Override）したいという強いニーズがあります。これらのパラメータをグローバルな Options だけで管理すると、同時並行で実行される異なるタスクに対して柔軟に対応できず、またメソッドの引数として個別に渡すと引数が肥大化し、将来的な拡張性が損なわれます。

## 2. Problem Statement (問題定義)

実行パラメータの管理における課題は以下の通りです。

- **Rigidity**: グローバル設定に依存しすぎると、特定の呼び出しのみパラメータを変えることが困難になる。
- **Brittle API**: パラメータを個別の引数としてメソッドに追加していくと、破壊的変更（Breaking Changes）が発生しやすく、保守性が低下する。
- **State Contamination**: 実行ごとに共有の Options オブジェクトを書き換えると、マルチスレッド環境において予期せぬ副作用が発生する。

## 3. Decision (決定事項)

VK.Blocks の標準設計指針である「Args Pattern」(AP.05) を AI モジュールのすべての実行メソッドに適用します。

1. **Args Records**:
   - 実行単位のパラメータを保持する `sealed record` を定義する（例：`VKChatArgs`, `VKEmbeddingArgs`）。
   - これらのレコードのプロパティはすべて Nullable とし、明示的に指定された場合のみオーバーライドとして扱う。
2. **Method Signature**:
   - 実行メソッドは、Args オブジェクトを末尾のオプション引数として受け取る。
3. **Merge Logic (Coalesce Pattern)**:
   - Engine 実装内部において、`??`（Null 合体演算子）を使用して、渡された Args とグローバル Options をマージする。
4. **Immutability**:
   - Args レコードは不変（Immutable）とし、一度作成された後に状態が変化しないことを保証する。

### 核心的なコードパターン

```csharp
// 実行用引数の定義
public sealed record VKChatArgs : IVKAIArgs
{
    public float? Temperature { get; init; }
    public int? MaxTokens { get; init; }
}

// Engine 内部でのマージ処理
public async Task<VKResult<VKChatMessage>> SendAsync(..., VKChatArgs? args = null)
{
    var temp = args?.Temperature ?? _options.DefaultTemperature;
    var tokens = args?.MaxTokens ?? _options.DefaultMaxTokens;
    // ... LLM へのリクエスト実行
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Telescoping Method Parameters
- **Approach**: `ChatAsync(string prompt, float? temperature = null, int? maxTokens = null, ...)` のように引数を並べる。
- **Rejected Reason**: パラメータが増えるたびにメソッドのシグネチャが変わり、利用側のコードが壊れるため。

### Option 2: Thread-Local Options Overrides
- **Approach**: スレッドローカルなストレージを使用して一時的に Options を上書きする。
- **Rejected Reason**: 非同期プログラミング（async/await）においてコンテキストの追跡が複雑になり、バグの温床となるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **API Stability**: パラメータが追加されても Args レコードのプロパティを増やすだけで済み、メソッドのシグネチャを維持できる。
- **Granular Control**: 呼び出しごとに完全に独立した挙動制御が可能になる。
- **Thread Safety**: 不変なレコードを使用するため、副作用のない安全な並行実行が可能。

### Negative
- **Allocation Overhead**: リクエストごとに Args オブジェクトを生成するため、微小ながらメモリ割り当てが発生する。

### Mitigation
- 頻繁に使用されるデフォルト設定については `VKChatArgs.Empty` のような静的インスタンスを提供し、不要なアロケーションを抑制する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Input Guard**: Args で渡された `Temperature` や `TopP` が有効な範囲（例：0.0 〜 2.0）に収まっているかを Engine 内部で `VKGuard` を用いて検証する。
- **Telemetry**: オーバーライドされたパラメータは `Activity` のタグとして記録し、どのアクティビティがどのような設定で実行されたかを追跡可能にする。

## 7. Status
✅ Accepted
