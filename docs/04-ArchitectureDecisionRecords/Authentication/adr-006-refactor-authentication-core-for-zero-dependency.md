# ADR 006: Refactor Authentication Core for Zero-Dependency and InMemory Defaults

**Date**: 2026-03-29
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Authentication Module

## 2. Context (背景)

Authentication モジュールはシステムのあらゆる場所で利用される中核的なインフラストラクチャパッケージです。これまでは、レート制限やトークン失効などの状態管理のために `Microsoft.Extensions.Caching.Abstractions` に依存し、分散キャッシュ（IDistributedCache）を前提とした設計となっていました。

しかし、この設計には以下の課題がありました：
1. **単一ノード環境での過剰な依存**：小規模なデプロイやテスト環境においても、不要なキャッシュ抽象層の依存を強いることになります。
2. **分散環境固有の最適化の欠如**：Redis のようなインフラ特有の強力な機能（Lua スクリプトによるアトミック操作など）を利用する場合、インターフェイスの制約上、美しくないハックが必要になる、あるいはコアパッケージ内に特定の技術実装が漏れ出す原因となります。

## 3. Problem Statement (問題定義)

コアインフラストラクチャパッケージが不必要なサードパーティ依存を持つことは、VK.Blocks アーキテクチャ原則（基盤パッケージは極限まで軽量に保つ）に反しています。現状の実装では、`IDistributedCache` が必須となっており、「Pay-as-you-go（必要な時に必要な機能だけ追加する）」というモジュラリティの思想を満たせていません。

## 4. Decision (決定事項)

Authentication コアモジュールを **ゼロ追加依存（Zero-Dependency Core）** にリファクタリングし、分散環境向けの機能は専用の拡張パッケージに分離する設計を採用します。

1. **ゼロ依存コア実装**: `VK.Blocks.Authentication` から `Microsoft.Extensions.Caching.Abstractions` などのキャッシュパッケージを完全に削除します。
2. **InMemory プレースホルダー実装**: キャッシュ抽象の代わりに、`ConcurrentDictionary` を使用した単一ノード向けの高速な `InMemory` 実装をデフォルトの `Singleton` として提供します（例：`InMemoryApiKeyRateLimiter`）。これにより、Redis等を用意しなくてもすぐにシステムが動作します。
3. **独立した拡張パッケージ**: 分散キャッシュや Redis 特有の操作が必要な環境向けに、`VK.Blocks.Authentication.StackExchangeRedis` という専用インフラストラクチャパッケージを作成します。
4. **シームレスなプロバイダ切り替え**: 拡張パッケージを利用する場合、`services.AddVKAuthenticationStackExchangeRedis()` と一行記述するだけで、DI コンテナ内の `InMemory` 状態管理クラスがすべて分散版（`Distributed...` または `Redis...`）に上書きされます。

## 5. Alternatives Considered (代替案の検討)

- **Option 1**: 既存のまま `IDistributedCache` と単一ノード用の `MemoryCache` を使い続ける。
  - **Rejected Reason**: MSの `MemoryCache` パッケージへの依存が残る。また、Redis特有のアトミック最適化（INCR等）を導入する際、`IDistributedCache` の制限がボトルネックになる。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - コアモジュールが極めて軽量になり、単一ノード（ローカル開発や小規模デプロイ）で外部サービスなしで完全に動作します。
  - ポリモーフィズムを活用して、Redis 等の特性をフル活用する実装をコアロジックを汚さずに追加できます。
- **Negative**:
  - `InMemory` モードでは、アプリケーション再起動時に失効リスト（ブラックリスト）やレートリミット状態が失われます（状態が揮発性）。またロードバランサ背後の複数ノード構成では状態が同期されません。
- **Mitigation**:
  - アプリケーションがスケールアウトするフェーズで、明示的に `StackExchangeRedis` 拡張パッケージを導入させるよう、プロバイダ拡張メソッド経由での意図的なオプトイン方式を採用しています。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **ガベージコレクションへの配慮**: `InMemory` 実装で使用する `ConcurrentDictionary` には無限に要素が蓄積しないよう、定期的（または遅延的）な有効期限切れキーのパージ処理（Lazy Eviction）を実装しています。
- **スレッドセーフティ**: 単一ノード内での高い並行性に対応するため、`Dictionary` ではなく `ConcurrentDictionary.AddOrUpdate` などのスレッドセーフな API を利用しています。
