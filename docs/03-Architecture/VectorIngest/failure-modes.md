# Failure Modes: VectorIngest Subsystem

- **Scope**: `VK.Blocks.VectorIngest`
- **Specification Type**: Parsing & Embedding Resilience (`OR.03`, `CS.01`)
- **Status**: ✅ Active

---

## 1. 💥 想定障害モードと縮退方針

| 障害シナリオ | 根本原因 | 基座の挙動 | 返却エラー | 推奨アクション |
| :--- | :--- | :--- | :--- | :--- |
| **未対応ファイル形式** | サポート外の拡張子または破損 MIME | パーサー解決時に拒絶 | `VectorIngest.UnsupportedMimeType` | 許可拡張子の事前バリデーション、またはカスタムパーサー追加 |
| **Embedding レートリミット (429)** | Azure OpenAI / OpenAI の制限超過 | Polly 指数バックオフ待機後にリトライ (3回) | `VectorIngest.EmbeddingFailed` | バッチサイズ（`MaxBatchSize`）の縮小、TPM クォータ引き上げ |
| **PDF 構文解析崩れ・パスワード保護** | 暗号化 PDF または破損バイナリ | パーサー内部で例外補足 | `VectorIngest.DocumentParsingFailed` | パスワード解除後の再投入、または人間介入キューへ退避 |
| **多ベクトル非対応エンジンへの投入** | `EnableMultiVector` が有効だがストアが非対応 | インジェスト前ケイパビリティ検証で即時遮断 | `VectorStore.MultiVectorNotSupported` | Qdrant 等の多ベクトル対応ストアの利用、またはオプション無効化 |

