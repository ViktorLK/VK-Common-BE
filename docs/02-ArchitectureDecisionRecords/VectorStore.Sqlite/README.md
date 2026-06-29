# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.VectorStore.Sqlite モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: SQLite-Based Local Vector Database Engine (SqliteVec)](./adr-001-sqlite-based-local-vector-database-engine-(sqlitevec).md)

**Status**: ✅ Accepted  
**概要**: 外部コンテナや外部クラウドサービスに依存せず、ローカルファイルベースまたはインメモリ上で高速な近傍ベクトル検索（k-NN）を実行するため、`sqlite-vec` 拡張モジュールを組み込んだインプロセスベクトルストレージプロバイダを提供する設計。  
**キーワード**: sqlite-vec, Local Vector Storage, In-Process Database, ACID Transactions

---

## 🎯 ADR の読み方ガイド

### ローカル用ベクトルインフラの理解用
1. **ADR-001**: 開発環境やオフラインエッジ環境での RAG 動作を極めてシンプルかつ高速にするためのインプロセス・ネイティブ SQLite ベクトル拡張の仕組みを学ぶために最初に読んでください。

## 🔗 関連ドキュメント
- [VectorStore.Sqlite Module Manifest](../../../src/BuildingBlocks/VectorStore.Sqlite/README.md)

**Last Updated**: 2026-06-22  
**Total ADRs**: 1
