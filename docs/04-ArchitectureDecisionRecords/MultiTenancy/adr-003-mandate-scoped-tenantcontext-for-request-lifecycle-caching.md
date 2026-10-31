# ADR 003: Mandate Scoped TenantContext for Request-Lifecycle Caching

## 1. Meta Data

- **Date**: 2026-03-12
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: MultiTenancy Module Refactoring

## 2. Context (背景)

テナントの解決プロセス（サブドメインの抽出、JWT Claims の解析、さらにはデータストアへのルックアップなど）は計算的・I/O的なコストを伴う可能性がある。リクエストのライフサイクルにおいて、認可パイプライン、データアクセス層（EFCore）、ロギングエンリッチャーなど、複数の異なるコンポーネントが現在の Tenant ID へのアクセスを必要とする。これらのコンポーネントがそれぞれテナント解決パイプラインを再呼び出しした場合、パフォーマンスが著しく低下し、DRY（Don't Repeat Yourself）原則にも反する。

## 3. Problem Statement (問題定義)

- **パフォーマンスのオーバーヘッド**: 複数回にわたるリゾルバの実行（特に DB 検索を伴う場合）は、スループットのボトルネックとなる。
- **不一致のリスク**: リクエストの存続期間中に解決ルールが変動することはないが、各コンポーネントが個別に解決を試みると、コンテキストの不整合が生じる潜在的リスクがある。

## 4. Decision (決定事項)

**Ambient Context パターンを採用し、Scoped `TenantContext` によるテナント情報のリクエスト単位でのキャッシュを強制する。**

- ミドルウェア（`TenantResolutionMiddleware`）において、DI スコープにつき1回だけテナント解決（`TenantResolutionPipeline.ResolveAsync`）を実行する。
- 解決結果を Scope 内で Singleton として振る舞う `TenantContext`（`ITenantContext` の実装）に格納する。
- 下流のすべてのコンポーネントは、解決パイプラインを再実行するのではなく、`ITenantContext` をインジェクト（または非HTTP境界では `TenantContextAccessor` を使用）し、キャッシュされた結果を取得する。

## 5. Alternatives Considered (代替案の検討)

### Option 1: `HttpContext.Items` へのキャッシュ
- **Approach**: 一度解決した `TenantInfo` を `HttpContext.Items["TenantContext"]` に保存し、各コンポーネントがそこから読み取る。
- **Rejected Reason**: 型安全性が失われる（キャストが必要）、およびマジックストリングに依存するため、保守性が低下する。さらに `HttpContext` に強く結合するため、HTTPコンテキストが存在しないバックグラウンドタスクなどへの応用が困難になる。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- 重いテナント解決処理がリクエストごとに最大1回に制限され、全体的なパフォーマンスが最適化される。
- 型安全であり、テストダブル（Mock）を用いた各コンポーネントの単体テストが極めて容易になる。

### Negative
- `ITenantContext` の状態がミドルウェアによって初期化されることに依存するため、ミドルウェアの登録漏れや実行順序の誤りにより、状態が未初期化（Null）になるリスクがある。

### Mitigation
- アプリケーション起動時の DI 登録（`AddMultiTenancy`）とミドルウェア登録（`UseMultiTenancy`）の提供順序を明確にドキュメント化する。未解決状態へのアクセスに対する安全なフォールバック機構（または意図的な例外スロー）を実装する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- `TenantContext` 内のデータは `TenantInfo`（sealed record）として保持するため、リクエスト処理中に誤って上書きされるリスクを防ぐ（不変性の担保）。
- `TenantContext.SetTenant()` メソッドは内部利用に限定する（アクセス修飾子の適切な制御、あるいは契約上の制限）。
