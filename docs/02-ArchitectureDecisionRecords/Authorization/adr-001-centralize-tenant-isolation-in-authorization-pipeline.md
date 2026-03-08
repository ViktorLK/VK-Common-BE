# ADR 001: Centralize Tenant Isolation in Authorization Pipeline

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Authorization Module

## Context (背景)

マルチテナント（Multi-tenant）アプリケーションにおいて、認可チェック時に `TenantId` の検証を怠ることは、テナント間の致命的なデータ漏洩（Cross-Tenant Data Leakage）に直結します。
ビジネスロジックのハンドラーごとに、開発者が手動で `TenantId` を渡し、コンテキストと一致しているかを毎回検証するアプローチに依存することは、人為的エラーを生む温床となります。

## Problem Statement (問題定義)

- **人間への依存と漏洩リスク**: コントローラーややミドルウェアなどで一時的にバリデーションを忘れた場合、直ちに別のテナントのデータにアクセス可能になってしまう。
- **認可境界（Authorization Boundary）の欠如**: アプリケーションロジックに到達する前に、確実に「現在のリクエストが正しいテナントコンテキストで実行されているか」をブロックする一元化された安全網（Safety Net）が存在しない。

## Decision (決定事項)

ASP.NET Core の Authorization（認可）パイプライン内に、一元化されたテナント分離の仕組みとして `TenantAuthorizationHandler` を導入することを決定しました。

このハンドラーは、すべての認可リクエスト（Authorization requests）をインターセプトし、JWT などのクレーム（Claims）に存在する `TenantId` と、現在のシステム/リクエストのテナントコンテキストが厳密に一致するかを検証します。
テナント情報の不一致や欠落が検出された場合、アプリケーション全体の実行を直ちにブロックし、認可失敗（Authorization failed）として処理します。

## Alternatives Considered (代替案の検討)

### Option 1: Global Query Filters in EF Core

- **Approach**: データベース層（EF Core の Global Query Filters）でのみ `TenantId` によるフィルタリングを強制する。
- **Rejected Reason**: DB操作レベルでの分離は非常に強力ですが、APIのエンドポイント自体へのアクセス（DBを触らない操作や外部API呼び出しなど）は保護されません。DB層の保護と併用すべきですが、認可の代替としては不十分です。

### Option 2: Custom Middleware

- **Approach**: 独自の ASP.NET Core Middleware を作成し、JWTから `TenantId` を抜き出して検証する。
- **Rejected Reason**: Middleware では、特定のエンドポイントに `[AllowAnonymous]` がついているかなどの「ルーティングにおける認可メタデータ」との統合が難しく、不要なエンドポイントまでブロックしてしまう、あるいは必要な検証をすり抜ける可能性があります。標準の Authorization Pipeline を利用する方が堅牢です。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - ゼロトラスト（Zero-trust）境界を Authorization レイヤーで確立し、後続のアプリケーションロジックに依存せずにテナント間アクセスを完璧に遮断できます。
    - 開発者はビジネスロジック内で個別のテナント検証コードを書く必要がなくなり、DRYの原則に従います。
- **Negative**:
    - パイプライン内で毎回 `TenantId` の検証処理が走るため、極僅かなオーバーヘッドが発生します。
- **Mitigation**:
    - `TenantId` の取得などにはメモリ割り当てを最小限に抑える効率化を行い、パフォーマンス影響を無視できるレベルに維持します。

## Implementation & Security (実装詳細とセキュリティ考察)

- **実装**: `TenantAuthorizationHandler` は `IVKAuthorizationHandler` インターフェースを実装し、DIコンテナに自動登録されます（`AuthorizationHandlersGenerator` による）。
- **セキュリティ考察**: 攻撃者が不正に操作した JWT を送信した場合でも、署名検証を通過したのちに `TenantId` クレームの存在と一致が保証されるため、改ざん・使い回しに対してもセキュアな設計を確立します。
