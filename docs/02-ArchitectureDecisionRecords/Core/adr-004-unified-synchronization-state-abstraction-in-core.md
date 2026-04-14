# ADR 004: Unified Synchronization State Abstraction in Core

**Date**: 2026-04-08  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: VK.Blocks.Core - Infrastructure Consistency

## 1. Context (背景)

VK.Blocks の各モジュール（Authorization, Navigation, Audit 等）は、コードで定義されたメタデータをデータベースに同期する機能を備えています。起動パフォーマンスを最適化するためにハッシュベースの同期（ADR-008 参照）を採用していますが、ハッシュ値をどこに、どのように永続化するかという問題が発生しました。各モジュールが個別にストレージ接口（Interface）を定義すると、ユーザーがインフラ層を実装する際に重複が発生し、また統一的な「メタデータ管理テーブル」を構築することが困難になります。

## 2. Problem Statement (問題定義)

同期の状態（指紋/バージョン）を管理する仕組みがモジュールごとに断片化しており、ボイラープレートコードが増加している。また、インフラ依存を排除しつつ、全モジュールで一貫した「指紋チェック」フローを確立するための共通の抽象化が不足しています。

## 3. Decision (決定事項)

共通の同期状態管理コントラクトとして `ISyncStateStore` を `VK.Blocks.Core` に導入します。

1. **インターフェースの標準化**：
   `ISyncStateStore` は特定のビジネスエンティティではなく、「キー（Key）」と「ハッシュ（Hash）」のペアによる永続化を抽象化します。
   - `GetLastHashAsync(string key, ...)`: 指定されたキーの最新ハッシュを取得。
   - `UpdateHashAsync(string key, string hash, ...)`: 同期成功後に新しいハッシュを永続化。
2. **デフォルト保底実装の提供**：
   `NoOpSyncStateStore`（何もしない、常に最新ハッシュなしを返す）をライブラリ内に提供し、DI 時に `TryAddSingleton` で登録します。これにより、ユーザーが明示的にストレージを実装しなくても、システムは「毎回全同期」という安全なデフォルト動作を維持できます。
3. **拡張性重視の API**：
   各モジュールの DI エクステンションにおいて、`.WithSyncStateStore<TImplementation>()` という流式 API を提供し、ユーザーが単一の DB テーブル（例：`Sys_Metadata`）で全モジュールの指紋を一括管理できるようにします。

## 4. Alternatives Considered (代替案の検討)

### Option 1: Module-Specific State (モジュール別管理)
- **Approach**: 各モジュールが `IPermissionSyncStateStore` のように個別のインターフェースを持つ。
- **Rejected Reason**: ユーザーが DB 実装を書く際に、同じようなコードを何度も書く必要があり、開発体験 (DX) が著しく損なわれる。

### Option 2: Use Distributed Cache (Redis 活用)
- **Approach**: `IDistributedCache` をそのまま使用する。
- **Rejected Reason**: キャッシュは「揮発性」であり、永続的な「システムの状態」を管理するには不向き。キャッシュパージにより意図しないタイミングで全同期が走り、スパイクが発生する可能性がある。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive (メリット)
- **エコシステムの一貫性**：新しい Building Block を追加する際、同期状態の管理方法に悩む必要がなくなる。
- **実装の集約**：ユーザーは 1 つの `ISyncStateStore` を実装するだけで、認可・メニュー・設定などすべての同期指纹の保存先をコントロールできる。

### Negative (デメリット)
- **Core への依存増加**：すべての同期を伴うモジュールが Core の抽象化に依存する。
- **緩和策**: `VK.Blocks.Core` は Zero-Dependency を維持しており、この抽象化は極めて軽量（軽量なインターフェース 1 つとクラス 1 つ）であるため、悪影響は限定的。

---
**Last Updated**: 2026-04-08  
