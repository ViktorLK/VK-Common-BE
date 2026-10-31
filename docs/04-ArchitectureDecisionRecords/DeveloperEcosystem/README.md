# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.DeveloperEcosystem 開発環境・ルールおよび横断基盤に関する主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture & Guidelines (コアアーキテクチャ・開発規約)

#### [ADR-001: Establish Tri-Layered AI Agent Collaboration Architecture](./adr-001-establish-tri-layered-ai-agent-collaboration-architecture.md)

**Status**: ✅ Accepted (Updated by ADR-003)  
**概要**: 開発ルールおよびエージェント開発環境において、Rules（憲法）・Workflows（手順書）・MCP Tools（メカニクス）の三層境界を明確に定義し、関心事の分離を確立した初期設計。  
**キーワード**: Agent Architecture, Tri-Layered, Guidelines

---

#### [ADR-002: Unified Generic Pipeline Execution Architecture](./adr-002-unified-generic-pipeline-execution-architecture.md)

**Status**: ✅ Accepted  
**概要**: 各ビルディングブロックに分散していたパイプライン実行の重複コードを排除し、`BeforeStages -> Middleware Onion -> AfterStages` の標準的な並行化・フック実行アルゴリズムを一元化した抽象基盤の導入。  
**キーワード**: Generic Pipeline, VKPipelineExecutorBase, DRY, Onion Middleware

---

#### [ADR-003: Evolve AI Agent Governance into Multi-Layered Inheritance and Tiered Dynamic Loading Architecture](./adr-003-evolve-ai-agent-governance-into-multi-layered-inheritance-and-tiered-dynamic-loading.md)

**Status**: ✅ Accepted (Updates and Extends ADR-001)  
**概要**: ADR-001 で単一ファイルに統合されていた Rules を、L0（基底思想）〜 L3（技術スタック）の多層単方向継承モデルへと分離し、常時ロード（Tier 1/2）と MCP によるオンデマンド動的ロード（Tier 3）を組み合わせた階層制ルール適用プロトコルへの発展的進化。  
**キーワード**: Layered Governance, Prompt Architecture, Tiered Strategy, Dynamic Loading, MCP, Hard-Lock, Fullstack Alignment

---

## 🎯 ADR の読み方ガイド

### 全体アーキテクチャと実行フローの理解用
1. **ADR-001**: 開発環境におけるエージェント開発の 3 層レイヤー（Rules / Workflows / Tools）の基本原則を理解するために読んでください。
2. **ADR-002**: システム全体の非同期パイプライン処理がどのようにスケジュールされ、並行実行（Task.WhenAll）されているかの基本設計を理解するために読んでください。
3. **ADR-003**: AI ガバナンス法典がどのように L0〜L3 に階層化され、MCP と連携してトークン節約と確定性を両立しているかの最新設計を理解するために読んでください。

**Last Updated**: 2026-10-05  
**Total ADRs**: 3
