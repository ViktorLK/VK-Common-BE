# ADR 002: Bulk Vector Operations Abstraction

- **Date**: 2026-06-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/VectorStore

## 1. Context (背景)

ベクトルデータベース（Qdrant や SQLite Vec 等）に対して大量のドキュメント（ドキュメントを数百〜数千のチャンクに分割したデータ）をインジェストする際、1件ずつ順次登録（挿入または更新）を行うと、以下の課題が発生する：
1. **通信オーバーヘッドの爆発**: 各レコードごとに個別のデータベース接続、ネットワークラウンドトリップ、またはディスク I/O トランザクションが発生するため、システムスループットが極めて悪化する。
2. **パフォーマンス劣化**: インメモリソートやツリー構築処理（HNSW インデックスの更新など）が挿入ごとに都度再計算され、CPU とメモリへ多大な負荷がかかる。

しかし、従来の `IVKVectorCollection` 抽象には、複数レコードを一括処理するバッチ登録用インターフェースが定義されていなかった。

## 2. Problem Statement (問題定義)

大規模なナレッジインジェスト処理等において、データベース接続とインデックス再構築の負荷を極小化し、大量のベクトルデータを効率的かつアトミックに一括登録・更新できる標準的なバッチ処理の仕組みが必要であった。

## 3. Decision (決定事項)

VK.Blocks.VectorStore において、**「一括ベクトル処理用インターフェース（`IVKBulkCapableVectorStore`）の定義およびバルク登録の標準実装」**を決定する。

### 1. `IVKBulkCapableVectorStore` 抽象の導入
- 一括挿入・更新を汎用的に表現する `IVKBulkCapableVectorStore` インフェースを定義し、バルク操作をサポートするプロバイダがこれを実装する。
- 同時に、`IVKVectorCollection` 側にもコレクション単位でのバッチ登録操作（`UpsertBatchAsync`）を定義する。

### 2. プロバイダでのバルク実装
- **`InMemoryVectorCollection`**: インメモリでのバルク登録に対応し、ロック範囲を最小限に抑えてバッチでリスト追加を行う。
- **`SqliteVectorCollection`**: 内部の SQLite 仮想テーブルへの `INSERT OR REPLACE` 処理を一括して単一の SQLite トランザクション内で実行するように実装し、ファイル I/O を劇的に圧縮する。

```csharp
public interface IVKBulkCapableVectorStore : IVKVectorStore
{
    // 一括でコレクションに対してベクトルをバルク挿入・更新する
    Task<VKResult> BulkUpsertAsync(
        string collectionName, 
        IReadOnlyList<VKVectorRecord> records, 
        CancellationToken ct = default);
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: パイプライン側で Task.WhenAll を用いて並列に 1 件挿入を呼び出す
- **Approach**: アプリケーション側で並行して `UpsertAsync` を同時に多数呼び出し、疑似バルクとする。
- **Rejected Reason**: データベースのコネクションプールが枯渇し、I/O 競合により SQLite が `database is locked` エラーを引き起こすなど、安定性が損なわれるため却下した。真の一括 SQL トランザクション実行が必要である。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **スループットの圧倒的向上**: SQLite への一括挿入等では、単一トランザクション化することで数千件のインジェスト処理が数十秒から数百ミリ秒へと劇的に高速化する。
- **統一された一括処理スキーマ**: Downstream サービス（Qdrant 等のプロバイダ）が備えるネイティブなバルク API（Batch Upsert）をインターフェースを歪めずにそのまま活用できる。

### Negative
- **メモリフットプリントの一時的増加**: バルクでまとめてデータをメモリに載せるため、一回に転送するリストサイズが大きくなりすぎると、ヒープメモリが急激に消費されるリスクがある。

### Mitigation
- 開発者に対して「バルクリストは一度に最大 100〜500 件程度でチャンク（分割）して呼び出すこと」をガイドライン化し、ライブラリ境界でも必要に応じてサイズ検証（ガードチェック）を入れる。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 登録レコードが空（Empty）の場合のガードチェックを徹底し、不要な接続のオープンを防ぐ。

## 7. Status
✅ Accepted
