# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.VectorIngest モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Modular Document Ingestion Parsing and Multi-Strategy Chunking Pipeline](./adr-001-modular-document-ingestion-parsing-and-multi-strategy-chunking-pipeline.md)

**Status**: ✅ Accepted  
**概要**: 生ドキュメントの解析・構造化・分割・ベクトル格納という一連の処理を統一制御するため、動的ファイル解析器と4つの分割戦略（固定、再帰、階層、セマンティック）を備えた `IVKIngestPipeline` 基盤を構築する設計。  
**キーワード**: Document Ingest, Text Chunker, Document Parser, Processing Pipeline

---

## 🎯 ADR の読み方ガイド

### インジェストプロセスの理解用
1. **ADR-001**: 外部ドキュメントをどのようなフェーズで安全にパース・チャンク分割し、高精度なベクトルデータベース登録を実現するかを理解するために最初に読んでください。

## 🔗 関連ドキュメント
- [VectorIngest Module Manifest](../../../src/BuildingBlocks/VectorIngest/README.md)

**Last Updated**: 2026-06-27  
**Total ADRs**: 1
