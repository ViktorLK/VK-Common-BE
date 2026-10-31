# ADR 005: Enforce Fail-Fast Tenancy Validation at System Boundary

## 1. Meta Data

- **Date**: 2026-03-12
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: MultiTenancy Module Refactoring

## 2. Context (背景)

マルチテナントのエンタープライズシステムにおいて、テナントコンテキストが不確定または不正な状態で処理を進めることは、致命的なデータ漏洩（Cross-Tenant Data Leakage）、権限認可のバイパス、あるいは意図しないデータ破壊につながる最大のリスクである。不適切なリクエストは、ドメイン層やデータアクセス層に達する前に、速やかにかつ安全に遮断される必要がある。

## 3. Problem Statement (問題定義)

テナントが解決できなかった（Missing / Invalid）リクエストに対して、どのレイヤーでどのようなエラーフォーマットを用いて対処すべきか。アプリケーション層内部のバリデーションや EFCore のクエリフィルターまで到達させて受動的に失敗させるか、それともシステムのエッジで能動的に弾くべきか。

## 4. Decision (決定事項)

**Fail-Fast パターンを採用し、`TenantResolutionMiddleware` をテナンシー検証のセキュリティ境界とする。**

- `MultiTenancyOptions.EnforceTenancy` オプションを導入し、デフォルトで `true` (強制) とする。
- このオプションが有効な場合、パイプラインが有効なテナントを解決できなかった時点で、後続のミドルウェアやコントローラーの処理を一切呼び出さずに即座にリクエストの実行を短絡（Short-Circuit）する。
- HTTP レスポンスとして、標準化された **RFC 7807 (Problem Details)** 形式の HTTP 401 Unauthorized を直接返却する。

## 5. Alternatives Considered (代替案の検討)

### Option 1: 属性ベース (Attribute-based) でのエンドポイント単位での検証
- **Approach**: `[RequireTenant]` のような ActionFilter 属性を作成し、コントローラーのアクションごとに明示的に付与する。
- **Rejected Reason**: 開発者が属性を付与し忘れた場合に、デフォルトでセキュアではなくなる（Fail-Open）。セキュリティにおいては Default Deny / Fail-Safe の原則が優越するため却下。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- システム全体がデフォルトで「テナント漏洩に対して安全（Secure by Default）」になる。
- 不正リクエストのために不要なリソース（DBコネクションなど）が消費されず、システムの可用性と防御力が高まる。
- クライアントに対して、標準化された一貫性のあるエラーメッセージ (Problem Details) が保証される。

### Negative
- ヘルスチェックや、テナントに依存しない一部の共通 API (例: OpenAPI 仕様のダウンロード) など、意図的にテナントなしでのアクセスを許可したい場合に対処が必要となる。

### Mitigation
- アプリケーション全体での `EnforceTenancy` 設定に加え、特定のパスやエンドポイントでテナント検証をバイパスできるホワイトリスト機構（例：`[AllowAnonymousTenant]` のようなメタデータ、またはミドルウェア前の条件分岐）を提供する設計拡張の余地を残す。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- ミドルウェア内での応答は、生 JSON 文字列の手組みを避け、`System.Text.Json` を用いてシリアライズされた `ProblemDetails` オブジェクトを構築する。（※内部エラー詳細の意図しない情報漏洩を防ぎ、Title, Status, Detail に限定した安全な表現）
- VK.Blocks ルールに基づき、構造化ログには必ず `{TraceId}` を含め、応答 JSON にも `traceId` プロパティを含めることで、エッジでの遮断であっても分散トレーシングと可観測性を完全なものにする。
