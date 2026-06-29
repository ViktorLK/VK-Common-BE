# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.DeveloperEcosystem 開発環境・ルールおよび横断基盤に関する主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture & Guidelines (コアアーキテクチャ・開発規約)

#### [ADR-001: Establish Tri-Layered AI Agent Collaboration Architecture](./adr-001-establish-tri-layered-ai-agent-collaboration-architecture.md)

**Status**: ✅ Accepted  
**概要**: 開発ルールおよびエージェント開発环境において、システム層・オーケストレーション層・実行層の三層境界を明確に定義し、カプセル化と疎結合を徹底する設計。  
**キーワード**: Agent Architecture, Tri-Layered, Guidelines

---

#### [ADR-002: Unified Generic Pipeline Execution Architecture](./adr-002-unified-generic-pipeline-execution-architecture.md)

**Status**: ✅ Accepted  
**概要**: 各ビルディングブロックに分散していたパイプライン実行の重複コードを排除し、`BeforeStages -> Middleware Onion -> AfterStages` の標準的な並行化・フック実行アルゴリズムを一元化した抽象基盤の導入。  
**キーワード**: Generic Pipeline, VKPipelineExecutorBase, DRY, Onion Middleware

---

## 🎯 ADR の読み方ガイド

### 全体アーキテクチャと実行フローの理解用
1. **ADR-001**: 開発環境におけるエージェント開発の 3 層レイヤーの原則と境界定義を理解するために読んでください。
2. **ADR-002**: システム全体の非同期パイプライン処理がどのようにスケジュールされ、並行実行（Task.WhenAll）されているかの基本設計を理解するために読んでください。

**Last Updated**: 2026-06-10  
**Total ADRs**: 2
