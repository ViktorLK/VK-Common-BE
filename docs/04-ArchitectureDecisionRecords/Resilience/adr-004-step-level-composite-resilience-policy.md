# ADR-004: Step-Level Composite Resilience Policy for AI and Workflow Orchestration

- **Status**: ✅ Accepted
- **Date**: 2026-08-27
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Resilience, src/BuildingBlocks/AI.Cortex, src/BuildingBlocks/Workflow

---

## 1. Context (背景)

`AI.Cortex`（LLM ターンオーケストレーション）および `Workflow`（サガ・ステップ補償オーケストレーション）の高度化に伴い、ステップ単位での宣言的な耐障害性ポリシーの定義と引数受け渡しが日常的なパターンとなった。
LLM 推論呼び出し（Chat Completion）やワークフローステップ実行では、各ステップごとに固有のタイムアウト、最大リトライ回数、および特定のプロバイダー別サーキットブレーカーキーを柔軟に上書き（Override）する必要があった。

## 2. Problem Statement (問題定義)

1. **ステップ単位引数の肥大化**: `ExecuteStepAsync` メソッドが個別の `timeout`, `retryCount`, `circuitBreakerKey` などの多引数を受け取ることになり、メソッドシグネチャが破綻していた。
2. **AI 固有ロジックの混在**: 初期の実装では `VK.Blocks.Resilience` 内に `LocalAIResilienceProvider` や `VKAIFallbackOptions` が存在し、耐障害性ブロックが AI 固有ドメインを知りすぎるドメイン汚染（`ARCH-03` Core/Foundation Purity 違反）が生じていた。
3. **合成実行コードの重複**: `AI.Cortex` と `Workflow` の双方が、リトライ・タイムアウト・熔断を合成する同様のループ制御ロジックを独自に抱え込んでいた。

## 3. Decision (決定事項)

1. **不変レコード `VKStepResiliencePolicy` の導入（`AP.01`）**:
   - `Retry`, `Timeout`, `CircuitBreaker` の各切片設定を束ねるイミュータブルな `sealed record VKStepResiliencePolicy` を定義。
   - `WithRetry()`, `WithTimeout()`, `WithCircuitBreaker()` などの Fluent な `with` 式ヘルパーを提供。
   - `VKStepRetryPolicy` からの暗黙の型変換（Implicit Operator）を用意し、単一リトライ設定との完全な後方互換性を保証。
2. **AI ドメインロジックの `AI.Synapse` への移譲**:
   - `Resilience` から AI 専用プロバイダー（`LocalAIResilienceProvider`）を完全排出し、`AI.Synapse` の `Quota/` および `Resilience/` 切片へ移譲。
3. **統合ステップランナー `VKStepResilienceRunner` の提供**:
   - サーキットブレーカー事前検査 ➔ 単一試行タイムアウト ➔ リトライループ ➔ 成功/失敗記録 を単一の拡張メソッド `ExecuteWithResilienceAsync` にカプセル化。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: DI コンテナからの名前引き（PolicyRegistry）のみに依存**:
  - *Approach*: ステップごとには文字列の名前（Profile 名）のみを渡し、動的パラメータ変更は認めない。
  - *Rejected Reason*: LLM 推論時のリクエスト固有のタイムアウト変更（例: 大規模バッチプロンプト時の一時的延長）やテスト用の上書きに柔軟に対応できないため却下。
- **Option 2: 各オーケストレーターでの個別ループ実装**:
  - *Approach*: `AI.Cortex` と `Workflow` がそれぞれ独自に合成実行ロジックを保守。
  - *Rejected Reason*: 二重実装によるバグの温床となり、耐障害性の統一的なメトリクス収集が阻害されるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - `AI.Cortex`（`DefaultChatTurnOrchestrator`）および `Workflow` のステップ実行コードが大幅に簡素化。
  - AI 固有の不純物が `Resilience` から一掃され、純粋な基盤ブロックへ回帰。
- **Negative / Mitigation**:
  - `VKStepResilienceRunner` 自体に手動のリトライループが含まれていたが、のちに ADR-005 により `IVKResilienceExecutor` 経由で Polly v8 パイプラインを直接呼び出すオーバーロードが追加され、手動ループのリスクは完全に緩和された。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `VKStepResiliencePolicy.Default` および `VKStepResiliencePolicy.None`（即時失敗）の定数を提供し、ゼロアロケーションなデフォルト運用を可能化。
- 各ポリシーのパラメータはコンパイル時安全な `sealed record` で表現され、`VKGuard` による境界検証を徹底。

