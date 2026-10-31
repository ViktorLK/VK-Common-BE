# ADR 002: Session Token Propagation for Distributed Read-Your-Writes Consistency

- **Date**: 2026-08-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence.EFCore.Cosmos

---

## 1. Context (背景)

Azure Cosmos DB の既定の整合性レベルは **Session 一致性（Session Consistency）** である。これは、単一のクライアントセッション内において「自分の書き込みを即座に読み取れる（Read-Your-Writes）」および「単調読み取り（Monotonic Reads）」を低レイテンシ・低コストで保証する。

しかし、ステートレスなコンテナ環境（Kubernetes / Azure Container Apps）で動作するマイクロサービス群では、以下の課題が発生する：
1. **分散ノード間でのセッショントークンの欠落**:
   ユーザーの一連の操作において、書き込みリクエストを処理した Pod A と、直後の読み取りリクエストを処理した Pod B が異なる場合、Pod B は書き込み時の Cosmos DB Session Token を保持していないため、レプリケーション遅延により古いデータ（Stale Data）を読み取ってしまう。
2. **強一致性（Strong Consistency）への安易な引き上げのリスク**:
   整合性の問題を回避するために Cosmos DB 全体を強一致性に設定すると、書き込みレイテンシが約 2 倍に増加し、リージョン障害時の可用性が低下し、消費 RU（Request Units）コストが増大する。

## 2. Problem Statement (問題定義)

強一致性の高コストと可用性犠牲を回避しつつ、分散マイクロサービス環境下で HTTP リクエストをまたいで Cosmos DB の Session 一致性を透過的に維持・伝播するアーキテクチャが必要であった。

## 3. Decision (決定事項)

**Session Token のインフラ伝播抽象（`IVKCosmosSessionTokenAccessor` & `SessionTokenManager`）** を導入する。

### 1. セッショントークンアクセサーの導入
- **`IVKCosmosSessionTokenAccessor`**:
  - 現在の実行コンテキスト（HTTP リクエストヘッダー `x-ms-session-token` 等）からセッショントークンを抽出し、Cosmos クライアントの `ItemRequestOptions.SessionToken` に自動付与する。
  - 書き込み完了時に Cosmos DB から返却送出された最新の Session Token を抽出し、下流レスポンスヘッダーに格納する。
- **`SessionTokenManager`**:
  - セッショントークンのスコープ内キャッシュおよび伝播を一元管理。
  - `VKPersistenceEFCoreCosmosOptions.EnableSessionTokenPropagation` によるオプトイン制御（`BB.05`）。

```csharp
if (options.EnableSessionTokenPropagation)
{
    services.TryAddSingleton<SessionTokenManager>();
    services.TryAddSingleton<IVKCosmosSessionTokenAccessor>(sp => sp.GetRequiredService<SessionTokenManager>());
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: データベースアカウント全体を Strong 一致性に変更
- **Approach**: Cosmos DB のデフォルト整合性レベルを Strong に引き上げる。
- **Rejected Reason**: リージョン間レイテンシが跳ね上がり、スループットと耐障害性が著しく悪化するため却下。

### Option 2: 整合性同期を Redis 等の外部分散キャッシュに委託
- **Approach**: 書いたデータを即座に Redis にキャッシュし、読み取りはキャッシュから行う。
- **Rejected Reason**: キャッシュの無効化管理が複雑化し、Cosmos DB ネイティブの Session 一致性トークン機能を無駄にするため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **低コスト・低遅延での因果整合性**: 最も経済的な Session 一致性を維持したまま、マルチインスタンス間での「書き込み直後の読み取り不整合」を完全に解消。
- **透過的な統合**: Web レイヤーのミドルウェアと連携することで、業務コード側にはトークン受け渡しロジックが一切侵入しない（`ARCH-09`）。

### Negative
- クライアント（SPA / モバイル）または API Gateway が `x-ms-session-token` ヘッダーを後続リクエストへ伝播する責務を持つ。

### Mitigation
- 前端 BFF（Backend For Frontend）および Gateway レベルで Cookie / ヘッダーバインディングを自動化する規約を策定（`FS-01`）。

## 6. Implementation & Status
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence.EFCore.Cosmos/Connection/Internal/SessionTokenManager.cs`

