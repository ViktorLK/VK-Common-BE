# ADR-019: Migration from Semantic Kernel to Microsoft.Extensions.AI and Microsoft Agent Framework

- **Status**: Accepted
- **Date**: 2026-11-10
- **Area**: AI / Engine & Agent Architecture
- **Related RFC**: [RFC-001 (AI Migration to MEAI & MAF)](/docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md)

---

## Context

現行の `VK.Blocks.AI.SemanticKernel` は `Microsoft.SemanticKernel` に全面的に依存しているが、フレームワークの重量化、不要な動的リフレクション（`Scrutor`, `PluginScanner`）、暗黙的な状態管理が Industrial DNA の原則（`AP.01`, `AP.07`, `ARCH-12`）と齟齬をきたしていた。
Microsoft より .NET 標準の AI 基盤抽象として `Microsoft.Extensions.AI (MEAI)` がリリースされ、エージェント協調の基盤として `Microsoft Agent Framework (MAF)` への一本化が進んでいる。

## Options Considered

1. **Option 1: 現行 Semantic Kernel を維持し内部を微修正する**
   - Pros: 破壊的変更がなく、移行コストがゼロ。
   - Cons: 今後の新機能（MAF の Agent ワークフロー等）に追従できず、SK 独自の Filters や Kernel 抽象の重厚さを抱え続けることになる。

2. **Option 2: 全面移行（MEAI + MAF 採用、SK を完全削除）**
   - Pros: 
     - `DelegatingChatClient` による直感的で高速な洋葱型ミドルウェア構成（`ARCH-12`）。
     - Keyed DI に基づくシンプルな接続管理と Source Generator による零反射 Tool 定義（`AP.01`）。
     - MAF による先進的マルチエージェントオーケストレーション（チェックポイント、Workflows）。
   - Cons: 破壊的変更を伴い、既存コードの差し替えが必要。

## Decision & Rationale

**Option 2 を採用する。**  
破壊的変更が許容され、かつ長期的な保守性とアーキテクチャの純粋性を最優先とするため、`VK.Blocks.AI.SemanticKernel` を全廃し、`VK.Blocks.AI.Extensions` (MEAI) および `VK.Blocks.AI.Agents` (MAF) の 2 パッケージへ再編する。

## Consequences & Trade-offs

- **プラスの影響**:
  - `Microsoft.SemanticKernel` 関連の 10 以上のパッケージ参照が削除され、フットプリントが大幅に軽量化。
  - すべての中間処理（監査、セキュリティ、トークン制御、キャッシュ）が標準の `DelegatingChatClient` で一元化される。
- **マイナスの影響 / リスク**:
  - `VK.Blocks.AI.SemanticKernel` を直接参照している箇所にコンパイルエラーが発生する（すべて新パッケージへ誘導）。
  - 音声・画像生成などの一部 MEAI 抽象のステータスに応じ、Adapter 内部で直接プロバイダ SDK を呼び出す過渡的実装が必要となる可能性がある。

