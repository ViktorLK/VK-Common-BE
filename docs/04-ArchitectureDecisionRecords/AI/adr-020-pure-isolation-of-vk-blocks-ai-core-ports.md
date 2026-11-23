# ADR-020: Pure Isolation of VK.Blocks.AI Core Ports (Option B)

- **Status**: Accepted
- **Date**: 2026-11-10
- **Area**: AI / Ports & Core Architecture
- **Related RFC**: [RFC-001 (AI Migration to MEAI & MAF)](/docs/02-RFCs/AI/rfc-001-migration-to-meai-and-agent-framework.md)

---

## Context

AI アーキテクチャの刷新に伴い、中核インターフェース（Ports）を提供する `VK.Blocks.AI` が外部の AI 抽象パッケージ（`Microsoft.Extensions.AI.Abstractions`）に直接依存すべきかどうかの境界設計判断が求められた。

- **案A**: `VK.Blocks.AI` が `Microsoft.Extensions.AI.Abstractions` を参照し、`IChatClient` などをそのまま Core Port として受け入れる。
- **案B**: `VK.Blocks.AI` は完全な中立・純粋性を維持し、一切の外部 AI パッケージを持たず、VK 独自モデル/Result 型による Ports を維持する。

## Options Considered

1. **Option A: MEAI Abstractions への直接依存 (案A)**
   - Pros: 型の二重マッピングが不要で実装コードが極小化する。
   - Cons: `VK.Blocks.Core` や `VKResult<T>` を中心とする VK.Blocks の統一エラーハンドリング（`CS.01`）が侵害され、外部パッケージの型変更に Core が直接影響を受ける。

2. **Option B: 純粋 Ports の隔離 (案B — ユーザー決定)**
   - Pros:
     - `VK.Blocks.AI` は純粋な C# 型と `VKResult<T>` のみで構成され、完全な Port-and-Adapter（`ARCH-02`, `ARCH-03` Core Isolation Test）を満たす。
     - 将来 MEAI に破壊的変更があっても、`VK.Blocks.AI` の契約（`IVKChatEngine` 等）およびコンシューマ側コードは無傷で保護される。
   - Cons:
     - Adapter 境界で `VKChatRequest` ⇄ MEAI `ChatMessage` のデータ変換コードが発生する。

## Decision & Rationale

**Option B（案B）を採用する。**  
VK.Blocks の最高原則である「Core の純粋性（`ARCH-03`）」および「Contract-First / Result パターン遵守（`BASE-01`, `CS.01`）」を貫徹するため、`VK.Blocks.AI` は一切のサードパーティ依存を持たない純粋な Ports 定義に徹する。MEAI や MAF との変換責務は下流のアダプタ層（`VK.Blocks.AI.Extensions`）に封じ込める。

## Consequences & Trade-offs

- **プラスの影響**:
  - `VK.Blocks.AI` は最も高い独立性と再利用性を持ち、いかなるフレームワークの栄枯盛衰にも耐えうる仕様となる。
  - すべてのメソッドが `VKResult<T>` を返し、フォールトトレラントなエラーハンドリングが保証される。
- **マイナスの影響 / リスク**:
  - リクエスト・レスポンス境界でのオブジェクト割り当て（メモリ/GC）の懸念。
  - **緩和策**: マッピング層においてスパンやプールを活用し、ゼロアロケーション（Zero-Allocation）化を進める。

