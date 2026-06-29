# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Persistence.Sqlite モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Independent Sqlite Persistence Block](./adr-001-independent-sqlite-persistence-block.md)

**Status**: ✅ Accepted  
**概要**: SQLite データベースの構成や NuGet 依存関係を他のデータベースプロバイダから分離し、依存関係を極小化した独立モジュールとしてパッケージングする設計。  
**キーワード**: SQLite Provider, DbContext Configurator, Dependency Isolation

---

## 🎯 ADR の読み方ガイド

### データベースプロバイダのモジュール分離の理解用
1. **ADR-001**: 共有永続化コアを汚染せず、特定のデータベース（SQLite）の構成ロジックと依存関係をプラグイン化する手法を理解するために読んでください。

## 🔗 関連ドキュメント
- [Persistence Module Manifest](../../../src/BuildingBlocks/Persistence/README.md)

**Last Updated**: 2026-06-20  
**Total ADRs**: 1
