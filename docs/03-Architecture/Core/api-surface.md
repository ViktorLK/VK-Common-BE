# 📜 Public API Surface & Compatibility Policy: VK.Blocks.Core

- **System ID / Package ID**: `VK.Blocks.Core`
- **Domain / Area**: `Core / Foundation`
- **Target Layer**: L1 Foundation & Core Protocols (Single Source of Truth)
- **Status**: 🏛️ Accepted
- **Last Verified**: 2026-10-30
- **Governing Rules**: `ARCH-07`, `AP.03`, `FS-09`, `BASE-01`, `CS.01`

---

## 1. 🎯 目的とスコープ (Purpose & Scope)

本文書は、`VK.Blocks.Core` が全社エコシステムの全 BuildingBlock、下流マイクロサービス、および AI コーディングエージェントに対してコミットする **Level 1 公開契約（Public API Surface）** の全容と、**SemVer 破壊的変更判定基準** を明文化する SSOT（単一の真実の源）です。

> [!IMPORTANT]
> `ARCH-07` および `AP.03` に基づき、明示的に Level 1 公開契約として承認された型・メンバ以外はすべて `internal` でカプセル化されます。AI エージェントおよび開発者は、`Internal/` 配下のクラス（`InnerCache`, `DefaultSequentialGuidGenerator`, `NoOpSyncStateStore` 等）を下流アセンブリから直接参照したり、不用意に `public` 化したりすることを固く禁じます。

---

## 2. 🏛️ Level 1 公開契約型一覧 (Public Exports Baseline — `AP.03`, `ARCH-07`)

### 2.1 戻り値・エラー・例外モデル (`Results/`, `Errors/`, `Exceptions/`)

| 公開シンボル (Symbol) | 種別 | 名前空間 | 責務と利用規約 |
| :--- | :---: | :--- | :--- |
| `VKResult` | `sealed class` | `VK.Blocks.Core.Results` | 値を持たない操作の標準戻り値契約。`VKResult.Success()` はキャッシュ済みシングルトン。 |
| `VKResult<T>` | `sealed class` | `VK.Blocks.Core.Results` | 値を返す操作の標準戻り値契約。`IsSuccess`, `Value`, `Error` を提供。 |
| `VKError` | `sealed record` | `VK.Blocks.Core.Errors` | エラーコード（`Code`）、メッセージ、分類（`Type`）、詳細配列、拡張データを保持する不変値。 |
| `VKErrorType` | `enum` | `VK.Blocks.Core.Errors` | HTTP ステータスコード（400, 401, 403, 404, 409, 429, 502, 503, 504）に対応する標準エラー分類。 |
| `VKErrorDetail` | `sealed record` | `VK.Blocks.Core.Errors` | フィールドレベルのバリデーションエラー構造体。 |
| `VKErrorResponse` | `sealed record` | `VK.Blocks.Core.Errors` | RFC 7807 準拠の ProblemDetails シリアライズ用ルート DTO。 |
| `VKCoreErrors` | `static class` | `VK.Blocks.Core.Errors` | Core 基盤が定義する標準エラーコード定数群（`CS.01`）。 |
| `VKBaseException` | `abstract class` | `VK.Blocks.Core.Exceptions` | インフラ障害および回復不能エラー用のフレームワーク基底例外クラス。 |
| `VKValidationException` | `sealed class` | `VK.Blocks.Core.Exceptions` | バリデーション失敗時の例外表現。 |
| `VKNotFoundException` | `sealed class` | `VK.Blocks.Core.Exceptions` | リソース不在時の例外表現。 |
| `VKConflictException` | `sealed class` | `VK.Blocks.Core.Exceptions` | 競合・重複時の例外表現。 |

### 2.2 サービスマーカーおよび DI 拡張 (`Common/DependencyInjection/`)

| 公開シンボル (Symbol) | 種別 | 名前空間 | 責務と利用規約 |
| :--- | :---: | :--- | :--- |
| `IVKBlockMarker` | `interface` | `...Common.DependencyInjection` | すべての BuildingBlock が実装するマーカーインターフェース。URN、名称、依存関係を定義。 |
| `IVKBlockMarkerProvider<T>` | `interface` | `...Common.DependencyInjection` | 特定のブロックマーカー実体を DI コンテナから型安全に解決するポート。 |
| `IVKBlockOptions` | `interface` | `...Common.DependencyInjection` | C# 11 `static abstract string SectionName { get; }` を持つ構成オプション基底ポート。 |
| `IVKToggleableBlockOptions` | `interface` | `...Common.DependencyInjection` | `Enabled` フラグを持つ開閉可能オプション契約。 |
| `IVKBuilder<TMarker>` | `interface` | `...Common.DependencyInjection` | モジュラービルダーの基底インターフェース。 |
| `VKBlockRegistrationExtensions` | `static class` | `...Common.DependencyInjection` | `AddVKBlockMarker`, `AddVKBlockOptions` などのべき等登録拡張。 |
| `VKBlockQueryExtensions` | `static class` | `...Common.DependencyInjection` | `IsVKBlockRegistered`, `EnsureDependenciesRegistered` などの検証・走査拡張。 |
| `VKBlockBuilderExtensions` | `static class` | `...Common.DependencyInjection` | 流暢なビルダー構築を支援する拡張。 |

### 2.3 実行コンテキストとテナント境界 (`Context/`, `Tenancy/`)

| 公開シンボル (Symbol) | 種別 | 名前空間 | 責務と利用規約 |
| :--- | :---: | :--- | :--- |
| `VKTenantId` | `readonly record struct` | `VK.Blocks.Core.Tenancy` | フレームワーク唯一の強型テナント識別子。`VKTenantId.From(Guid)` ファクトリ経由で生成。 |
| `IVKTenantProvider` | `interface` | `VK.Blocks.Core.Tenancy` | 現在のスコープにおける `VKTenantId` 解決ポート。 |
| `VKAmbientExecutionContext` | `sealed class` | `VK.Blocks.Core.Context` | `AsyncLocal` によりテナント、ユーザー、トレース座標をスレッド横断で伝播。 |
| `IVKUserContext` | `interface` | `VK.Blocks.Core.Context` | 認証済みユーザーの `UserId`, `UserName`, `Roles` 解決ポート。 |

### 2.4 ドメインモデリングとリポジトリ抽象 (`Domain/`, `Specifications/`, `Repositories/`)

| 公開シンボル (Symbol) | 種別 | 名前空間 | 責務と利用規約 |
| :--- | :---: | :--- | :--- |
| `VKEntity<TId>` | `abstract class` | `VK.Blocks.Core.Domain` | ID 等価性（Equality）に基づくドメインエンティティ基底。 |
| `VKValueObject` | `abstract class` | `VK.Blocks.Core.Domain` | アロケーションフリーな全属性等価性（Structural Equality）を持つ値オブジェクト基底。 |
| `VKAggregateRoot<TId>` | `abstract class` | `VK.Blocks.Core.Domain` | ドメインイベント収集・ディスパッチ機能を持つ集約ルート基底。 |
| `IVKDomainEvent` | `interface` | `VK.Blocks.Core.Domain` | ドメインイベントのマーカーインターフェース。発生日時と ID を保持。 |
| `IVKSpecification<T>` | `interface` | `VK.Blocks.Core.Specifications` | インメモリ式木評価を可能にする仕様パターンインターフェース。 |
| `IVKReadRepository<T, TId>` | `interface` | `VK.Blocks.Core.Repositories` | 読み取り専用リポジトリポート。 |
| `IVKWriteRepository<T, TId>` | `interface` | `VK.Blocks.Core.Repositories` | 更新・永続化専用リポジトリポート。 |

### 2.5 パイプライン・判定エンジン・基盤抽象 (`Pipeline/`, `Filtering/`, `Guids/`, `Sync/`)

| 公開シンボル (Symbol) | 種別 | 名前空間 | 責務と利用規約 |
| :--- | :---: | :--- | :--- |
| `IVKPipeline` | `interface` | `VK.Blocks.Core.Pipeline` | 多段階ワークフロー実行パイプラインの中核ポート。 |
| `VKPipelineExecutorBase<TContext, TResult>` | `abstract class` | `VK.Blocks.Core.Pipeline` | 前後ミドルウェア、ステージ実行、キャンセル伝播を制御する基底クラス。 |
| `IVKEntryFilter<TItem, TContext>` | `interface` | `VK.Blocks.Core.Filtering` | アイテムの合否を非同期判定するエントリーフィルタープロトコル。 |
| `VKFilterVerdict` | `sealed record` | `VK.Blocks.Core.Filtering` | 判定結果（`IsAccepted`）、理由コード、スコアを保持する不変値。 |
| `IVKGuidGenerator` | `interface` | `VK.Blocks.Core.Guids` | 順序付き GUID（COMB / UUIDv7）および決定論的 GUID 生成ポート。 |
| `ISyncStateStore` | `interface` | `...Core.Synchronization` | メタデータ同期指紋（FNV-1a ハッシュ）の管理ポート。 |
| `IVKDistributedLockProvider` | `interface` | `...Core.Synchronization` | 分散ロック取得・解放プロトコル。 |
| `IVKJsonSerializer` | `interface` | `...Core.Serialization` | 高速 JSON シリアライズ/デシリアライズ抽象。 |
| `VKGuard` | `static class` | `VK.Blocks.Core.Guards` | 事前条件・境界値表明用の防御的ヘルパークラス。 |

### 2.6 コード生成属性メタデータ (`Attributes/`)

| 公開シンボル (Symbol) | 種別 | 責務と利用規約 |
| :--- | :---: | :--- |
| `[VKBlockMarker]` | `Attribute` | BuildingBlock ルートクラスに付与し、Source Generator によるマーカー生成をトリガー。 |
| `[VKFeature]` | `Attribute` | サブ機能スライスを宣言し、親ブロックへのプルアップ登録を自動化。 |
| `[VKOptions]` | `Attribute` | 動的 Args 生成設定（`ArgsMode`, `GenerateMerge`）を集約する属性（`ADR-020`）。 |
| `[VKRequestOverride]` | `Attribute` | 動的上書きを許容する Options プロパティをマーク。 |
| `[VKNoRequestOverride]` | `Attribute` | 動的上書きから明示的に除外する Options プロパティをマーク。 |
| `[VKStronglyTypedId]` | `Attribute` | 強タイプ ID 構造体のコード生成をトリガー。 |
| `[VKSensitiveData]` | `Attribute` | PII・秘匿性フィールドをマークし、ログマスキングを指示（`OR.02`）。 |

---

## 3. 🔒 内部実装のカプセル化 (Internal Boundaries — `AP.03`)

`AP.03` に従い、以下の型はすべて `internal sealed` として隔離されており、`VK` プレフィックスを持ちません：

```csharp
// ❌ 禁止: 下流アセンブリでの直接参照
// internal class InnerCache<T>
// internal class DefaultSequentialGuidGenerator : IVKGuidGenerator
// internal class NoOpSyncStateStore : ISyncStateStore
// internal class DefaultJsonSerializer : IVKJsonSerializer
```

- **`Internal/` ディレクトリ配下**: すべてのアセンブリ境界で隠蔽され、`InternalsVisibleTo` はテストプロジェクトのみに限定されます。
- **DI 解決**: 外部利用者は必ず `IVKGuidGenerator` や `ISyncStateStore` などの **公開ポート（Level 1）** 経由でサービスを解決します。

---

## 4. ⚖️ SemVer 運用と破壊的変更 (Breaking Changes) 基準 (`FS-09`, `FS-02`)

### 4.1 破壊的変更（Major バージョン昇格必須・要 ADR）
- **公開型・メソッドのシグネチャ変更**: `VKResult<T>` や `VKError` のパブリックプロパティの削除・リネーム。
- **エラーコード文字列定数の改変 (`FS-02`)**: `VKCoreErrors.*` の定数値の変更（下流の ProblemDetails マッピングおよびフロントエンド Zod バリデーションを破壊するため禁止）。
- **`IVKBlockMarker` / `IVKBlockOptions` 規約の変更**: `SectionName` 解決プロトコルの変更。
- **`VKTenantId` のメモリレイアウトや等価性比較挙動の変更**。

### 4.2 後方互換変更（Minor / Patch）
- 新しいエラーコード定数の追加（`VKCoreErrors` へのフィールド追加）。
- `VKGuard` への新しい防御的表明メソッドの追加。
- `VKResult` への新しい流暢な ROP 拡張メソッド（例: `.Ensure(...)`, `.Map(...)`）の追加。
- `InnerCache` や `VKValueStringBuilder` の内部アルゴリズム改善によるメモリ削減。

---

## 5. ⏳ 非推奨化（Deprecation）プロセス (`DL.03`)

公開 API を廃止する場合は、以下の手順を厳格に遵守します：

1. **ADR の策定 (`DL.03`)**: 廃止理由、代替 API、および移行手順を記録。
2. **`[Obsolete]` 属性の付与**:
   ```csharp
   [Obsolete("Use VKResult<T>.Success(data) instead. Scheduled for removal in v2.0. (ADR-CORE-013)", false)]
   public static Result<T> Success<T>(T value) => ...;
   ```
3. **1 以上のマイナーバージョン維持**: 即座の物理削除は行わず、最低 1 メジャーリリース期間を移行猶予期間として確保。

