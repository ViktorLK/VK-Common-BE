# アーキテクチャ監査レポート: Persistence

> Active: [L1+L2:Persistence] | Context: /src/BuildingBlocks/Persistence | Sync: [BB.01:ctx, AP.03:ctx, BB.02:ctx, BB.03:ctx, BB.04:ctx, BB.05:ctx, AP.02:ctx, CS.02:ctx]

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 93/100
- **Fast Audit スコア**: 27/27 (100%)
- **対象レイヤー判定**: Domain Layer — ORM非依存の永続化コントラクト (Pure BCL + Core)
- **総評 (Executive Summary)**: Persistence モジュールは VK.Blocks アーキテクチャの模範的な実装である。ORM非依存のコントラクト設計、Read/Write の明確な分離、`VKResult<T>` による一貫したエラーフロー、`[VKBlockMarker]` SG 駆動の DI 登録、そして網羅的な `VKPersistenceErrors` エラー定数体系を備えている。唯一の指摘事項として、`IVKOutboxStore` の責務境界に関する設計上の TODO がコード内に残存しており、これは将来のリファクタリング候補として認識されている。

---

## Phase 1: 構造監査 (Fast Audit)

> 詳細は Fast Audit 実行結果を参照。全27項目合格。

| カテゴリ | Tier | ✅ | ❌ | ⚠️ |
|:---|:---|:---|:---|:---|
| Structure | 🟡 | 6 | 0 | 0 |
| Marker | 🔴 | 4 | 0 | 0 |
| DI Registration | 🔴 | 7 | 0 | 0 |
| Options | 🟡 | 4 | 0 | 0 |
| Implementation | 🔴 | 3 | 0 | 0 |
| Naming | 🟡 | 3 | 0 | 0 |

---

## Phase 2: DI 登録監査 (Registration Audit)

### Execution Order (BB.03) ✅

`[VKBlockMarker]` SG が生成するコードフローにより、以下のステップが保証される：

| Step | Action | 検証結果 |
|:-----|:-------|:---------|
| 1 | Check-Self (`IsVKBlockRegistered`) | ✅ SG 自動生成 |
| 2 | Options Registration (`AddVKBlockOptions`) | ✅ SG 自動生成 |
| 3 | Mark-Self (`AddVKBlockMarker`) | ✅ SG 自動生成 |
| 4 | Validate Options (`IValidateOptions`) | ✅ SG + カスタム `ValidateBlockCustom` |
| 5 | Feature Toggle (`if (!options.Enabled)`) | ✅ `IVKToggleableBlockOptions` により SG が自動挿入 |
| 6 | Custom Hook (`RegisterBlockCustom`) | ✅ [VKPersistenceBlock.cs](/src/BuildingBlocks/Persistence/VKPersistenceBlock.cs) L14-17 |

### Func Transform (BB.03 / ADR-016) ✅

`VKPersistenceOptions` は `sealed partial record` であり、SG 生成の公開メソッド `AddVKPersistenceBlock` は `Func<VKPersistenceOptions, VKPersistenceOptions> configure` パラメータを使用する。`Action<T>` ではなく関数型変換パターンを採用。

### Enabled Policy Position (BB.03) ✅

`IVKToggleableBlockOptions` を実装しているため、SG が `if (!options.Enabled) return builder;` を **Step 3 (Mark-Self) の後** に自動挿入する。マーカーは常に登録され、サービスのみが無効化される正しいパターン。

### Builder Pattern (BB.03) ✅

- [VKPersistenceBuilderExtensions.cs](/src/BuildingBlocks/Persistence/Common/DependencyInjection/VKPersistenceBuilderExtensions.cs): `IVKPersistenceBuilder` を返すチェーン可能なビルダーパターン
- `OverrideAuditProvider<TProvider>()` メソッドは `builder.WithScoped<VKPersistenceBlock, IVKAuditProvider, TProvider>()` を使用し、AP.06 (Anti-overprotection) に準拠
- `VKGuard.NotNull(builder)` による境界防御

### OptionsValidator Quality (BB.05) ✅

[VKPersistenceBlock.cs](/src/BuildingBlocks/Persistence/VKPersistenceBlock.cs) L19-37 にて `ValidateBlockCustom` を実装：

- `DefaultCommandTimeoutSeconds > 0` ✅
- `DefaultPageSize` の範囲検証 (1 ～ MaxPageSize) ✅
- `MaxPageSize > 0` ✅
- `ConcurrencyRetryCount >= 0` ✅

全ての数値プロパティに対して適切なバリデーションが実装されている。

**Phase 2 判定: PASS ✅**

---

## Phase 3: 実装監査 (Deep Analysis)

### 1. 設計原則 (Design Principles)

#### SRP (単一責任原則) ✅

各インターフェースは明確な単一責任を持つ：

| インターフェース | 責務 |
|:---|:---|
| [IVKReadRepository](/src/BuildingBlocks/Persistence/Repositories/Protocols/IVKReadRepository.cs) | 読み取り専用クエリ (NoTracking) |
| [IVKWriteRepository](/src/BuildingBlocks/Persistence/Repositories/Protocols/IVKWriteRepository.cs) | 書き込み操作 (Add/Update/Delete) |
| [IVKBulkRepository](/src/BuildingBlocks/Persistence/Repositories/Protocols/IVKBulkRepository.cs) | 高性能バッチ操作 |
| [IVKUnitOfWork](/src/BuildingBlocks/Persistence/Common/Protocols/IVKUnitOfWork.cs) | トランザクション管理 + SaveChanges |
| [IVKSaveChangesPipeline](/src/BuildingBlocks/Persistence/Common/Protocols/IVKSaveChangesPipeline.cs) | SaveChanges 前後フック |
| [IVKOutboxStore](/src/BuildingBlocks/Persistence/Common/Protocols/IVKOutboxStore.cs) | Outbox メッセージ永続化 |
| [IVKConcurrencyResolver](/src/BuildingBlocks/Persistence/Common/Protocols/IVKConcurrencyResolver.cs) | 楽観的排他制御 |
| [IVKConnectionChecker](/src/BuildingBlocks/Persistence/Common/Protocols/IVKConnectionChecker.cs) | 接続ヘルスチェック |
| [IVKDatabaseInitializer](/src/BuildingBlocks/Persistence/Common/Protocols/IVKDatabaseInitializer.cs) | マイグレーション・シード |
| [IVKAuditProvider](/src/BuildingBlocks/Persistence/Auditing/Protocols/IVKAuditProvider.cs) | 監査情報取得 |

#### ISP (インターフェース分離原則) ✅

Read と Write を明確に分離。`IVKBaseRepository` は組み合わせ用の合成インターフェースとして提供。`IVKSystemRepository` は テナントフィルタバイパス専用の派生インターフェース。

#### DIP (依存性逆転原則) ✅

`VK.Blocks.Core` にのみ依存。コンクリート ORM への参照は一切なし。

#### KISS / YAGNI ✅

インターフェースは過度な抽象化を避け、実用的な API 面を提供している。`VKQueryOptions` は必要十分なヒント（Tracking、Timeout、QueryFilters、SplitQuery、ReadReplica）を `sealed record` + `init` で定義。

### 2. 設計パターン (Design Patterns)

| パターン | 適用箇所 | 評価 |
|:---|:---|:---|
| **Repository** | `IVKReadRepository`, `IVKWriteRepository`, `IVKBulkRepository` | ✅ ORM非依存の Generic Repository |
| **Unit of Work** | `IVKUnitOfWork` | ✅ トランザクション管理を集約 |
| **Strategy** | `IVKAuditProvider`, `IVKConcurrencyResolver` | ✅ プロバイダ交換可能 |
| **Null Object** | `NoOpAuditProvider` | ✅ 監査無効時の安全なフォールバック |
| **Pipeline** | `IVKSaveChangesPipeline` | ✅ Before/After/OnFailed のフック構造 |
| **Specification** | `IVKSpecification<T>` 引数 | ✅ クエリ条件のカプセル化 |
| **Builder** | `IVKPersistenceBuilder` + Extensions | ✅ Fluent DI 設定 |
| **Outbox** | `IVKOutboxStore` + `VKOutboxMessage` | ✅ At-least-once メッセージ配信保証 |

### 3. アーキテクチャ原則 (Architectural Principles)

#### 関心の分離 ✅

- **Auditing/**: 監査専用のドメインスライス
- **Repositories/**: リポジトリ専用のドメインスライス（Models + Protocols）
- **Transactions/**: トランザクション専用のドメインスライス
- **Common/**: 横断的なプロトコル、DI、Diagnostics

#### カプセル化 ✅

- `Internal/` 配下の型 (`NoOpAuditProvider`, `PersistenceDiagnostics`) はすべて `internal`
- Public API 面はすべて `VK`/`IVK` プレフィックス付き

#### 凝集度 ✅

各ドメインスライス内の型は密接に関連。`Repositories/Models/VKQueryOptions.cs` はリポジトリクエリに直結するモデル。

#### 結合度 ✅

唯一の外部依存は `VK.Blocks.Core` のみ。BCL 型 (`System.Linq.Expressions`, `System.Data.IsolationLevel`) のみを使用。

### 4. アーキテクチャスタイル (Architectural Styles)

#### Clean Architecture 準拠 ✅

Persistence モジュールは **Domain Layer** に位置し、インフラストラクチャへの依存を一切持たない。`csproj` は `Core` プロジェクトのみを参照 ([CS.02] 完全準拠)。

#### Vertical Slice 準拠 ✅

`Auditing/`, `Repositories/`, `Transactions/` の各スライスは独立したドメイン境界を形成し、BB.01 の構造規約に準拠。

### 5. アーキテクチャパターン (Architectural Patterns)

#### CQRS (Command Query Responsibility Segregation) ✅

Read (`IVKReadRepository`) と Write (`IVKWriteRepository`) の完全分離。Read 側は `AsNoTracking` がデフォルト、Write 側は Change Tracker を活用。Tracked Read クエリは `GetTracked*` メソッドとして明示的に分離されている。

### 6. エンタープライズパターン (Enterprise Patterns)

| パターン | 評価 | 備考 |
|:---|:---|:---|
| 冪等性 | ✅ | `[VKBlockMarker]` SG による DI 冪等性保証 |
| 楽観的排他制御 | ✅ | `IVKConcurrencyResolver` + `ConcurrencyRetryCount` 設定 |
| Outbox | ✅ | `IVKOutboxStore` + `VKOutboxMessage` による at-least-once 保証 |
| ヘルスチェック | ✅ | `IVKConnectionChecker` (ASP.NET 非依存) |
| 可観測性 | ✅ | `[VKBlockDiagnostics]` + `VKPersistenceDiagnosticsConstants` (Activity/Meter/Metric) |
| マルチテナント | ✅ | `IVKSystemRepository` による明示的フィルタバイパス + `EnableMultiTenancy` オプション |
| ソフトデリート | ✅ | `DeleteAsync` (ポリシー準拠) vs `HardDeleteAsync` (物理削除) の二段階 API |

### 7. VK.Blocks 固有の準拠度 (VK.Blocks Compliance — Deep)

#### BB.03 実行順序 ✅
SG 自動生成により保証。カスタムフック (`RegisterBlockCustom`) はデフォルト `NoOpAuditProvider` の `TryAddScoped` 登録のみ。

#### ADR-016 Func 変換 ✅
`sealed partial record` + SG `Func<T,T>` パターン。

#### Error 定数パターン (CS.01) ✅
[VKPersistenceErrors.cs](/src/BuildingBlocks/Persistence/Common/VKPersistenceErrors.cs): 全エラーが `static readonly VKError` として分類定義済み。フォーマットは `{ModuleName}.{Category}.{Reason}` に完全準拠。

| カテゴリ | エラー数 |
|:---|:---|
| UnitOfWork | 2 (SaveChangesFailed, ConcurrentUpdate) |
| Repository | 1 (EntityNotFound) |
| Database | 2 (ConnectionFailed, ConstraintViolation) |
| Transaction | 4 (BeginFailed, CommitFailed, AlreadyActive, NoActiveTransaction) |
| Health | 2 (ConnectionUnhealthy, MigrationPending) |

#### CancellationToken 伝播 (CS.03) ✅
全非同期メソッドに `CancellationToken cancellationToken = default` パラメータが一貫して宣言されている。`// [CS.03]` タグも適切に付与。

#### Visibility 整合性 (AP.03) ✅
- Public API: すべて `VK`/`IVK` プレフィックス
- Internal: `NoOpAuditProvider` (Auditing/Internal), `PersistenceDiagnostics` (Diagnostics/Internal)
- 違反なし

#### Core 拡張と基盤抽象の徹底活用 (CS.06) ✅
- [NoOpAuditProvider.cs](/src/BuildingBlocks/Persistence/Auditing/Internal/NoOpAuditProvider.cs): `TimeProvider.GetUtcNow()` を使用。`DateTime.UtcNow` の直接使用なし。`// [CS.06]` タグ付与済み。
- `Guid.NewGuid()` / `JsonSerializer` の直接使用なし。

#### 境界防御 (VKGuard) ✅
[VKPersistenceBuilderExtensions.cs](/src/BuildingBlocks/Persistence/Common/DependencyInjection/VKPersistenceBuilderExtensions.cs) L23: `VKGuard.NotNull(builder)` で境界を防御。

---

## 深度逻辑与状态演进审查 (Deep Logic & State Evolution Audit)

### 実行パス脳内推演 (Mental Execution)

#### 成功パス: SaveChanges フロー

```
App → IVKUnitOfWork.SaveChangesAsync(ct)
  → Provider: IVKSaveChangesPipeline.BeforeSaveAsync(ct)  // Domain Events 収集
  → Provider: DbContext.SaveChangesAsync(ct)               // EF Core 等
  → Provider: IVKSaveChangesPipeline.AfterSaveAsync(ct)   // Events Dispatch
  → Return: VKResult<int>.Success(affectedRows)
```

状態の伝播: `CancellationToken` がチェーン全体を通過 ✅。`VKResult<int>` で行数が呼び出し元に返却 ✅。

#### 失敗パス: Concurrency Conflict

```
App → IVKUnitOfWork.SaveChangesAsync(ct)
  → Provider: DbConcurrencyException 発生
  → IVKConcurrencyResolver.ResolveAndRetryAsync(saveAction, retryCount, ct)
  → retryCount 回リトライ → 最終失敗
  → IVKSaveChangesPipeline.OnSaveFailedAsync(ex, ct)
  → Return: VKResult<int>.Failure(VKPersistenceErrors.UnitOfWork.ConcurrentUpdate)
```

エラー情報は構造化された `VKError` として返却 ✅。例外は境界で捕捉されて `Result` に変換 ✅。

### 「逻辑死胡同」スキャン (Dead Ends)

- ⚠️ **[IVKOutboxStore TODO]**: [IVKOutboxStore.cs](/src/BuildingBlocks/Persistence/Common/Protocols/IVKOutboxStore.cs) L12-14 に `// TODO: Refactor Outbox architecture...` が残存。これは DL.04 に基づきバックログ同期が推奨される。ただし、現在のインターフェースは機能的に完全であり、即座の問題は発生しない。

- ✅ **VKOutboxMessage**: `required` キーワードで `Id`, `EventType`, `Payload`, `OccurredOn` が必須化されており、null 状態での構築は不可能。

### 防御的逆向思考 (Destructive Thinking)

1. **データ損失リスク**: `IVKWriteRepository.DeleteAsync` はソフトデリートポリシーに従い、`HardDeleteAsync` は明示的呼び出しのみ。誤った物理削除のリスクは API 設計レベルで最小化されている。

2. **トランザクション不整合リスク**: `IVKTransaction` は `IDisposable` + `IAsyncDisposable` を実装し、`using` ブロックでの安全な解放を保証。`IVKUnitOfWork<TDbContext>.ExecuteInTransactionAsync` は Execution Strategy パターンを提供し、一時的障害時のリトライを安全に実行。

3. **テナント分離バイパスリスク**: `IVKSystemRepository` が明示的に存在し、通常の `IVKBaseRepository` とは別のインターフェースとして分離されている。誤ったフィルタバイパスのリスクは DI レベルで管理可能。

4. **`IVKReadRepository.GetByIdAsync` の戻り値**: `TEntity?` (nullable) を返すため、CS.01 の `Result<T>` パターンとは異なるが、これは **基盤コントラクト層** での設計判断として妥当。`GetByIdAsync` は低レベルのルックアップであり、上位の Application Layer で `Result<T>` に変換する責務を持つ。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

該当なし。致命的な設計上の問題は検出されなかった。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **ページネーション強制**: `IVKReadRepository` のページネーション API (`GetPagedAsync`, `GetCursorPagedAsync`) は `pageSize` パラメータを明示的に要求し、`VKPersistenceOptions.MaxPageSize` でグローバル上限を設定。メモリ枯渇攻撃に対する防御が設計レベルで組み込まれている。

- 🔒 **NoTracking デフォルト**: `VKPersistenceOptions.DefaultTracking = VKQueryTracking.NoTracking` により、読み取りクエリは Change Tracker を使用しない安全なデフォルト。パフォーマンスとメモリ使用量が最適化されている。

- 🔒 **コマンドタイムアウト**: `DefaultCommandTimeoutSeconds` (デフォルト30秒) により、長時間クエリのハングアップを防止。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **高いテスト容易性**: 全てのコントラクトがインターフェースベースで定義されており、モック化が容易。`NoOpAuditProvider` は `TimeProvider` を DI 経由で受け取り、テスト時の時刻制御が可能。

- ⚙️ **`IVKSpecification<TEntity>` サポート**: Repository メソッドの多くが Specification パターンをサポートし、クエリ条件の単体テストを独立して実行可能。

- ⚙️ **具象クラス依存なし**: モジュール全体で `new` キーワードによる具象インスタンス生成は存在しない（`NoOpAuditProvider` は DI 経由で登録）。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **構造化診断**: `[VKBlockDiagnostics<VKPersistenceBlock>]` によるSG駆動の診断基盤。`VKPersistenceDiagnosticsConstants` で Activity / Meter / Metric 名が一元管理。

- 📡 **Result パターン**: 全ての `IVKUnitOfWork` 操作が `VKResult<T>` を返却し、RFC 7807 準拠のエラーレスポンスへの変換を上位層に委譲。

- 📡 **トレーサビリティ**: `IVKTransaction.TransactionId` (`Guid`) により、分散トレーシングでのトランザクション追跡が可能。`VKQueryOptions.QueryTag` でクエリレベルのタグ付けをサポート。

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **DL.04 TODO 残存**: [IVKOutboxStore.cs](/src/BuildingBlocks/Persistence/Common/Protocols/IVKOutboxStore.cs) L12-14 に Outbox リファクタリングの TODO コメントが存在。DL.04 に基づき、バックログへの同期が推奨される。現時点では機能に影響はないが、責務分離の観点から将来的に `IVKOutboxWriter` (Write) と Messaging ブロック (Read/Process) への分割が計画されている。

- ⚠️ **Diagnostics 名前空間**: [VKPersistenceDiagnosticsConstants.cs](/src/BuildingBlocks/Persistence/Common/Diagnostics/VKPersistenceDiagnosticsConstants.cs) の名前空間が `VK.Blocks.Persistence.Common.Diagnostics` となっており、ライブラリのフラット root 名前空間 (`VK.Blocks.Persistence`) ではない。BB.04 では「`VK` プレフィックス付きの `public static class`」と規定されているが、名前空間の深さについては AP.03 の Public API Surface ルール（`Common/` 配下の public 型は root 名前空間を使用すべき）との整合性に軽微な差異がある。

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **ORM 完全非依存**: `csproj` は `VK.Blocks.Core` のみ参照。EF Core / Redis / Cosmos への参照ゼロ。モジュール Manifest の「Provider Neutrality」原則を完全遵守。

2. **Read/Write 分離の徹底**: `IVKReadRepository` (NoTracking) と `IVKWriteRepository` の分離に加え、`GetTracked*` メソッド群で明示的なトラッキング有効化を提供。CQRS の精神を忠実に実装。

3. **多層ページネーション**: Offset (`GetPagedAsync`) と Cursor (`GetCursorPagedAsync`) の両方をサポートし、ユースケースに応じた最適な選択が可能。Projection 付きオーバーロードも完備。

4. **Soft Delete の二段階 API**: `DeleteAsync` (ポリシー準拠) と `HardDeleteAsync` (強制物理削除) の明確な分離。誤操作防止と運用柔軟性の両立。

5. **包括的エラー定数**: `VKPersistenceErrors` が UnitOfWork / Repository / Database / Transaction / Health の5カテゴリ × 11エラーを `static readonly VKError` で定義。raw string エラー皆無。

6. **NoOpAuditProvider の CS.06 準拠**: `TimeProvider.GetUtcNow()` を使用し、非決定的 API への直接依存を回避。`// [CS.06]` タグによるルール引用。

7. **Specification パターン統合**: `IVKSpecification<TEntity>` を Read / Bulk 双方でサポートし、複雑なクエリ条件のカプセル化とテスト容易性を提供。

8. **IVKPropertySetter のフルエント設計**: バルク更新時のプロパティ設定を Expression ベースで型安全に記述可能。定数値と算出値の両方をサポート。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

なし。致命的な課題は検出されなかった。

### 2. リファクタリング提案 (Refactoring)

1. **Outbox 責務分離 (DL.04)**: `IVKOutboxStore` の Read/Process 操作 (`GetPendingAsync`, `MarkAsProcessedAsync`) を `VK.Blocks.Messaging.Outbox` ブロックに移行し、Persistence 側は `IVKOutboxWriter` (Write only) に簡素化する。TODO コメント (L12-14) に既に方針が記載されている。

2. **VKPersistenceDiagnosticsConstants 名前空間**: `namespace VK.Blocks.Persistence.Common.Diagnostics` → `namespace VK.Blocks.Persistence` への変更を検討。Public 型は AP.03 に基づきフラット root 名前空間を使用するのが望ましい。ただし、これは軽微な変更であり破壊的変更には該当しない。

3. **IVKReadRepository.GetByIdAsync の Result ラッピング検討**: 現在 `TEntity?` (nullable) を返すが、Application Layer との一貫性を考慮し、将来的に `VKResult<TEntity>` への移行を DIM (Default Interface Methods) で段階的に導入する選択肢がある。ただし、低レベル Repository API として nullable が適切な設計判断である場合はこの限りではない。

### 3. 推奨される学習トピック (Learning Suggestions)

1. **Outbox Pattern の発展形**: Debezium / Change Data Capture (CDC) ベースの Outbox 実装と、ポーリング型 (`GetPendingAsync`) との比較研究。
2. **IAsyncEnumerable とバックプレッシャー**: `StreamAsync` メソッドの実装時に gRPC Server Streaming や Channel<T> との統合パターン。

---

## 🚩 監査例外 (Audit Exceptions)

Audit: ✅ 全アーキテクチャ制約に準拠。軽微な改善提案 (DL.04 Outbox TODO, 名前空間) は将来対応として記録。

> Phase 1: 27/27 | Phase 2: PASS | Phase 3 Score: 93/100
