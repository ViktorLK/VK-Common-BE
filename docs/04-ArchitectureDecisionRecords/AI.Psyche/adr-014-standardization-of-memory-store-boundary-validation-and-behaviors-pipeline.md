# ADR 014: Standardization of Memory Store Boundary Validation and Behaviors Pipeline

- **Date**: 2026-06-15
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psyche モジュールは、プロンプトの構築、ライフサイクル管理、および実行制御を行う中心的なオーケストレーションライブラリである。これまでの開発において、2 つのアーキテクチャ上の課題が存在していた。第一に、モジュール内の各 Feature に属するメモリ内ストア（`InMemoryEchoStore`, `InMemoryDirectiveStore`, `InMemoryKnowledgeStore`, `InMemoryPersonaStore`, `InMemoryPatternStore`）で、引数の検証に `throw new ArgumentException` が混在していた点である。これは `AP.01` および `CS.01`（境界防御の統一、例外スローの禁止）に違反していた。第二に、Behaviors Pipeline の実行フローおよび Onion-Middleware チェーンの仕様がコード上のみに存在し、文書化された明確な設計合意（ADR）として記録されていなかったことである。

## 2. Problem Statement (問題定義)

1. **防御コードの非標準化**: 依然として残存していた `throw new ArgumentException` は、アーキテクチャ基準への準拠度監査（AI.Psyche_20260617.md）において CS.01 違反として検出され、モジュール内の防御的設計の一貫性を損なっていた。
2. **Behaviors パイプライン設計の曖昧性**: 前回の監査から追加された Before/After 各種ステージ（Pipeline Runner）や `IVKPsycheMiddleware` 委譲パターンの接続規則について、どのタイミングで何が実行されるべきか（並列・直列・スレッド安全性）の設計合意が不明瞭であった。

## 3. Decision (決定事項)

AI.Psyche 内の設計整合性と実行パイプラインの標準化を推進するため、以下の変更を適用する。

1. **`VKGuard` への防御コード統一**:
   - `InMemoryEchoStore` 等の全 5 つの InMemory 構造において、例外を直接スローしていた箇所を `VKGuard.NotEmptyGuid` および `VKGuard.NotNull` 等のガードチェックに完全に置き換える。
2. **Behaviors Onion-Style パイプライン仕様の確立**:
   - `IVKPsycheBeforePipelineStage` (事前処理)、`IVKPsycheAfterPipelineStage` (事後処理)、および `IVKPsycheMiddleware` (Onion フィルタチェーン) を用いた実行制御スキームを形式化する。
   - `PsychePipelineRunner` による非同期実行および、スレッドセーフな `VKPsycheContext` を用いた状態共有を標準アーキテクチャとして定義する。

### 変更後の防御コード（InMemoryStore 一例）

```csharp
namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class InMemoryEchoStore : IVKEchoStore
{
    public Task<VKResult<IReadOnlyList<VKEchoTrace>>> GetAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotEmptyGuid(sessionId.Value); // VKGuard による一貫した防御

        if (!_store.TryGetValue(sessionId, out var traces))
        {
            return Task.FromResult(VKResult.Success<IReadOnlyList<VKEchoTrace>>([]));
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKEchoTrace>>(traces.AsReadOnly()));
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: ビジネスロジックとしてのパラメータエラーを Result.Failure で返却する
- **Approach**: 呼び出し元が不適切な ID を渡した場合に、プログラム例外を避けて `Result.Failure(VKBehaviorsErrors.InvalidId)` を返す。
- **Rejected Reason**: ID が空である等の条件は API 境界のコントラクト違反（バグ）であるため、早い段階でフェイルファスト（Fail-Fast）させて修正を促すべきである。したがって、呼出側でのハンドリングを前提とした `Result.Failure` ではなく、`VKGuard` による防御的例外チェックが妥当である。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な監査準拠**: `throw new` が完全に排除されたことで、AI.Psyche は静的ルール監査で 100% 準拠するようになった。
- **実行チェーンの明文化**: Onion Middleware 構造の決定により、セキュリティ監視、ロギング、キャッシュなどの横断的関心事（Cross-Cutting Concerns）をミドルウェアとして安全に差し込むことができる構造が保障された。

### Negative
- 特になし。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `VKPsycheContext` はマルチスレッドで書き込まれる可能性があるため、`VKPsycheEvictedState` の導入や将来的な `ConcurrentDictionary` の適用を視野に入れたスレッドセーフな設計を遵守する。

## 7. Status
✅ Accepted
