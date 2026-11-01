# 📜 Public API Surface & Compatibility Policy: VK.Blocks.Authorization

- **System ID / Package ID**: `VK.Blocks.Authorization`
- **Domain / Area**: `Security`
- **Target Layer**: L2 Domain BuildingBlock / L3 Infrastructure Adapters
- **Status**: 🏛️ Accepted
- **Last Verified**: 2026-10-30
- **Governing Rules**: `ARCH-07`, `AP.03`, `FS-09`, `BASE-01`, `CS.01`, `BB.01`

---

## 1. 🎯 目的とスコープ (Purpose & Scope)

本文書は、`VK.Blocks.Authorization` が下流マイクロサービス、Web API コントローラー、および AI コーディングエージェントに対してコミットする **Level 1 公開契約（Public API Surface）** の境界線と、**SemVer 互換性・非推奨化ガバナンス** を明文化する SSOT（単一の真実の源）です。

> [!IMPORTANT]
> `ARCH-07` および `AP.03` に基づき、明示的に Level 1 公開契約として承認された型・メンバ以外はすべて `internal` でカプセル化されます。AI エージェントおよび開発者は、`Internal/` 配下のクラス（各機能の Handler や Default Provider）を下流アセンブリから直接参照したり、不用意に `public` 化したりすることを固く禁じます。

---

## 2. 🏛️ Level 1 公開契約型一覧 (Public Exports Baseline — `AP.03`, `ARCH-07`)

### 2.1 モジュール基盤および共通基底 (`Common/`, Root)

| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKAuthorizationBlock` | `sealed partial class` | ブロックマーカー型（`IVKBlockMarker`）。Source Generator が URN とバージョンを自動合成（`ADR-012`, `ADR-014`）。 |
| `VKAuthorizationOptions` | `sealed partial record` | モジュール最上位構成オプション（`IVKToggleableBlockOptions`）。全機能の有効化・無効化を統括。 |
| `IVKAuthorizationBuilder` | `interface` | 各認可機能スライスを流暢にチェーン登録するためのビルダーポート（`ADR-013`）。 |
| `VKAuthorizationExtensions` | `static class` | `IServiceCollection.AddVKAuthorizationBlock(...)` 公開登録拡張メソッド。 |
| `VKAuthorizationBuilderExtensions` | `static class` | `AddRoles()`, `AddPermissions()`, `AddTenantIsolation()` などの機能チェーン拡張メソッド群。 |
| `VKAuthorizationErrors` | `static class` | RFC 7807 準拠の構造化エラーコード定数群（`CS.01`, `FS-02`）。 |
| `VKAuthorizationClaimTypes` | `static class` | 認可パイプラインが解釈する標準クレーム型定数群。 |
| `VKAuthorizationPolicies` | `static class` | 共通ポリシー名の定数カタログ。 |
| `IVKAuthorizationAuditHook` | `interface` | 認可拒絶（Denial）イベントを非同期傍受するプラグイン監査フックポート。 |

### 2.2 垂直スライス公開契約 (Feature Slices Public Surface)

#### A. Permissions スライス (`Permissions/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKAuthorizePermissionAttribute` | `sealed class` | `IAuthorizationRequirementData` を実装した宣言的権限認可属性（`ADR-006`）。 |
| `VKGeneratePermissionsAttribute` | `sealed class` | Source Generator に権限属性自動合成を指示するメタデータマーカー（`ADR-007`）。 |
| `IPermissionEvaluator` | `interface` | 権限評価の中核契約。`Task<VKResult<bool>> HasPermissionAsync(...)` を提供（`ADR-005`, `CS.01`）。 |
| `IVKPermissionProvider` | `interface` | ユーザーまたはロールに紐づく有効権限セットを解決するポート。 |
| `IVKPermissionStore` | `interface` | 権限マスタの永続化・照会ポート（差分同期時に利用）。 |
| `IVKPermissionOptions` | `interface` | 開閉トグルとキャッシュ時間を定義する Options 契約（`IVKToggleableBlockOptions` 継承, `ADR-017`）。 |
| `VKPermissionOptions` | `sealed partial record` | Permissions スライスの静的オプション実装。 |
| `VKPermission` | `sealed record` | 権限マスタエンティティ（Id, Code, Module, DisplayName, Description）。 |
| `PermissionsCatalog` | `static partial class` | Source Generator により自動生成される全権限一覧（`All`）および 64-bit 指紋（`MetadataHash`）。 |

#### B. TenantIsolation スライス (`TenantIsolation/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKAuthorizeTenantIsolationAttribute` | `sealed class` | テナント境界強制認可属性。 |
| `IVKTenantEvaluator` | `interface` | テナントコンテキスト整合性を評価するポート。 |
| `IVKUserTenantProvider` | `interface` | ユーザーが所属する有効テナント一覧を取得するポート。 |
| `VKTenantIsolationOptions` | `sealed partial record` | テナント分離の有効化および例外パスオプション。 |

#### C. Entitlements スライス (`Entitlements/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKRequireTenantFeatureAttribute` | `sealed class` | テナントの契約プラン・機能有効化を検証する属性（`ADR-015`）。 |
| `VKTenantFeatureRequirement` | `sealed record` | `FeatureId` を保持する認可要件データ型。 |
| `VKEntitlementsOptions` | `sealed partial record` | テナント機能認可の構成オプション。 |

#### D. Roles スライス (`Roles/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKAuthorizeRolesAttribute` | `sealed class` | 複数ロールの Any / All を評価する属性。 |
| `IVKRoleEvaluator` | `interface` | ロール合致判定ポート。 |
| `IVKRoleProvider` | `interface` | ユーザーの所属ロール一覧を提供するポート。 |
| `IVKRoleOptions` | `interface` | ロール構成契約（`IVKToggleableBlockOptions` 継承, `ADR-017`）。 |
| `VKRoleOptions` | `sealed partial record` | ロール構成オプション（`RoleClaimType` 等）。 |

#### E. WorkingHours スライス (`WorkingHours/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKAuthorizeWorkingHoursAttribute` | `sealed class` | 業務時間帯制限属性。 |
| `IVKWorkingHoursEvaluator` | `interface` | Semantic-Priority パターンを適用した業務時間判定ポート（`ADR-016`）。 |
| `IVKWorkingHoursProvider` | `interface` | テナント別またはポリシー別の業務時間帯定義を提供するポート。 |
| `VKWorkingHoursOptions` | `sealed partial record` | デフォルトタイムゾーンおよび営業時間設定。 |

#### F. MinimumRank スライス (`MinimumRank/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKAuthorizeMinimumRankAttribute` | `sealed class` | 数値化された役職ランク・VIP レベルを検査する属性（`ADR-006`）。 |
| `IVKMinimumRankEvaluator` | `interface` | 最低ランク判定ポート。 |
| `IRankProvider` | `interface` | ユーザーの現在ランクを取得するポート。 |
| `VKMinimumRankOptions` | `sealed partial record` | ランククレーム名と基準値設定。 |

#### G. InternalNetwork スライス (`InternalNetwork/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKAuthorizeInternalNetworkAttribute` | `sealed class` | 内部 IP 帯域（CIDR）制限属性。 |
| `IVKInternalNetworkEvaluator` | `interface` | IP アドレス判定ポート。 |
| `IIpAddressProvider` | `interface` | リクエストの送信元 IP を抽出するポート。 |
| `VKInternalNetworkOptions` | `sealed partial record` | 許可 CIDR レンジおよびプロキシ透過設定。 |

#### H. DynamicPolicies スライス (`DynamicPolicies/`)
| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `VKDynamicAuthorizeAttribute` | `sealed class` | 動的属性・演算子（Equals, In, Exists）評価属性（`ADR-003`, `006`）。 |
| `IVKDynamicPoliciesEvaluator` | `interface` | 動的ルール評価ポート。 |
| `VKDynamicRequirement` | `sealed record` | 属性名、演算子、期待値を保持する要件モデル。 |
| `VKAuthorizationOperator` | `enum` | 動的評価演算子列挙型（`Equals`, `NotEquals`, `In`, `GreaterThan` 等）。 |
| `VKDynamicPoliciesOptions` | `sealed partial record` | 動的ポリシー評価オプション。 |

---

## 3. 🔒 内部カプセル化境界 (`Internal/` Boundary — `AP.03`, `BB.01`)

以下のコンポーネントは **厳格に `internal sealed`** とされ、ライブラリ外部への露出が禁止されています：

1. **認可ハンドラー具象クラス**:
   - `TenantAuthorizationHandler`, `PermissionHandler`, `RoleHandler`, `TenantFeatureAuthorizationHandler`, `WorkingHoursAuthorizationHandler`, `MinimumRankAuthorizationHandler`, `InternalNetworkAuthorizationHandler`, `DynamicRequirementHandler`。
   - ※ ASP.NET Core の認可システムへは、DI 拡張経由で `IAuthorizationHandler` または専用 Evaluator インターフェースとして間接バインドされます（`ADR-011`）。
2. **デフォルトプロバイダー実装**:
   - `DefaultPermissionProvider`, `DefaultRoleProvider`, `DefaultUserTenantProvider`, `DefaultWorkingHoursProvider`, `DefaultRankProvider`, `DefaultIpAddressProvider`, `DefaultDynamicPoliciesEvaluator`。
3. **診断およびログメッセージ**:
   - 各スライスの `XxxLog.cs`（`[LoggerMessage]` 定義クラス）および内部診断コンテキスト。

---

## 4. ⚖️ SemVer & 互換性ガバナンスポリシー (`FS-09`, `AP.03`)

### 4.1 破壊的変更（Breaking Changes）の判定基準
以下の変更は **メジャーバージョンアップ（例: 2.0.0）** を要求し、事前の ADR（`DL.03`）が必須となります：
- 公開インターフェース（`IPermissionEvaluator` 等）のシグネチャ変更またはメソッド削除。
- 公開属性（`VKAuthorizePermissionAttribute` 等）のプロパティ削除・型変更。
- 既存の公開エラーコード（`VKAuthorizationErrors` 内の文字列値）の変更・削除。
- 認可評価の既定のフェイルクローズ挙動（Fail-Closed）の緩和。

### 4.2 後方互換な追加変更（Minor Additions）
- C# 8.0+ の **Default Interface Methods (DIM)** を用いた公開インターフェースへの新規メソッド追加。
- 既存 Options クラスへのオプショナルプロパティ（既定値付き）の追加。
- 新規の垂直スライスの追加（既存機能への影響ゼロ）。

### 4.3 非推奨化（Deprecation）プロセス
1. 対象の公開シンボルに `[Obsolete("Use NewApi instead. Will be removed in vNext.", false)]` を付与。
2. ドキュメントおよび Release Notes に移行コード（Before/After）を明記。
3. 最低 1 マイナーバージョン期間の猶予を経てから削除。

