# 📖 Data Dictionary & Contract Catalog: `VK.Blocks.Authentication`

> **Governance**: 本ドキュメントに記載されたエラーコード、テレメトリタグ、およびスキーマ定義は Level 1 公開契約（`ARCH-07`）であり、後方互換性のない変更には新バージョンの発行と ADR が必須です。

---

## 1. 🚨 Error Code Catalog (`CS.01`, `FS-02`)

> 命名規則: `{Module}.{Feature}.{Reason}`。Backend の各 `VK*Errors` 定数と Frontend の `as const` マッピングで完全一致（Isomorphic Error Model）させます。

| Error Constant (C#) | Wire Error Code (`string`) | `VKErrorType` | HTTP Status | Description & Remediation (発生条件と対処) |
| :--- | :--- | :--- | :---: | :--- |
| `VKAuthenticationErrors.InvalidClaims` | `Auth.InvalidClaims` | `Unauthorized` | `401` | 必須クレーム（`sub`, `tenant_id` 等）がプリンシパルから欠落しています。 |
| `VKAuthenticationErrors.TenantIsolationFailed` | `Auth.TenantIsolationFailed` | `Forbidden` | `403` | テナント分離チェックに失敗しました（テナント ID が無効またはアクセス権がありません）。 |
| `VKJwtErrors.EmptyToken` | `Auth.EmptyToken` | `Validation` | `400` | 提供された JWT トークンが空または空白文字です。 |
| `VKJwtErrors.ConfigurationError` | `Auth.ConfigurationError` | `Failure` | `500` | 認証インフラの設定（Issuer / SigningKey 等）が不完全です。 |
| `VKJwtErrors.InvalidFormat` | `Auth.InvalidFormat` | `Validation` | `400` | トークンの形式が有効な JWT（Base64URL 形式）ではありません。 |
| `VKJwtErrors.Revoked` | `Auth.Revoked` | `Unauthorized` | `401` | トークンが失効リストに登録済みです。再ログインが必要です。 |
| `VKJwtErrors.Expired` | `Auth.Expired` | `Unauthorized` | `401` | トークンの有効期限（`exp`）が切れています。リフレッシュトークンで再取得してください。 |
| `VKJwtErrors.Invalid` | `Auth.Invalid` | `Unauthorized` | `401` | 署名不一致または一般的な JWT 検証エラーです。 |
| `VKJwtErrors.IssuerInvalid` | `Auth.IssuerInvalid` | `Unauthorized` | `401` | トークンの発行者（`iss`）が許可構成と一致しません。 |
| `VKJwtErrors.AudienceInvalid` | `Auth.AudienceInvalid` | `Unauthorized` | `401` | トークンのオーディエンス（`aud`）が許可構成と一致しません。 |
| `VKJwtErrors.RateLimitExceeded` | `Auth.RateLimitExceeded` | `TooManyRequests` | `429` | トークン検証またはリフレッシュの試行回数が上限を超過しました。 |
| `JwtRefreshTokenErrors.InvalidIds` | `RefreshToken.InvalidIds` | `Validation` | `400` | トークンの JTI または Family ID が欠落しています。 |
| `JwtRefreshTokenErrors.Compromised` | `RefreshToken.Compromised` | `Unauthorized` | `401` | 既に使用済みのリフレッシュトークンが再利用されました（Replay Attack 検知）。 |
| `JwtRefreshTokenErrors.Expired` | `RefreshToken.Expired` | `Unauthorized` | `401` | リフレッシュトークンが有効期限切れです。 |
| `JwtRefreshTokenErrors.ServiceUnavailable` | `RefreshToken.ServiceUnavailable` | `Failure` | `503` | トークン失効・リフレッシュ基盤（Redis 等）が一時的に利用不可です。 |
| `VKApiKeyErrors.Empty` | `ApiKey.Empty` | `Validation` | `400` | API Key ヘッダーまたはクエリパラメータが空です。 |
| `VKApiKeyErrors.Invalid` | `ApiKey.Invalid` | `Unauthorized` | `401` | 指定された API Key が存在しないか無効です。 |
| `VKApiKeyErrors.Revoked` | `ApiKey.Revoked` | `Unauthorized` | `401` | API Key が明示的に無効化（Revoke）されています。 |
| `VKApiKeyErrors.Expired` | `ApiKey.Expired` | `Unauthorized` | `401` | API Key の有効期限が切れています。 |
| `VKApiKeyErrors.Disabled` | `ApiKey.Disabled` | `Unauthorized` | `401` | API Key が管理者によって一時停止（Disabled）されています。 |
| `VKApiKeyErrors.RateLimitExceeded` | `ApiKey.RateLimitExceeded` | `TooManyRequests` | `429` | API Key のリクエストレート上限を超過しました。`Retry-After` を確認してください。 |
| `VKOAuthErrors.ProviderNotSupported` | `OAuth.ProviderNotSupported` | `Failure` | `400` | 要求された OAuth プロバイダー（例: Google, GitHub）が未サポートです。 |
| `VKOAuthErrors.InvalidState` | `OAuth.InvalidState` | `Failure` | `400` | OAuth コールバックの state パラメータが無効または期限切れです（CSRF 防護）。 |
| `VKOAuthErrors.RemoteFailure` | `OAuth.RemoteFailure` | `Failure` | `502` | 外部 ID プロバイダーからの認証応答でエラーを受信しました。 |
| `VKOAuthErrors.MissingRequiredClaim` | `OAuth.MissingRequiredClaim` | `Failure` | `502` | 外部プロバイダーから必要なクレーム（メールアドレス等）が返却されませんでした。 |
| `VKOAuthErrors.MapperNotFound` | `OAuth.MapperNotFound` | `Failure` | `500` | プロバイダー固有のクレームマッパーが登録されていません。 |
| `VKOAuthErrors.UserInfoFailure` | `OAuth.UserInfoFailure` | `Failure` | `502` | 外部プロバイダーの UserInfo エンドポイントからのプロファイル取得に失敗しました。 |

---

## 2. 🔢 Enum & Wire Serialization Rules (`FS-01`, `FS-09`, `FS-13`)

> ⚠️ **注意**: 個別の Enum メンバー定義は `VK.Tools.DocsGenerator` によって [`../Enums.md`](/docs/05-DataDictionary/Enums.md) および [`../enums.manifest.json`](/docs/05-DataDictionary/enums.manifest.json) に自動抽出されます。手動で重複テーブルを作成しないでください。

- **Wire Format (`FS-13`)**:
  - JSON シリアライズ時は **文字列（String）** として伝送します（例: `"Symmetric"`, `"OidcDiscovery"`）。
  - クライアント側（Frontend / BFF）は、未知の Enum 値を受信してもクラッシュしないフォールバック処理を保持すること（`FS-09`）。
- **管轄 Enum 一覧**:
  - `VKJwtAuthMode`: JWT インフラでサポートされる認証モード（`Symmetric = 0`, `OidcDiscovery = 1`）。
- **Evolution Rule (`FS-09`)**:
  - 既存メンバーの整数値変更・名前変更は禁止（Breaking Change）。
  - 新規メンバーの末尾追加（Additive）のみ同一メジャーバージョン内で許容。

---

## 3. 📡 Diagnostics & Telemetry Dictionary (`BB.04`, `ARCH-12`, `FS-11`)

> `VKAuthenticationDiagnosticsConstants` で公開される OpenTelemetry 計装契約。

### 3.1 Instrumentation Identity
- **ActivitySource / Meter Name**: `VK.Blocks.Authentication`（`[VKBlockDiagnostics]` により自動バインド）

### 3.2 Meters & Instruments

| Constant Name | Wire Instrument Key | Type | Unit | Description |
| :--- | :--- | :---: | :---: | :--- |
| `AuthRequestCounterName` | `authentication.requests` | `Counter<long>` | `{requests}` | 処理された認証リクエストの総数 |
| `TooManyRequestsCounterName` | `vk.auth.too_many_requests` | `Counter<long>` | `requests` | レートリミット制限を超過したリクエスト数 |
| `RevocationCounterName` | `vk.auth.revocations` | `Counter<long>` | `{revocations}` | トークン／ユーザー失効チェックにより拒絶された件数 |
| `ReplayCounterName` | `vk.auth.replay_detection` | `Counter<long>` | `{attacks}` | リフレッシュトークンのリプレイ攻撃検知数 |
| `ClaimsTransformationCounterName`| `vk.auth.claims_transformation.count` | `Counter<long>` | `{operations}` | クレーム変換の試行回数 |
| `ClaimsTransformationDurationName`| `vk.auth.claims_transformation.duration` | `Histogram<double>` | `ms` | クレーム変換処理の所要時間（ミリ秒） |

### 3.3 Semantic Tags & Activities

| Constant Name | Wire Tag / Activity Key | Type | PII Scrubbed | Description |
| :--- | :--- | :---: | :---: | :--- |
| `ActivityAuthenticateJwt` | `JwtAuthenticationService.AuthenticateAsync` | `Activity` | ✅ Safe | JWT 認証パイプラインのスパン |
| `ActivityValidateApiKey` | `ApiKeyValidator.ValidateAsync` | `Activity` | ✅ Safe | API Key 検証処理のスパン |
| `ActivityTransformClaims` | `VKClaimsTransformer.TransformAsync` | `Activity` | ✅ Safe | クレーム補強・変換処理のスパン |
| `ActivityIsRevoked` | `JwtTokenRevocationService.IsRevokedAsync` | `Activity` | ✅ Safe | トークン失効チェック処理のスパン |
| `TagAuthType` | `auth.type` | `string` | ✅ Safe | 認証種別（`"jwt"`, `"apikey"`） |
| `TagAuthResult` | `auth.result` | `string` | ✅ Safe | 認証結果（`"Success"`, `"Failure"`） |
| `TagUserId` | `auth.user.id` | `string` | ⚠️ Safe (Hashed) | 認証対象のユーザー識別子（平文個人情報はマスク必須 `OR.02`） |
| `TagKeyId` | `auth.key_id` | `string` | ✅ Safe | API Key の一意識別子（秘密鍵文字列の出力は禁止） |
| `TagTenantId` | `auth.tenant_id` | `string` | ✅ Safe | コンテキストのテナント識別子（`FS-07`） |
| `TagFailureReason` | `auth.failure_reason` | `string` | ✅ Safe | 認証失敗理由（Error Code に準拠） |
| `TagClaimsTransformed` | `auth.claims_transformed` | `boolean` | ✅ Safe | クレーム変換が実行されたか否かのフラグ |

---

## 4. 🗄️ Persistence Schema Dictionary (`CS.08`, `OR.02`)

> 永続化を伴う認証データストア（`IVKApiKeyStore`, `IVKSessionStore`, `IVKAccountLinkService`, `IVKLoginTracker` 等）の物理スキーマ規約。

### 4.1 Table: `Auth_ApiKeys` (`VKApiKeyRecord`)
- **Multi-Tenancy**: `TenantId` (`NOT NULL`, EF Core Global Query Filter 必須 `OR.02`)
- **Primary Key**: `Id` (`uniqueidentifier`, Sequential UUIDv7 `CS.08`, `FS-04`)

| Column Name | DB Type | Nullable | MaxLength / Precision | Constraint / Index (`IX_` / `FK_` / `PK_`) |
| :--- | :--- | :--- | :---: | :--- |
| `Id` | `uniqueidentifier` | `NOT NULL` | - | `PK_Auth_ApiKeys` |
| `TenantId` | `varchar(64)` | `NOT NULL` | 64 | `IX_Auth_ApiKeys_TenantId_OwnerId` (複合インデックス) |
| `OwnerId` | `nvarchar(128)` | `NOT NULL` | 128 | `IX_Auth_ApiKeys_TenantId_OwnerId` |
| `IsEnabled` | `bit` | `NOT NULL` | - | ステータス管理フラグ |
| `ExpiresAt` | `datetimeoffset` | `NULL` | - | `IX_Auth_ApiKeys_ExpiresAt` (失効バッチ用) |
| `Scopes` | `nvarchar(max)` / JSON | `NOT NULL` | - | 付与スコープ配列の JSON シリアライズ |

### 4.2 Table: `Auth_Sessions` (`VKSessionInfo`)
- **Session Lifecycle**: 分散キャッシュまたは永続セッションストア対応

| Column Name | DB Type | Nullable | MaxLength / Precision | Constraint / Index |
| :--- | :--- | :--- | :---: | :--- |
| `SessionId` | `varchar(128)` | `NOT NULL` | 128 | `PK_Auth_Sessions` |
| `UserId` | `nvarchar(128)` | `NOT NULL` | 128 | `IX_Auth_Sessions_UserId` |
| `TicketData` | `nvarchar(max)` | `NOT NULL` | - | 暗号化された認証チケットバイナリ/Base64 |
| `ExpiresAt` | `datetimeoffset` | `NOT NULL` | - | `IX_Auth_Sessions_ExpiresAt` |
| `CreatedAt` | `datetimeoffset` | `NOT NULL` | - | セッション発行日時 |

### 4.3 Table: `Auth_AccountLinks` (`VKAccountLinkInfo`)
- **External Identity Federation**: 外部 IDP（Google, GitHub 等）とローカルアカウントの紐付け

| Column Name | DB Type | Nullable | MaxLength / Precision | Constraint / Index |
| :--- | :--- | :--- | :---: | :--- |
| `UserId` | `nvarchar(128)` | `NOT NULL` | 128 | `FK_Auth_AccountLinks_Users_UserId` |
| `LoginProvider` | `varchar(64)` | `NOT NULL` | 64 | `PK_Auth_AccountLinks` (複合 PK: `LoginProvider` + `ProviderKey`) |
| `ProviderKey` | `nvarchar(256)` | `NOT NULL` | 256 | `PK_Auth_AccountLinks` (複合 PK: `LoginProvider` + `ProviderKey`) |
| `ProviderDisplayName` | `nvarchar(128)` | `NOT NULL` | 128 | 表示用プロバイダー名 |
| `LinkedAt` | `datetimeoffset` | `NOT NULL` | - | 連携日時 |

### 4.4 Table: `Auth_LoginAuditLog` (`VKLoginRecord`)
- **Audit Storage**: 監査ログ記録。追記専用（Append-Only）

| Column Name | DB Type | Nullable | MaxLength / Precision | Constraint / Index |
| :--- | :--- | :--- | :---: | :--- |
| `Id` | `uniqueidentifier` | `NOT NULL` | - | `PK_Auth_LoginAuditLog` (Sequential UUIDv7) |
| `UserId` | `nvarchar(128)` | `NOT NULL` | 128 | `IX_Auth_LoginAuditLog_UserId_Timestamp` |
| `AuthenticationMethod` | `varchar(32)` | `NOT NULL` | 32 | 例: `"JWT"`, `"Cookie"`, `"ApiKey"` |
| `IpAddress` | `varchar(45)` | `NOT NULL` | 45 | IPv4 (15) / IPv6 (45) |
| `UserAgent` | `nvarchar(512)` | `NOT NULL` | 512 | クライアント User-Agent 文字列 |
| `DeviceFingerprint` | `varchar(128)` | `NULL` | 128 | 端末フィンガープリント |
| `Timestamp` | `datetimeoffset` | `NOT NULL` | - | `IX_Auth_LoginAuditLog_UserId_Timestamp` |
| `Success` | `bit` | `NOT NULL` | - | 認証成否フラグ |
| `FailureReason` | `varchar(128)` | `NULL` | 128 | 失敗時のエラーコード（`CS.01` 準拠） |

