# Vector Search & Memory Retrieval Evaluation Baseline Specification

## 1. 目的と位置づけ
本仕様は、`VectorStore` / `VectorSearch` のリファクタリング（Microsoft.Extensions.VectorData 統合および不要抽象の削除）にあたり、Koto/PWP および `AI.Engram` における記憶検索の品質・多言語性・テナント安全性を定量評価するためのベースライン定義書である。

---

## 2. ゴールデンデータセット構成方針 (30〜50ケース)

### 2.1 データセットカテゴリと配分
| カテゴリ | 件数割合 | 目的・検証内容 |
| :--- | :--- | :--- |
| **A. 事実的エピソード検索 (Direct Fact)** | 35% | 単一セッション内の明確な事実（固有名詞、好み、約束事、特定時刻の出来事）の Recall |
| **B. 時間軸・更新された記憶 (Temporal Update)** | 20% | 「以前はXが好きだったが今はYが好きになった」等の状態変化において最新記憶を優先取得できるか |
| **C. 複数セッション横断 (Multi-Session Synthesis)** | 20% | セッションAの話題とセッションBの話題を関連付けて取得できるか（クロスエピソード検索） |
| **D. CJK多言語混在 (Multilingual / CJK)** | 10% | 日本語・中国語・英語が混在するクエリおよび記憶断片での形態素・部分一致・意味的適合性 |
| **E. 該当なし・無回答ケース (Negative / Unanswerable)** | 15% | 該当事実が存在しないクエリに対し、スコア閾値以下として適切に空または低関連として判定できるか（誤召回の防止） |

---

## 3. テナント分離レッドライン検証 (Security Invariant)

`OR.02` および `BASE-05` に従い、以下の条件を常時100%パスすることを必須とする：

1. **データ投入**:
   - `Tenant-Alpha` に「秘密コード: 987654」を記録。
   - `Tenant-Beta` に「秘密コード: 123456」を記録。
2. **実行検証**:
   - `Tenant-Alpha` のコンテキストから「秘密コード」を検索した場合、`Tenant-Beta` のレコードが検索結果上位・下位を問わず 1 件たりとも含まれないこと（Recall = 0 for other tenants）。
   - クエリレベルでのプレディケート注入が Provider レベルで機能していることを確認。

---

## 4. 評価指標 (IR & Performance Metrics)

| 指標 | 基準目標 | 説明 |
| :--- | :--- | :--- |
| **Recall@5** | 退化 0% (許容閾値: 現行 -2% 以内) | 正解記憶IDが Top 5 に含まれる割合 |
| **MRR (Mean Reciprocal Rank)** | 向上または維持 | 正解記憶が何番目にランクインしたかの逆数平均 |
| **Negative False Positive Rate** | 0% | カテゴリE（無回答）において閾値以上の誤回答を返さない割合 |
| **Search Latency (p50 / p95)** | p50 < 30ms, p95 < 80ms (InMemory/Sqlite) | 検索レイテンシの許容上限 |
| **Cross-Tenant Leakage** | **0.00% (ゼロトレランス)** | テナント横断漏洩の完全遮断 |

---

## 5. 消融実験 (Ablation Matrix) 計画

リファクタリングの各段階で以下の組み合わせを検証する：

1. **純粋Dense検索**（VectorStore標準）
2. **ハイブリッド検索**（ネイティブまたは自前RRF融合）
3. **リランキング有無**（Top 20取得後のCross-Encoder / Cohere適用）
4. **CJKトークン境界**（標準空白区切り vs トライグラム/形態素を意識した分割）

