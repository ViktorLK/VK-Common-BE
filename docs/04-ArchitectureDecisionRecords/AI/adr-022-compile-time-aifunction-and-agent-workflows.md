# ADR-022: Compile-Time AIFunction Generation and Multi-Agent Workflows

- **Status**: Accepted
- **Date**: 2026-11-11
- **Area**: AI / Tooling & Agent Workflow Architecture
- **Related ADRs**: [ADR-019 (MEAI Migration)](/docs/04-ArchitectureDecisionRecords/AI/adr-019-migration-from-semantic-kernel-to-meai-and-maf.md), [ADR-020 (Pure Isolation of Ports)](/docs/04-ArchitectureDecisionRecords/AI/adr-020-pure-isolation-of-vk-blocks-ai-core-ports.md), [ADR-021 (Deterministic MEAI Middleware Pipeline)](/docs/04-ArchitectureDecisionRecords/AI/adr-021-deterministic-meai-middleware-pipeline.md)

---

## Context

AI モデルおよび自律型マルチエージェント（Microsoft Agent Framework / MAF）におけるツール（Function Calling / Tools）の定義と実行では、以下の重大な課題が存在していた：

1. **リフレクションによる実行時オーバーヘッドとメモリ割当 (AP.01 違反)**:
   従来のツール実行機構では、リフレクションを用いて引数を動的にバインド・呼び出していたため、高頻度なツール呼び出しにおいて GC 負荷と実行時例外のリスクが増大していた。
2. **コンテキスト伝播の断絶 (ARCH-09 違反)**:
   非同期ツール実行ループにおいて、呼び出し元（エージェント）の `VKAgentExecutionContext` がツール実行境界を越えて伝播せず、トレースや変数共有が困難であった。
3. **ワークフロー制御の決定論性**:
   エージェント間の協調において、場当たり的なループ実行ではなく、明示的なワークフロー協調（`Microsoft.Agents.AI.Workflows`）およびスレッド状態の永続化分離（`IVKAgentThreadStore`）が不可欠となった。

## Options Considered

1. **Option 1: ランタイムリフレクション + 動的 DynamicMethod デリゲート生成**
   - Pros: ソースジェネレーターが不要。
   - Cons: AOT コンパイル（Native AOT）との非互換、起動時コスト、型制約のコンパイル時検証不能（`AP.01`, `BASE-03` に反する）。

2. **Option 2: Roslyn Source Generator による静的 `IVKAtomicTool` 生成 + 明示的コンテキスト伝播 (採用)**
   - Pros:
     - `[VKAITool]` および `[VKAIToolParameter]` 属性から、完全ゼロリフレクションの `IVKAtomicTool` 実装クラスをコンパイル時に生成。
     - 対象クラスのアクセシビリティ（`internal`/`public`）を継承し、`AP.03` に適合。
     - `CancellationToken` および `VKAgentExecutionContext` 引数は JSON Schema から除外した上で、実行時に明示的に注入・バインド。
     - `System.Text.Json.JsonElement` を含むゼロリフレクション型変換（`ConvertArg<T>`）を静的コードとして生成。
     - 静的グローバル（`AsyncLocal`）を排し、`VKAtomicToolAdapter.ToAIFunction(tool, context)` による明示的コンテキスト伝播（`BASE-02`, `ARCH-09`）。
     - `IVKAgentThreadStore` による状態ストレージの契約分離（ARCH-05 / AP.07）。
     - `DefaultAgentGroupRunner` において `AgentWorkflowBuilder` + `InProcessExecution.RunAsync` による実際の MAF ワークフロー駆動。
   - Cons:
     - Source Generator のビルドパイプライン統合が必要。

## Decision & Rationale

**Option 2 を採用する。**  
VK.Blocks の基本原則である `BASE-01`（Contract-First）、`BASE-02`（Zero Implicit Globals）、`BASE-03`（Compile-Time First）、`AP.01`（Zero Reflection）、`ARCH-09`（Boundary-Established Context）に準拠し、以下のアーキテクチャを確立する：

1. **Compile-Time Tool Generation**:
   - `VKAIToolGenerator` が `[VKAITool]` を付与されたクラス・メソッドから `IVKAtomicTool` ラッパークラスを自動生成。
   - アクセシビリティ整合、`JsonElement` アンパック、システム引数（`CancellationToken` / `VKAgentExecutionContext`）の明示的受け渡しを静的コードとして出力。
2. **Explicit Context Bridging**:
   - `ToolExecutionContextAccessor` (`AsyncLocal`) を廃止し、`VKAtomicToolAdapter.ToAIFunction(tool, context)` により実行時スコープを安全にバインド。
3. **Agent Workflow & Thread Store**:
   - `IVKAgentThreadStore` ポートを定義し、デフォルトの `InMemoryAgentThreadStore` を同居登録（ARCH-05）。
   - `DefaultAgentGroupRunner` において `Microsoft.Agents.AI.Workflows` (`InProcessExecution.RunAsync`) による実ワークフロー協調（Sequential, Concurrent）および LLM/RoundRobin オーケストレーションを実現。
   - `ExecutorId` に基づく決定論的な発言者（AuthorName）逆引きと、厳密一致によるエージェント選定。
   - `DefaultClientAgent` においてセッションの `StateBag` 復元時の `JsonElement` 汚染を解消し、マルチターン会話履歴を安全に伝播。

## Consequences & Trade-offs

- **プラスの影響**:
  - ゼロリフレクションによる高い実行パフォーマンスと Native AOT への完全適合。
  - スキーマ不整合や引数エラーがコンパイル時および静的型付の段階で検出可能。
  - エージェントとツールの非同期実行間でトレースと状態が完全に同期。
- **マイナスの影響 / リスク**:
  - ツールクラスの引数型にカスタムオブジェクトを使用する場合、シリアライザーまたは型コンバーターの定義が必要。

