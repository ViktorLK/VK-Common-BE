# 💥 Failure Modes & Resilience Handbook: VK.Blocks.Persistence

- **System ID / Package ID**: `VK.Blocks.Persistence`
- **Domain / Area**: `Persistence`
- **Target Audience**: SRE、クラウド運用者、下流サービス開発者、AI ペアプログラミング
- **Governing Rules**: `OR.03`, `CS.01`, `BASE-05`, `FS-02`
- **Status**: 🏛️ Accepted

---

## 1. 🎯 目的と概要 (Purpose & Overview)

データベースおよび永続化レイヤーは、ネットワーク分断、デッドロック、楽観的排他競合、およびクラウド側のスロットリング（429）などの物理的障害の最前線に位置します。

本文書は、`VK.Blocks.Persistence` サブシステムにおいて想定される主要な**障害モード（Failure Modes）**、それらに対応する**縮退挙動（Degraded Behavior）**、下流ハンドラーへ返却される **`VKResult` エラーコード（`CS.01`）**、および推奨される**回復ポリシー（Recovery / Retry Policy）** を定義する運用ハンドブックです。

---

## 2. 📊 障害モードマトリクス (Failure Modes Matrix)

| 障害モード (Failure Mode) | トリガー条件 | システムの縮退挙動 (Behavior) | 返却エラーコード (`CS.01`) | 推奨回復アクション |
| :--- | :--- | :--- | :--- | :--- |
| **F-01: データベース一時的接続切断** | 一時的なネットワーク瞬断、DB フェイルオーバー | Polly により即座に指数バックオフリトライ（3 回）を実行。全滅時は即座に Fail-Closed。 | `Persistence.Database.Unavailable` (503) | Polly による自動回復。呼び出し側は HTTP 503 として短時間待機後に再送。 |
| **F-02: 楽観的排他制御競合** | 同一エンティティに対する同時更新（`RowVersion` 不一致） | 即座にトランザクションをロールバックし、競合エラーとして返却。 | `Persistence.Concurrency.Conflict` (409) | 最新状態を再取得（`GetByIdAsync`）した上で、業務ロジックを再実行して再保存。 |
| **F-03: 一意制約違反 (Unique Constraint)** | 重複するキー（メールアドレス、伝票番号等）の挿入 | 外部キー/一意インデックス違反を捉え、例外ではなく Result エラーへマッピング。 | `Persistence.Entity.DuplicateKey` (409) | 呼び出し元で入力値を修正するか、冪等処理として既存エンティティを取得。 |
| **F-04: Cosmos DB RU スロットリング** | 瞬間的なリクエスト急増による RU 上限超過（HTTP 429） | Cosmos SDK および Polly が `x-ms-retry-after-ms` を読み取り、自動で待機リトライ。 | `Persistence.Cosmos.Throttled` (429) | RU 容量の自動スケール引き上げ検討。呼び出し元は `Retry-After` 待機。 |
| **F-05: Transactional Outbox 保存失敗** | SaveChanges 時の Outbox レコードシリアライズ失敗 | トランザクション全体をロールバックし、業務データの変更も破棄（原子性の死守）。 | `Persistence.Outbox.SaveFailed` (500) | ペイロードのサイズ制限確認、循環参照オブジェクトのシリアライズ排除。 |
| **F-06: SQLite データベースロック** | 複数プロセス/スレッドによる SQLite への同時書き込み | SQLite ビジープールがタイムアウト（デフォルト 30 秒）まで待機。 | `Persistence.Sqlite.DatabaseBusy` (503) | WAL（Write-Ahead Logging）モードの有効化、接続文字列のプーリング確認。 |

---

## 3. 🛠️ 回復・耐障害性パターン (Resilience Patterns)

### 3.1 一時的障害の Polly ポリシー (`OR.03`)
リレーショナル DB への接続およびクエリ実行には、以下の Polly ポリシーがパイプライン境界で適用されます：
- **Retry Policy**: 3 回の指数バックオフ（Jitter 付き: 100ms, 200ms, 400ms）。
- **Circuit Breaker**: 連続 5 回の接続失敗で 30 秒間サーキットを開放（Open）。後続リクエストを即座に `503 Unavailable` で遮断し、DB の過負荷自壊を防止。
- **Timeout**: 個別クエリタイムアウト（既定 30 秒）。

### 3.2 楽観的排他制御競合のハンドリング例 (`F-02`)

```csharp
var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

if (result.IsFailure)
{
    if (result.Error.Code == VKPersistenceErrors.ConcurrencyConflict)
    {
        // 1. 最新エンティティを再取得
        var latest = await _repository.GetByIdAsync(orderId, cancellationToken);
        
        // 2. 業務ルールを再適用
        latest.ApplyUpdate(...);
        
        // 3. 再コミット試行
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    return result; // その他のエラーは呼び出し側へ伝播
}
```

---

## 4. 📡 運用可観測性とアラート基準 (`OR.01`, `BB.04`)

SRE およびプラットフォーム運用チームは、以下のテレメトリシグナルを監視します：

1. **`persistence_db_query_duration_seconds`**:
   - $p95 > 200$ms または $p99 > 1,000$ms の場合、インデックス欠落またはテーブルスキャンの発生を警戒。
2. **`persistence_concurrency_conflicts_total`**:
   - 突発的な急増（スパイク）は、同一エンティティに対する不適切なホットスポット集中アクセスを示す。
3. **`persistence_outbox_pending_count`**:
   - Outbox の未処理件数が 1,000 件を超過して増加し続けている場合、下流のメッセージディスパッチャージョブ（`BackgroundJobs`）の遅延またはブローカー停止を検知。

