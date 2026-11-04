# Failure Modes: VectorSearch Subsystem

- **Scope**: `VK.Blocks.VectorSearch`
- **Specification Type**: Degradation & Failure Handling (`OR.03`, `CS.01`)
- **Status**: ✅ Active

---

## 1. 💥 想定障害モードと縮退方針

| 障害シナリオ | 根本原因 | 基座の挙動 (Degradation) | 返却エラー | 推奨アクション |
| :--- | :--- | :--- | :--- | :--- |
| **下流ベクトルストア接続断** | Qdrant / DB の障害・停止 | Fail-Fast でエラー返却 | `VectorSearch.StoreUnavailable` | キャッシュ（Redis/SemanticCache）の有効化 |
| **リランキングタイムアウト** | Cross-Encoder / LLM の高負荷 | **縮退 (Soft-Degrade)**: Rerank をスキップし、RRF 粗召回順位を返却 | なし（ログに Warning 記録） | Rerank モデルのスケールアウト |
| **空クエリ投入** | ユーザー入力が空白文字列 | パイプライン即時中断 | `VectorSearch.EmptyQuery` | UI / 入力境界でのバリデーション |
| **多系統検索の片肺停止** | BM25 は成功したが Dense がタイムアウト | 成功した系統のみで RRF 計算を実施し結果返却 | なし（部分縮退） | メトリクス監視で Dense エンジンの負荷追跡 |

