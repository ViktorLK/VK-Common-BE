# ADR 010: Decoupling Multi-Tenancy from Auditing Infrastructure

## 1. Meta Data

- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Multi-Tenancy Isolation & EFCore Persistence Architecture Audit

## 2. Context (背景)

本決定は、`src/BuildingBlocks/Persistence/EFCore` および `src/BuildingBlocks/MultiTenancy` モジュールのアーキテクチャ監査における「関心事の分離（Separation of Concerns）」および「インターフェース分離の原則（ISP）」の違反指摘を背景としています。
これまで、`IAuditProvider` インターフェースが `CurrentUserId` と `TenantId` の両方を提供する役割を負っており、テナント解決機能が純粋な監査インフラストラクチャに密結合（Coupling）されていました。

## 3. Problem Statement (問題定義)

既存の設計には以下の問題がありました：

1. **Abstraction Leak (抽象漏れ)**: Auditing（監査機能）がOFFの場合でも、Multi-Tenancyを使いたいアプリケーション（Web APIやBackground Workers）は、不要な `IAuditProvider` の実装をDIに登録しなければならず、クリーンアーキテクチャの原則に違反していました。
2. **God Interface**: `IAuditProvider` が監査情報（誰がいつ変更したか）とテナント情報（どのテナントのデータか）の両方の責任を持ちすぎており、単一責任の原則（SRP）に反していました。
3. **Interceptor Coupling**: EF CoreのInterceptorでテナントIDによるフィルタリングや自動設定を行う処理が、監査用のInterceptorと入り組んでおり、テストや保守性が低下していました。

## 4. Decision (決定事項)

**Multi-Tenancy（マルチテナント機能）を Auditing（監査機能）から完全分離し、専用のモジュールと抽象を確立します。**

具体的には：

1. **専用の抽象インターフェース `ITenantProvider` の新設**:
   `TenantId` の提供のみを専門とする `ITenantProvider` を `VK.Blocks.MultiTenancy` モジュールに作成します。
2. **専用の EF Core インターセプター `TenantInterceptor` の導入**:
   テナント自動付与などのロジックを `AuditingInterceptor` から切り離し、専用の `TenantInterceptor` で処理します。
3. **完全オプトイン（Opt-In）化**:
   `PersistenceOptions` に `EnableMultiTenancy` フラグを追加し、Multi-Tenancy, Auditing, SoftDelete の3つの機能を独立してオン/オフ可能（必要な機能のInterceptorだけをDI登録）とするアーキテクチャに改修します。

**設計詳細（ITenantProvider）**:

```csharp
namespace VK.Blocks.MultiTenancy.Abstractions;

public interface ITenantProvider
{
    string? CurrentTenantId { get; }
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: IAuditProvider 内に TenantId を残しつつ独立させる**
    - **Approach**: `IAuditProvider` のサブインターフェースとして `ITenantAuditProvider` などを定義し、必要に応じてキャストして利用する。
    - **Rejected Reason**: キャスト自体が安全ではなく、監査（Audit）とテナント（Tenant）は本質的に異なるドメインの関心事であるため、物理的にも論理的にも別モジュールに分離すべきと判断しました。

- **Option 2: DbContext レベルで直接 HttpContextAccessor を Injection する**
    - **Approach**: インターセプターを介さず、DbContext自体が `IHttpContextAccessor` を受け取って TenantId を解決する。
    - **Rejected Reason**: DB永続化層がWebインフラ（HttpContext）に依存してしまうことになり、Clean Architecture に致命的に違反するため却下しました。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive (メリット)**:
    - クリーンな Separation of Concerns が実現され、各モジュールの再利用性（Auditingがいらないバッチ処理でもMultiTenancyが使える等）が向上しました。
    - `PersistenceOptions` のフラグにより、DIコンテナに登録される不要なサービスやInterceptorが削減され、パフォーマンス微増と可読性の向上が期待できます。
- **Negative (デメリット)**:
    - 既存の `IAuditProvider` に依存して `TenantId` を取得していたコードのマイグレーション（後方互換性の破壊）が発生します。
- **Mitigation (緩和策)**:
    - アプリケーション全体で `IAuditProvider` を呼び出している箇所の呼び出し元を `ITenantProvider` に置換する一括リファクタリングを既に実施し、コンパイルエラーを解消済みです。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ要点**:
    - `TenantInterceptor` は `DbContext` の状態変更前に介入し、エンティティが `IMultiTenant` インターフェースを実装している場合のみ、`ITenantProvider` から取得した `TenantId` を強制的に上書きします。これにより、APIレベルでの意図的な TenantId 改ざんや、クロステナント攻撃（Cross-Tenant Data Leakage）を物理的に防ぐ堅牢なインフラストラクチャが保証されます。

## 8. Implementation References (参考リンク)

- `src/BuildingBlocks/MultiTenancy`: 新設されたマルチテナントモジュール。
- `src/BuildingBlocks/Persistence/EFCore/DependencyInjection/ServiceCollectionExtensions.cs`: 各機能（Tenancy, Auditing, SoftDelete）の独立DI登録ロジック。
- `src/BuildingBlocks/Persistence/EFCore/Interceptors/TenantInterceptor.cs`: テナント設定専用の EF Core インターセプター。
