# ADR 003: Zero Infrastructure InMemory Defaults

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psycheは、ペルソナ情報、プロンプト指示、会話履歴、および知識ベースのエントリといったステートフルなデータを読み書きする。これらのデータストア（Store）インターフェースを実装する際、開発者による検証環境の構築を迅速にし、CI/CD環境における自動テストを安定して実行するためには、高度な外部分散キャッシュ（Redis等）やリレーショナルデータベースの設定を最初から要求しない仕組みが必要である。

## 2. Problem Statement (問題定義)

最初から外部インフラへの接続を義務付ける設計には、以下の問題がある：
1. **開発開始時のハードル**: 開発者がローカル環境でPsycheを起動する際、DBやRedisコンテナが起動していないとエラーになり、動作確認の敷居が高くなる。
2. **インフラ障害への脆弱性**: インフラのネットワーク遅延や一時的なダウンタイムがそのままローカルデバッグ作業やCI実行を阻害する。
3. **テストの副作用**: 共有インフラをテストで使用すると、データ競合や不整合が発生しやすく、テストの並行実行が制限される。

## 3. Decision (決定事項)

最小の初期オーバーヘッドでモジュールを実行可能にするため、**「Zero-Infrastructure Startup with InMemory Defaults (ゼロ外部インフラ起動)」**ポリシーを採用する。

1. **インメモリデフォルト実装の提供**:
   - `IVKPersonaStore`、`IVKEchoStore`、`IVKKnowledgeStore`、`IVKDirectiveStore` などのストアインターフェースに対し、それぞれスレッドセーフな `ConcurrentDictionary` をベースとした `InMemoryXxxStore` クラスを標準で作成する。
2. **DIでのフォールバック登録**:
   - 各特性的登録（Feature）において、開発者がカスタムの実装（SQL DatabaseやRedisベースの実装）をDI登録していない場合、自動的にインメモリ実装が注入されるように `TryAddSingleton` または `TryAddScoped` を使用してDI登録を行う。

### 核心的なDI登録とストア実装例

```csharp
namespace VK.Blocks.AI.Psyche;

internal sealed class InMemoryEchoStore : IVKEchoStore
{
    private readonly ConcurrentDictionary<string, List<VKPromptFragment>> _db = new();

    public Task<VKResult<IReadOnlyList<VKPromptFragment>>> GetHistoryAsync(
        string tenantId,
        string sessionId,
        CancellationToken ct = default)
    {
        var key = $"{tenantId}:{sessionId}";
        var list = _db.GetOrAdd(key, _ => new List<VKPromptFragment>());
        lock (list)
        {
            return Task.FromResult(VKResult.Success<IReadOnlyList<VKPromptFragment>>([.. list]));
        }
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Mock/Stub Implementations in Test Project Only
- **Approach**: インメモリ実装をテストプロジェクト内にのみ配置する。
- **Rejected Reason**: アプリケーションの開発初期段階やステージングでの「とりあえず動かす」モック実行が難しくなり、インフラのセットアップを強制されるため。

### Option 2: Require Local SQLite Database File
- **Approach**: デフォルトストアとして、ファイル書き込み型のSQLiteを利用する。
- **Rejected Reason**: コンフィギュレーションおよび書き込みディレクトリのパーミッション問題など、物理ファイルI/Oに伴うエラーハンドリングが必要になり、完璧な「ゼロコンフィグ」を実現できないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **迅速な開発**: 下游アプリケーション開発者は、一行のDIコードを書くだけでインメモリストアによりプロンプト編排の動作検証が行える。
- **テストの決定論**: 外部影響を受けない高速な単体・結合テストを実行できる。

### Negative
- **データの非永続化**: デフォルト状態のインメモリストアはアプリケーションの再起動に伴いデータがクリアされるため、そのまま本番環境で誤用されるとデータロストに繋がる。

### Mitigation
- ログとドキュメントにおいて「InMemoryStoreは開発・テスト専用であり、生産環境では永続化ストア（例：Redisやデータベース）の登録が必須である」旨を明記し、DI警告をログ出力するなどの策を講じる。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Thread-Safety**: インメモリストア実装はマルチスレッド同時実行環境で壊れないよう、`ConcurrentDictionary` や明示的な `lock` ブロックによる同期を徹底する。
- **Memory Consumption**: テストの過程でメモリ消費が無限に増大しないよう、インメモリ実装にはシンプルな上限設定（エントリ数の上限や期限切れエビクション）が考慮される余地を残す。

## 7. Status
✅ Accepted
