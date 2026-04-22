# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.MultiTenancy モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture (コアアーキテクチャ)

#### [ADR-001: Transition to Tenant Resolution Pipeline Architecture](/docs/02-ArchitectureDecisionRecords/MultiTenancy/adr-001-transition-to-tenant-resolution-pipeline-architecture.md)

**Status**: ✅ Accepted  
**概要**: Strategy Pattern と Pipeline を用いた、複数のソース（Header, Claims, Domain等）からの柔軟なテナント解決アーキテクチャへの移行。  
**キーワード**: `Strategy Pattern`, `Pipeline`, `Extensibility`

---

#### [ADR-002: Adopt Custom TenantResolutionResult Instead of Generic Result](/docs/02-ArchitectureDecisionRecords/MultiTenancy/adr-002-adopt-custom-tenantresolutionresult-instead-of-generic-result.md)

**Status**: ❌ Superseded  
**概要**: テナント解決専用の `TenantResolutionResult` の採用決定であったが、VK.Blocks 標準の `Result<T>` エラーハンドリングへの準拠のため廃止・リファクタリング予定。  
**キーワード**: `Result Pattern`, `Error Handling`, `Superseded`

---

#### [ADR-003: Mandate Scoped TenantContext for Request-Lifecycle Caching](/docs/02-ArchitectureDecisionRecords/MultiTenancy/adr-003-mandate-scoped-tenantcontext-for-request-lifecycle-caching.md)

**Status**: ✅ Accepted  
**概要**: 重いテナント解決処理を1リクエストにつき1回に制限するため、Ambient Context として Scoped `TenantContext` を採用。  
**キーワード**: `Ambient Context`, `Performance`, `Scoped Lifetime`

---

#### [ADR-004: Implement TenantContextTenantProvider as Legacy Bridge](/docs/02-ArchitectureDecisionRecords/MultiTenancy/adr-004-implement-tenantcontexttenantprovider-as-legacy-bridge.md)

**Status**: ✅ Accepted  
**概要**: EFCoreなど既存のコンポーネントが依存する `ITenantProvider` への後方互換性を提供するため、新基盤へ委譲する Bridge パターンを導入。  
**キーワード**: `Bridge Pattern`, `Backward Compatibility`, `Strangler Fig`

---

### Security (セキュリティおよび運用堅牢性)

#### [ADR-005: Enforce Fail-Fast Tenancy Validation at System Boundary](/docs/02-ArchitectureDecisionRecords/MultiTenancy/adr-005-enforce-fail-fast-tenancy-validation-at-system-boundary.md)

**Status**: ✅ Accepted  
**概要**: テナント未解決のアクセスをドメイン層到達前に遮断し、エッジにて RFC 7807 (Problem Details) 様式で HTTP 401 を Fail-Fast で返す設計の強制。  
**キーワード**: `Fail-Fast`, `Security`, `RFC 7807`

---

#### [ADR-006: Adopt Environment-Specific Selective Tenant Resolver Registration](/docs/02-ArchitectureDecisionRecords/MultiTenancy/adr-006-adopt-environment-specific-selective-tenant-resolver-registration.md)

**Status**: ✅ Accepted  
**概要**: 開発環境用クエリ文字リゾルバの悪用（テナントスプーフィング）を防ぐため、本番環境の DI 登録から危険なリゾルバを除外する選択的登録。  
**キーワード**: `Security`, `DI Registration`, `Attack Surface`

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとセキュア設計の理解用

1. **ADR-001**: モジュールの根幹となるパイプライン化の設計意図を理解するための最初のドキュメントです。
2. **ADR-005**: セキュリティに直結する必須設定であり、テナントの漏洩を防ぐ「Fail-Safe」の仕組みを理解できます。
3. **ADR-006**: 本番環境での設定ミスが招く重大なリスクとその防御策について解説しています。

### 拡張と後方互換の理解用

1. **ADR-003, ADR-004**: リゾルバの解決結果がどのようにアプリケーション全体に配布・キャッシュされ、また古いアーキテクチャからの以降をどう安全に担保しているかを理解できます。

## 🔗 関連ドキュメント

- [Architectural Audit Report: MultiTenancy Module (2026-03-10)](/docs/04-AuditReports/MultiTenancy/MultiTenancy_20260310.md)

---
**Last Updated**: 2026-03-12
**Total ADRs**: 6
