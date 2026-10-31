# ADR 017: Standardize LifeCycle Management in In-Memory Authentication Providers

- **Date**: 2026-04-01
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Resource Management & Lifecycle

## Context (背景)

メモリ内キャッシュを使用する認証プロバイダー（ApiKey, JWT リフレッシュトークン等）は、以前は明確なリリースメカニズムを持っておらず、アプリケーションの終了やテスト終了時のリソース整合性が不透明であった。また、IAsyncDisposable によるクリーンアップが DI コンテナに委ねられていなかった。

## Problem Statement (問題定義)

1. **リソースリーク**: 明示的な `Dispose` ロジックがない場合、特にテスト環境においてメモリ内データが残留し、後続のテストに影響を与える可能性がある。
2. **DI不整合**: 非同期なクリーンアップが正しく DI ライフサイクルに統合されていない。

## Decision (決定事項)

すべての InMemory Authentication Provider に `IAsyncDisposable` を実装し、キャッシュの即時解放を保証する。また、`InMemoryCleanupBackgroundService` において Self-Adaptive 戦略（アクティブなプロバイダーのみクリーンアップ）とホットリロード可能なインターバル設計を採用する。

- すべてのメモリ内プロバイダーに `IAsyncDisposable` を強制。
- クリーンアップサービスは対象のプロバイダーが登録されている場合のみ稼働。
- 実行時のインターバル変更をサポート。

## Consequences & Mitigation (結果と緩和策)

- **Positive**: リソース整合性の向上、テストの決定論的動作の保証。
- **Negative**: 非同期破棄のオーバーヘッド。
