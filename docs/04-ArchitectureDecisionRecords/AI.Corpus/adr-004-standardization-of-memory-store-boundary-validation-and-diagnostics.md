# ADR 004: Standardization of Memory Store Boundary Validation and Diagnostics

- **Date**: 2026-06-15
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Corpus

## 1. Context (背景)

AI.Corpus モジュールの監査（AI.Corpus_20260617.md）において、境界防御および可観測性の設計面で 2 点の課題が指摘された。第一に、テスト用の `InMemoryKnowledgeInjectionStore` において、入力値の検証に従来の `throw new ArgumentException` パターンが使用されており、VK.Blocks の標準設計（`AP.01` および `CS.01`）で規定されている `VKGuard` 防御および結果パターン（Result Pattern）から逸脱していたこと。第二に、モジュールレベルでの診断情報（Diagnostics）を定義する定数クラスや専用ログ処理クラスが欠落していたことである。

## 2. Problem Statement (問題定義)

1. **例外スローによる制御フローの汚染**: `InMemoryKnowledgeInjectionStore` で例外を直接スローすると、実行パイプラインが予期せず切断され、呼出元で Result 処理による制御が行えなくなる。また、手動の null/empty チェックは冗長であり、静的解析やアーキテクチャ監査（CS.01 違反）の原因となる。
2. **可観測性の不完全さ**: 日常的な動作状況やエラー発生時のトレースにおいて、適切な EventId 定義や Source Generated された構造化ログがなく、ログ収集ツール（OpenTelemetry 等）との連携時に分析効率が低下する。

## 3. Decision (決定事項)

AI.Corpus モジュール全体の境界防御の統一および観測性向上を図るため、以下の変更を適用する。

1. **`VKGuard` を用いた境界防御の徹底化**:
   - `InMemoryKnowledgeInjectionStore` のメソッド引数検証から `throw new` を完全に排除し、`VKGuard.NotEmptyGuid` および `VKGuard.NotNull` による防衛チェックへ統一する。
2. **`AI.Corpus` 向け診断・ログインフラの追加**:
   - `Common/Diagnostics/CorpusDiagnosticsConstants.cs` を新規作成し、イベント ID とセマンティックなトークン定義を一元管理する。
   - `Common/Diagnostics/Internal/CorpusDiagnostics.cs` を追加し、`[VKBlockDiagnostics<VKAICorpusBlock>]` を付与して診断機構に統合する。
   - `Common/Diagnostics/Internal/CorpusLog.cs` を作成し、Source Generator を用いた `[LoggerMessage]` パターンによる構造化ログを実装する。

### 変更後の構造

```csharp
namespace VK.Blocks.AI.Corpus.Tracking.Internal;

internal sealed class InMemoryKnowledgeInjectionStore : IVKKnowledgeInjectionStore
{
    public Task<VKResult> LogAsync(VKKnowledgeInjection injection, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(injection); // VKGuard による防衛
        // ... 
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: ArgumentException の代わりに Result.Failure を返却する
- **Approach**: 呼び出し元に `Result.Failure` を返してエラーハンドリングさせる。
- **Rejected Reason**: 本件のチェックは呼び出し元がコントラクトを違反した（引数に Null または EmptyGuid を渡した）場合に適用される防衛であるため、設計契約（Design by Contract）に基づき、直ちに `VKGuard` で例外検知すべきであると判断した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **防御パターンの統一**: `VKGuard` の活用により、InMemory ストアの堅牢性が向上し、静的コード監査ツールとの親和性が最大化された。
- **診断性能の向上**: イベント ID が定数化され、Source Generator ロガーを介した構造化ロギングが強制されるため、パフォーマンスのオーバーヘッドを最小化しつつ明確なトレースログが出力される。

### Negative
- 特になし。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `CorpusDiagnostics` クラスは `internal` として定义し、名前空間ルール（`AP.03`）に従い `VK` プレフィックスを排除して `VK.Blocks.AI.Corpus` 内部のカプセル化を維持する。

## 7. Status
✅ Accepted
