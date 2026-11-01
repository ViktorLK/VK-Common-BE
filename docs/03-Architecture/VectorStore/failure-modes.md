# Failure Modes & Resilience: VectorStore Subsystem

- **Scope**: `VK.Blocks.VectorStore` および各種アダプター（`Pgvector`, `Qdrant`, `Cosmos`, `Sqlite`）
- **Specification Type**: Failure Modes, Fallback & SRE Runbook (`OR.03`, `CS.01`)
- **Status**: ✅ Active

---

## 1. 💥 想定障害モードと基座挙動 (Failure Modes & System Behavior)

| 障害シナリオ (Failure Scenario) | 根本原因 (Root Cause) | 基座の挙動 (System Behavior) | 返却エラーコード (`CS.01`) | 推奨ハンドリング (Downstream Action) |
| :--- | :--- | :--- | :--- | :--- |
| **テナント未指定検索** | 呼び出し側が `TenantId` を渡さずに検索 | 即座に検索を中断（Fail-Fast） | `VectorStore.MissingTenantId` | アプリケーション層で実行コンテキストから `TenantId` を注入 |
| **ベクトル次元不一致** | コレクション定義長と異なるベクトルを投入 | 投入前検証で拒絶 | `VectorStore.DimensionMismatch` | モデル・Embedding エンジンの次元設定の一致を確認 |
| **外部エンジン接続断** | Qdrant / Cosmos クラスターの瞬断・停止 | Polly サーキットブレーカー開放、`VKResult.Failure` 返却 | `VectorStore.ExternalFailure` | バックオフ待機後の再試行、または縮退キャッシュの利用 |
| **リレーショナル拡張機能未導入** | PostgreSQL に `pgvector` 拡張機能が未作成 | アプリケーション起動時（HostedService）で即時起動停止（Fail-Fast） | `VectorStore.SchemaValidationFailed` | `CREATE EXTENSION vector;` 移行の実行 |
| **非サポート機能要求** | `RelationalColumn` に対する動的コレクション作成要求 | 例外ではなく typed な不許可返却 | `VectorStore.LifecycleNotSupported` | EF Core 移行による DDL 管理への切り替え |
| **`sqlite-vec` ネイティブ未検出** | 指定パスまたは実行 OS に対応する `.dll`/`.so` が不在 | 接続初期化時に検知し即座に失敗 | `VectorStore.ExternalFailure` | OS アーキテクチャ（x64 / ARM64）とランタイムネイティブ配置の確認 |
| **SQLite ファイルロック競合** | マルチプロセス書き込みによる `SQLITE_BUSY` | `BusyTimeoutMs`（既定 5000ms）まで待機後失敗 | `VectorStore.ExternalFailure` | WAL モードの有効化、または単一書き込み接続プール設定 |

---

## 2. 🛡️ 耐障害性・縮退方針 (Resilience & Degradation Strategy)

1. **Fail-Closed 原則 (`CS.08`, `OR.02`)**:
   - テナント分離や認可に関わる不整合が検知された場合、部分的な結果を返却せず、必ず処理全体を失敗（`Fail-Closed`）させます。
2. **Polly によるペリメーター保護 (`OR.03`)**:
   - 外部 gRPC / HTTP 呼び出しには、3 回の指数バックオフリトライ、Circuit Breaker、および 5 秒の明示的タイムアウトを設定。
3. **組込エンジン (SQLite) の同時実行性制御**:
   - 書き込み競合を低減するため、SQLite 接続では既定で WAL（Write-Ahead Logging）モードを有効化し、`BusyTimeoutMs` により一時的な競合を透過リトライ。
4. **運用可観測性 (`OR.01`, `BB.04`)**:
   - 通信失敗時は、送信ベクトルデータを除去（PII / 機密データ保護）した上で、ステータスコード、コレクション名、テナント識別子のみを構造化ログに出力。
