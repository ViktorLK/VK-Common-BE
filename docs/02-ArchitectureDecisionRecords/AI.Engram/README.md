# Architecture Decision Records (ADR) - AI.Engram Index

このディレクトリには、VK.Blocks.AI.Engram 長期記憶および忘却・統合管理モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Memory Engine Architecture (記憶エンジンアーキテクチャ)

#### [ADR-001: Memory Consolidation, Scoring, and Decay Strategies for Long-Term Dialogue](./adr-001-memory-consolidation-scoring-and-decay-strategies-for-long-term-dialogue.md)

**Status**: ✅ Accepted  
**概要**: 長期対話エージェントにおける記憶（エングラム）のトークンオーバーフローやコンテキスト汚染を防ぐため、重要度スコアリング、時間減衰、要約・統合、および不要記憶の刈り込みを4つの独立した戦略インターフェースでカプセル化する設計。  
**キーワード**: Memory Consolidation, Scoring Strategy, Forgetting Factor, AI.Engram

---

#### [ADR-002: Tiered Chat History Compression and Eviction](./adr-002-tiered-chat-history-compression-and-eviction.md)

**Status**: ✅ Accepted  
**概要**: LLMトークンリミットとAPI料金高騰を回避するため、設定した `L1TokenBudget` に基づく履歴量の自動検出、文脈保護用の `TargetTurns` 生データ維持、不要になった古いFragmentの `Eviction` 退避を行う多階層チャット履歴圧縮アーキテクチャ。  
**キーワード**: Tiered Compression, Token Budget, Memory Eviction, Chat History

---

#### [ADR-003: Asynchronous Strategy-Based Cognitive Memory Compression](./adr-003-asynchronous-strategy-based-cognitive-memory-compression.md)

**Status**: ✅ Accepted  
**概要**: 重いLLM要約生成による呼び出しスレッドのブロッキングと遅延を防ぐため、非同期インメモリジョブキュー (`CompressionJobQueue`) とバックグラウンドホストサービスを導入し、さらに多様な圧縮アプローチ (要約、KeyValue抽出、トピック分割など) を動的に選択可能にする戦略パターン設計。  
**キーワード**: Background Worker, Strategy Pattern, Asynchronous Compression, Budget Config

---

## 🎯 ADR の読み方ガイド

### 長期記憶と認知的忘却の理解用

1. **ADR-001**: 過去の対話や事実を無制限に引きずることなく、どのように選択的に要約・忘却し、LLMコンテキストを最適化しているかを理解するために読んでください。
2. **ADR-002**: L1トークン予算の設定とスライディング方式による過去ログ自動修剪（Eviction）の設計境界を理解するために読んでください。
3. **ADR-003**: なぜ同期的な要約処理がレスポンス遅延を引き起こすのか、それを解決するインメモリタスクキューと、多様な認知圧縮戦略（KeyValueやトピック分割）の切り替え設計を理解するために読んでください。

## 🔗 関連ドキュメント

- [AI.Engram Module Manifest (Layer 3)](/src/BuildingBlocks/AI.Engram/README.md)

**Last Updated**: 2026-06-29  
**Total ADRs**: 3
