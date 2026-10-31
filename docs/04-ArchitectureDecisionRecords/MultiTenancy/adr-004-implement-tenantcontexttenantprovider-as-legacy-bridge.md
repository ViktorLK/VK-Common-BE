# ADR 004: Implement TenantContextTenantProvider as Legacy Bridge

## 1. Meta Data

- **Date**: 2026-03-12
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: MultiTenancy Module Refactoring

## 2. Context (背景)

（参照: ADR 001, ADR 003）
MultiTenancy モジュールのリファクタリング（リゾルバパイプラインと `ITenantContext` の導入）以前、システム全体のコンポーネントは `ITenantProvider.GetCurrentTenantId()` インターフェースに直接依存して現在のテナントを取得していた。特に、EFCore の Global Query Filter やその他の一部のインフラストラクチャーは、このインターフェースと密結合している。
基礎となる MultiTenancy モジュールを抜本的に再構築する際、依存する全てのコンシューマを一斉に新しい `ITenantContext` へ書き換えることはリスクが大きく、対象スコープを肥大化させる。

## 3. Problem Statement (問題定義)

新アーキテクチャ（`ITenantContext`）とレガシーアーキテクチャ（`ITenantProvider` に基づくコンシューマ）の間で後方互換性をどのように提供するか。

## 4. Decision (決定事項)

**Bridge / Adapter パターンを導入し、`TenantContextTenantProvider` を `ITenantProvider` の実装として提供する。**

- 新しいアーキテクチャにおいても `ITenantProvider` インターフェースは `Abstractions` 層に維持する。
- 具象実装として、内部で `ITenantContext` に処理を委譲する `TenantContextTenantProvider` を DI に登録する。
- これにより、既存のコンシューマ（例: DbContext）は自分自身のコードを変更することなく、裏側では新しいリゾルバパイプラインとキャッシュ機構の恩恵を享受できる。

```csharp
internal sealed class TenantContextTenantProvider(ITenantContext tenantContext) : ITenantProvider
{
    public string? GetCurrentTenantId() => tenantContext.CurrentTenant?.Id;
}
```

## 5. Alternatives Considered (代替案の検討)

### Option 1: 一斉リファクタリングの実施 (Big Bang)
- **Approach**: `ITenantProvider` を完全に削除し、システム全体の全コンシューマを `ITenantContext` に一斉移行する。
- **Rejected Reason**: 他の BuildingBlocks (EFCore, Authorization など) との広範な統合テストが必要となり、障害のリスクが高まる。部分的な段階的移行（Strangler Fig Pattern の応用）が安全であると判断したため却下。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- 破壊的変更（Breaking Changes）なしで、段階的かつ安全に新しい基盤アーキテクチャのロールアウトが可能。
- 新規開発コードでは直接 `ITenantContext` を使い、レガシーコードでは移行期間中に `ITenantProvider` を使い続けることができる。

### Negative
- 機能的に重複する抽象概念 (`ITenantContext` と `ITenantProvider`) が並存するため、新規開発者に対して「どちらを使うべきか」という不要な認知負荷を与える。

### Mitigation
- `ITenantProvider` の XML コメントにおいて、これが後方互換性のためのブリッジであり、新規の実装では原則として `ITenantContext` に依存すべきであることを明記（非推奨ガイダンスの追加）する。将来のメジャーバージョンでの `ITenantProvider` 廃止をマイルストーンに記録する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- `TenantContextTenantProvider` 自体は `internal sealed class` とし、アセンブリ境界を越えて他のモジュールがこの具象クラスに直接依存したり継承したりするのを防ぐ。
- セキュリティ上のリスクをもたらす決定ではないが、単一の信頼できる情報源（Single Source of Truth）を `ITenantContext` に集中させることで、テナントの不一致バグを防ぐアーキテクチャ上の堅牢化につながる。
