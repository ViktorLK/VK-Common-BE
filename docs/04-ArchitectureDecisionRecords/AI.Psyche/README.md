# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.AI.Psyche モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### 核心的データ構造と物理レイアウト (Core Data Structures)

#### [ADR-001: Structured Tapestry and Layout Indexing](./adr-001-structured-tapestry-and-layout-indexing.md)

**Status**: ✅ Accepted  
**概要**: プロンプトのセマンティクス（チャットメッセージ構造）と物理的なレイアウト順序を完全に分離し、物理順序定数 `PromptLayout` を用いて一元ソート管理する設計決定。  
**キーワード**: Prompt Weaving, Prompt Layout, Structured Tapestry

---

### DI 登録と起動モデル (DI & Bootstrap Model)

#### [ADR-002: Modular Chained Builder](./adr-002-modular-chained-builder.md)

**Status**: ✅ Accepted  
**概要**: 各サブ機能（Echo、Knowledge、Persona等）を可插抜にするため、`IVKAIPsycheBuilder` を介した流式（Fluent）チェインビルダー設計を採用。  
**キーワード**: Chained Builder, Fluent API, Feature Registration

---

#### [ADR-003: Zero Infrastructure InMemory Defaults](./adr-003-zero-infrastructure-inmemory-defaults.md)

**Status**: ✅ Accepted  
**概要**: 開発開始の迅速化と安定したCI実行のため、データベースやRedisなどのインフラが無い場合でも自動でスレッドセーフなインメモリストアへフォールバックする起動設計。  
**キーワード**: Zero Infrastructure, InMemory Store, Failback

---

### パラメータ安全性と実行制御 (Safety & Pipeline Execution)

#### [ADR-004: Strict Overrides Contract](./adr-004-strict-overrides-contract.md)

**Status**: ✅ Accepted  
**概要**: リクエストごとに変更可能なプロパティとシステム固定のインフラオプションをインターフェースレベルで厳格に分離し、安全なマージを強制する設計。  
**キーワード**: Strict Overrides, Security Boundary, AP.05

---

#### [ADR-005: Thread Safe Extensible Context](./adr-005-thread-safe-extensible-context.md)

**Status**: ✅ Accepted  
**概要**: 複数の Weaving ステージが並行して実行される中で、スレッド安全に提示文を蓄積し、かつステージ間の疎結合を保つために `System.Threading.Lock` と型キー拡張コンテナを採用する設計。  
**キーワード**: Thread Safety, Extensible Context, System.Threading.Lock

---

#### [ADR-006: Parallel Chunk Weaving Pipeline](./adr-006-parallel-chunk-weaving-pipeline.md)

**Status**: ✅ Accepted  
**概要**: ステージ順序（StageOrder）と並行グループを考慮した `VKWeavingStepRunner` による実行チャンク化、および `Task.WhenAll` による高スループット並行実行・Fail-Fast処理の決定。  
**キーワード**: Parallel Pipeline, Task.WhenAll, Fail-Fast

---

### 各ステージの最適化と防衛策 (Algorithm Optimization & Resiliency)

#### [ADR-007: Compiled Expression Trees Matcher](./adr-007-compiled-expression-trees-matcher.md)

**Status**: ✅ Accepted  
**概要**: 大量の知識エントリ（Knowledge）を高速に評価するため、マッチング条件を式木（Expression Trees）で動的コンパイル・キャッシュし、ReDoS防御のため正規表現に100msのタイムアウトを強制。  
**キーワード**: Expression Trees, ReDoS, Performance Cache

---

#### [ADR-008: Sliding Token Aware Pruning](./adr-008-sliding-token-aware-pruning.md)

**Status**: ✅ Accepted  
**概要**: LLMの入力制限を超過してAPIエラーになるのを防ぐため、会話履歴数およびToken budgetを考虑し、古い会話から順次退避させていくスライディング修剪アルゴリズム。  
**キーワード**: Token Aware Pruning, Dialogue Eviction, API Resiliency

---

#### [ADR-009: Dynamic XML Wrapper Tagging in Knowledge Injection](./adr-009-dynamic-xml-wrapper-tagging-in-knowledge-injection.md)

**Status**: ✅ Accepted  
**概要**: 知識段落のフォーマットにおいて、暗黙のルールによる固定タグの出力を廃止し、Entryごとのカスタムタグ属性値を動的にXML包装器として利用できる設計。  
**キーワード**: Dynamic Tagging, XML Wrappers, Custom Layout

---

#### [ADR-010: Dynamic Lifecycle Management with Sticky Cooldown and Delay Presets](./adr-010-dynamic-lifecycle-management-with-sticky-cooldown-and-delay-presets.md)

**Status**: ✅ Accepted  
**概要**: 知識トリガー後の生存・待機・遅延時間をマジックナンバーではなく、対話リズムに適したTシャツサイズ型（Sticky, Cooldown, Delay）の定数で一律表现・制御する設計。  
**キーワード**: Knowledge Lifecycle, Conversation Pacing, Custom Constants

---

#### [ADR-011: Dynamic Prompt Fragment Token Replacement Task with Injection Shielding](./adr-011-dynamic-prompt-fragment-token-replacement-task-with-injection-shielding.md)

**Status**: ✅ Accepted  
**概要**: プロンプトテンプレート内の変数を置換する際、ユーザー入力由来の `Echo` ティアを明示的に除外することで、変数書き換え攻撃（Prompt Variable Injection）を遮断する設計。  
**キーワード**: Variable Replacement, Prompt Injection, Security Shield

---

#### [ADR-012: Dynamic Regex Pattern Matching Stage in Prompt Assembly](./adr-012-dynamic-regex-pattern-matching-stage-in-prompt-assembly.md)

**Status**: ✅ Accepted  
**概要**: 静的な変数置換だけでは賄えない、正規表現パターンに基づくテキストの正規化や動的置換を安全（100ms タイムアウト強制）かつ動的に適用できる `Pattern` 特性の統合。  
**キーワード**: Regex Patterns, Prompt Formatting, ReDoS Prevention

---

#### [ADR-013: Dynamic Prompt Segment and Absolute Relative Layout Positioning](./adr-013-dynamic-prompt-segment-and-absolute-relative-layout-positioning.md)

**Status**: ✅ Accepted  
**概要**: 提示文の固定レイアウト（物理权重順ソート）を廃止し、AbsoluteDepth（絶対Messageインデックス）やRelativeDepth（相対的な前後関係）、同順優先度（Priority 0-999）による高度な動的座標解決を可能にする設計。  
**キーワード**: Prompt Segment, Dynamic Layout, Relative Positioning

---

#### [ADR-014: Standardization of Memory Store Boundary Validation and Behaviors Pipeline](./adr-014-standardization-of-memory-store-boundary-validation-and-behaviors-pipeline.md)

**Status**: ✅ Accepted  
**概要**: 各 Feature が持つインメモリデータストアの引数検証処理から例外スローを完全に排除して `VKGuard` 防御へ統一し、さらに Onion-Middleware パターンの Behaviors Pipeline 実行フローと仕様を標準ドキュメントとして合意する設計。  
**キーワード**: Memory Stores, VKGuard Boundary Defence, Behaviors Pipeline, Onion Middleware

---

#### [ADR-015: Migration of Prompt Assembly Pipeline to Core Pipeline Abstraction](./adr-015-migration-of-prompt-assembly-pipeline-to-core-pipeline-abstraction.md)

**Status**: ✅ Accepted  
**概要**: モジュール固有で実装されていた Weaving 実行順序と並行グループ制御ランタイムを廃止し、`Core` 横断基盤の `VKPipelineExecutorBase` 抽象クラスによる標準パイプライン実行モデルへ移行・統合する設計。  
**キーワード**: Pipeline Refactoring, VKPipelineExecutorBase, DRY, Execution Runtime

---

## 🎯 ADR の読み方ガイド

### 新しい開発者向けのロードマップ

1. **コアコンセプトの理解**: まず最初に [ADR-001: Structured Tapestry](./adr-001-structured-tapestry-and-layout-indexing.md) を読み、どのようなモデルで提示文が構築されるかを理解してください。
2. **実行制御の理解**: 次に [ADR-005](./adr-005-thread-safe-extensible-context.md) と [ADR-006](./adr-006-parallel-chunk-weaving-pipeline.md) を読み、並行に動くステージの挙動を追跡します。さらに [ADR-015](./adr-015-migration-of-prompt-assembly-pipeline-to-core-pipeline-abstraction.md) を読み、Core共通抽象のパイプライン実行機構への移行経緯を学びます。
3. **安全とパフォーマンスへの考慮**: 最後に [ADR-007](./adr-007-compiled-expression-trees-matcher.md)、[ADR-008](./adr-008-sliding-token-aware-pruning.md)、[ADR-009](./adr-009-dynamic-xml-wrapper-tagging-in-knowledge-injection.md)、[ADR-010](./adr-010-dynamic-lifecycle-management-with-sticky-cooldown-and-delay-presets.md)、[ADR-011](./adr-011-dynamic-prompt-fragment-token-replacement-task-with-injection-shielding.md)、[ADR-012](./adr-012-dynamic-regex-pattern-matching-stage-in-prompt-assembly.md)、およびストア防衛チェックを統一した [ADR-014](./adr-014-standardization-of-memory-store-boundary-validation-and-behaviors-pipeline.md) で、いかに高負荷と悪意ある入力からサーバーと可用性を守り、表現の柔軟性と安全な対話制御を行うかを学んでください。
4. **高度なレイアウト制御の理解**: [ADR-013](./adr-013-dynamic-prompt-segment-and-absolute-relative-layout-positioning.md) を読み、新旧 of レイアウト解決（絶対/相対座標と優先度）の違いを理解してください。
5. **パイプラインとストア防御設計の高度化**: [ADR-014](./adr-014-standardization-of-memory-store-boundary-validation-and-behaviors-pipeline.md) の Onion-Middleware (Behaviors Pipeline) 設計方針と、引数検証の標準化ルールを理解します。

## 🔗 関連ドキュメント

- [AI.Psyche Module Manifest (Layer 3)](/src/BuildingBlocks/AI.Psyche/README.md)

**Last Updated**: 2026-06-22  
**Total ADRs**: 15
