# 💥 Failure Modes & Resilience Handbook: VK.Blocks.Authorization

- **System ID / Package ID**: `VK.Blocks.Authorization`
- **Domain / Area**: `Security`
- **Target Audience**: SRE、セキュリティ運用者、下流サービス開発者、AI ペアプログラミング
- **Governing Rules**: `OR.03`, `CS.01`, `BASE-05`, `FS-02`, `Manifest 5`
- **Status**: 🏛️ Accepted

---

## 1. 🎯 目的と概要 (Purpose & Overview)

認可レイヤーは、不正アクセスや権限昇格、テナント越境データ漏洩を阻止する**最終防衛ライン**です。

認可システムにおける判定不能、タイムアウト、または外部ストアの接続断は、いかなる場合も安全側に倒す（**Fail-Closed 原則**）必要があります。

本文書は、`VK.Blocks.Authorization` において発生し得る主要な**障害モード（Failure Modes）**、システムの**縮退挙動（Degraded Behavior）**、返却される **`VKResult` エラーコード（`CS.01`）**、および SRE / 運用者向けの**回復・調査 Runbook** を定義します。

---

## 2. 📊 障害モードマトリクス (Failure Modes Matrix)

| 障害モード (Failure Mode) | トリガー条件 | システムの挙動 (Behavior) | 返却エラーコード (`CS.01`) | HTTP | 推奨回復・調査アクション |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **F-01: テナント境界不一致** | トークン内 `TenantId` とリクエスト現行コンテキストの不一致、またはクレーム欠落 | 即座にパイプラインを遮断し、後続ハンドラーの実行を拒絶（`ADR-001`）。 | `Authorization.TenantMismatch` | 403 | クライアントが送信した JWT のテナント ID と、ルーティング/ヘッダーの選択テナントの整合性を確認。 |
| **F-02: ユーザー操作権限不足 (PBAC)** | ユーザーが必要な権限（Permission）を保持していない | 評価器が `VKResult.Failure` を返し、ハンドラーが `context.Fail()`。 | `Authorization.PermissionDenied` | 403 | ユーザーのロール割当または権限マスタを確認。必要に応じてロールの再付与を申請。 |
| **F-03: テナント契約能力無効 (Entitlements)** | テナントの現在の契約プランで該当機能が無効化されている | `TenantFeatureAuthorizationHandler` が拒絶（`ADR-015`）。 | `Authorization.FeatureDenied` | 403 | テナントのサブスクリプションプランおよび有効化機能フラグを確認・アップグレード。 |
| **F-04: 業務時間外アクセス** | 許可された勤務時間帯（タイムゾーン、曜日、祝日）外のアクセス | `WorkingHoursAuthorizationHandler` が拒絶（`ADR-016`）。 | `Authorization.OutOfWorkingHours` | 403 | 緊急メンテ時の時間外アクセスの場合は、特権クレーム（SuperAdmin）または時間外申請を実施。 |
| **F-05: 役職ランク不足** | ユーザーの役職ランク値が要求最低基準（MinimumRank）を下回る | `MinimumRankAuthorizationHandler` が拒絶（`ADR-006`）。 | `Authorization.InsufficientRank` | 403 | 役職クレーム値の確認、または承認ワークフローによる上位者代行。 |
| **F-06: 外部 IP 制限拒絶** | 送信元 IP アドレスが許可された内部ネットワーク帯域（CIDR）外 | `InternalNetworkAuthorizationHandler` が拒絶。 | `Authorization.NetworkDenied` | 403 | 社内 VPN 経由での再接続、または API Gateway / リバースプロキシの `X-Forwarded-For` 透過確認。 |
| **F-07: 動的演算子設定不正** | `DynamicRequirement` に未定義または無効な演算子（Operator）が指定された | 評価器が不正リクエストとして構造化エラー返却（`ADR-006`）。 | `Authorization.InvalidOperator` | 400 | コントローラーに付与された `[VKDynamicAuthorize]` 属性の演算子設定を修正。 |
| **F-08: 外部権限ストア・分散キャッシュ停止** | Redis や権限 DB への接続タイムアウト・瞬断 | **Fail-Closed 原則（Manifest 5）** に従い、全滅時は即座に認可拒絶。 | `Authorization.StoreUnavailable` | 503 | Redis / DB クラスターのヘルスチェック、ネットワーク疎通確認、Polly 断路器の回復待機。 |

---

## 3. 🛡️ 耐障害性パターンと回復設計 (Resilience Design)

### 3.1 厳格なフェイルクローズ（Fail-Closed Policy — `Manifest 5`）
`VK.Blocks.Authorization` では、障害時に「とりあえず許可する（Fail-Open）」挙動は**セキュリティ上致命的**であるため、アーキテクチャレベルで禁止されています：
- 分散キャッシュが応答しない場合でも、未検証のままアクセスを許可しません。
- 外部プロバイダーのタイムアウト（既定 2 秒）超過時は、直ちに認可拒絶として処理されます。

### 3.2 外部プロバイダーの Polly サーキットブレーカー (`OR.03`)
外部 DB や分散 Redis に依存するカスタム Provider に対しては、以下の Polly ポリシーを境界アダプターに適用します：
- **Timeout**: 個別リクエストタイムアウト 2 秒。
- **Circuit Breaker**: 連続 5 回のタイムアウトまたは接続失敗で 30 秒間サーキットを開放（Open）。後続リクエストを高速に `503 Service Unavailable` で拒絶し、DB への過負荷を遮断。

---

## 4. 📋 SRE 運用トラブルシューティング Runbook

### 4.1 `Authorization.TenantMismatch` 多発時の初動
1. **Prometheus メトリクス確認**: `authz_denials_total{reason="Authorization.TenantMismatch"}` の急増を検知。
2. **ログ調査 (`OR.01`)**:
   ```json
   {
     "Event": "TenantIsolationDenied",
     "TraceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
     "ClaimTenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
     "ContextTenantId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
   }
   ```
3. **原因切り分け**:
   - フロントエンドが古いテナントセッションのまま他テナントのリソース URL を叩いているか。
   - API Gateway のルーティング書き換えバグによる `TenantId` 注入不整合か。

### 4.2 `Authorization.StoreUnavailable` 発生時の初動
1. 認可キャッシュストア（Redis）の CPU 使用率、コネクションプール枯渇状況を確認。
2. 緊急回復策として、`VKAuthorizationOptions.Permissions.CacheEnabled = false` によるローカル InMemory フォールバック（単一ノード運用時）の一時適用を検討。

