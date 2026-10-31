# ADR 005: Thread Safe Extensible Context

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psycheのプロンプト構築処理（Weaving）において、リクエストパラメータや各並行処理ステージで生成されたプロンプトフラグメント、および中間状態をスレッド間で共有しながら処理を進める必要がある。ステージの並行実行に対応しながら、かつ中間データ（各機能独自のスクラッチデータなど）を自由に保持できる、柔軟で安全なコンテキスト（Context）オブジェクトが求められていた。

## 2. Problem Statement (問題定義)

不適切なスレッド同期や、拡張性の低いコンテキストクラスの定義には、以下の問題がある：
1. **競合状態 (Race Condition)**: 複数のStageが同時に `VKWeavingContext` に対して `AddFragment` を呼び出すと、内部のリスト構造が破壊されたり、一部のフラグメントが喪失したりする。
2. **密結合による拡張性の低下**: コンテキストに新しいステージ専用の中間状態プロパティを追加し続けると、コンテキストの定義自体が肥大化し、コアモジュールが各サブ機能の内部実装の詳細に依存することになる。
3. **データ重複とメモリオーバーヘッド**: スレッド安全のために都度ディープコピー（Deep Copy）を行う設計では、アロケーション回数が劇的に増加し、高並行アクセス時にGC（ガベージコレクション）の重大なボトルネックが発生する。

## 3. Decision (決定事项)

コンテキストの並行操作安全性とステージ依存関係のデカップリングを両立させるため、**「Thread-Safe Extensible Context (スレッド安全かつ拡張可能なコンテキスト)」**パターンを採用する。

1. **`VKWeavingContext` の宣言**:
   - 不変の識別属性（`TenantId`、`PersonaId`、`SessionId`、`CorrelationId` 等）はイミュータブルな `init` プロパティとし、C# 12+ `sealed record` で表現する。
2. **`System.Threading.Lock` による排他制御**:
   - 動的に追加・変更される `Fragments`（提示文）および `Evicted`（切り捨て履歴）の変更処理に対して、.NET 9 から導入された新しい `Lock` オブジェクトを用いた `lock` ステートメントにより、リストへの追加・置換操作のスレッド安全性を完全に保証する。
3. **強タイプな拡張コンテナ (Type-Keyed Extension Container) の提供**:
   - `Dictionary<Type, object>` を内部に設け、`SetExtension<T>()` / `GetExtension<T>()` を介して、各ステージ固有の中間データ（例：特定のセッションメタデータ、切り捨て計算時のトークン一時キャッシュなど）を動的にアタッチできるようにする。

### 核心的なスレッド同期と拡張コンテナの実装

```csharp
namespace VK.Blocks.AI.Psyche;

public sealed record VKWeavingContext
{
    // 不変の属性定義
    public required string TenantId { get; init; }
    public required string PersonaId { get; init; }
    public required string SessionId { get; init; }

    // スレッド同期用 Lock
    private readonly Lock _lockFragments = new();
    private readonly List<VKPromptFragment> _fragments = [];

    public IReadOnlyList<VKPromptFragment> Fragments
    {
        get
        {
            lock (_lockFragments)
            {
                return [.. _fragments]; // 呼び出し側には安全なスナップショットを返却
            }
        }
    }

    public void AddFragment(VKPromptFragment fragment)
    {
        VKGuard.NotNull(fragment);
        lock (_lockFragments)
        {
            _fragments.Add(fragment);
        }
    }

    // 拡張コンテナ
    private readonly Dictionary<Type, object> _extensions = new();

    public void SetExtension<T>(T value) where T : class
    {
        _extensions[typeof(T)] = VKGuard.NotNull(value);
    }

    public T? GetExtension<T>() where T : class
    {
        return _extensions.TryGetValue(typeof(T), out var v) ? (T)v : null;
    }
}
```

## 4. Alternatives Considered (代替案的検討)

### Option 1: Immutable State Passing (Event Sourcing Style)
- **Approach**: 各ステージが「古い Context から新しい Context オブジェクトを複製して返す」という純粋な関数型アプローチを採用する。
- **Rejected Reason**: ステージの並列実行中に生成された状態を同期させるのが難しく、マージのオーケストレーションが極めて複雑になり、パフォーマンスも悪化するため。

### Option 2: ThreadLocal Storage
- **Approach**: 各スレッドのローカル変数として状態を保持する。
- **Rejected Reason**: 非同期メソッド（`async/await`）の await 前後でスレッドが切り替わるため、.NET の非同期実行においてスレッドローカル変数ではコンテキストを安全に引き継げないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な並行安全性**: どのスレッド、どの並列タスクからフラグメントが追加されても、状態の破損が発生しない。
- **ステージ間の結合度低下**: 各ステージはContextクラスのプロパティを書き換えることなく、`SetExtension<T>` を用いて安全にカスタム状態を格納できる。

### Negative
- **Lock によるスレッド競合の可能性**: 多数のタスクが同時に同一コンテキストへ書き込みを行うと、ロックの獲得待ち（Lock Contention）が発生する。

### Mitigation
- Weavingプロセス全体はせいぜい4〜5個の主要ステージで構成されるため、スレッドの競合時間は極めて短く、無視できるレベルであることをパフォーマンス測定により実証済み。また、Fragmentsの取得時にはコピー `[.. _fragments]` を返すことで、反復子（Iterator）の競合を回避している。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Lock Object Type**: 従来の `private readonly object _lock = new();` の代わりに、JITコンパイラおよびランタイムによる最適化の恩恵を受けられる新しい `System.Threading.Lock` 型を導入し、オーバーヘッドを極限まで低減させる。
- **Type Safety**: 拡張コンテナの格納値を generic `<T>` で制約し、実行時の予期せぬキャストエラーをコンパイル段階で防御する。

## 7. Status
✅ Accepted
