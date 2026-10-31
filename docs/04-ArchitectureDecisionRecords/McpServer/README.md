# Architecture Decision Records (ADR) - McpServer Index

このディレクトリには、VK.Blocks MCP Server ツールセットの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

#### [ADR-001: Migration to Native .NET MCP Server Implementation](./adr-001-migration-to-native-.net-mcp-server-implementation.md)

**Status**: ✅ Accepted  
**概要**: MCP サーバーのコア実装を TypeScript プロトタイプからネイティブ .NET 10 へ移行し、BuildingBlock 資産の直接再利用と開発体験の向上を実現。  
**キーワード**: .NET Native, Porting, Code Reuse

---

#### [ADR-002: Relocation of Mcp Server to Tools Directory](./adr-002-relocation-of-mcp-server-to-tools-directory.md)

**Status**: ✅ Accepted  
**概要**: クラスライブラリ（BuildingBlocks）と開発補助ツールの物理的境界を厳密化するため、`src/McpServer` から `src/Tools/McpServer` へプロジェクトを移設し、名前空間を `VK.Tools` に統一する設計。  
**キーワード**: Project Relocation, Directory Cleanup, Namespace Standardization

---

#### [ADR-003: Backlog Front-Matter Single Source of Truth and Deterministic Sprint Automation](./adr-003-backlog-front-matter-single-source-of-truth-and-sprint-automation.md)

**Status**: ✅ Accepted  
**概要**: バックログ管理の二重管理と不整合を解消するため、YAML front-matter を唯一の真実の源泉（SSoT）とし、状態遷移および個人用 Sprint サイクル（1週間固定、Review/Retro統合）を決定論的に自動化する設計。  
**キーワード**: Backlog Management, SSoT, YAML Front-Matter, Sprint Automation, State Machine

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとエコシステムの理解用
1. **ADR-001**: なぜ TypeScript を捨てて .NET へ移行したのか、その技術的背景とトレードオフを理解するために読んでください。
2. **ADR-002**: 開発補助用のクローズドなツール群と公開製品ライブラリを物理および名前空間レベルでどのように分離・構造化しているかを理解するために読んでください。
3. **ADR-003**: 個人開発におけるバックログと Sprint の自動化、および front-matter による機械的追跡性（Traceability）の実現方法を理解するために読んでください。

## 🔗 関連ドキュメント
- [Project Documentation](/README.md)
- [BuildingBlock Standards](/docs/01-Standards/README.md)

**Last Updated**: 2026-10-31  
**Total ADRs**: 3
