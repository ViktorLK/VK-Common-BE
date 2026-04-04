# ADR 015: Eliminate Reflection for OAuth Mappers via Source Generators

- **Date**: 2026-04-01
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Native AOT Compatibility / Startup Optimization

## Context (背景)

OAuth クレームマッパーの動的登録は、従来 `ModuleInitializer` とリフレクション、および静的キャッシュを使用して実行時に行われていた。これは起動時のオーバーヘッドを伴い、Native AOT との互換性も低かった。

## Problem Statement (問題定義)

1. **起動速度**: `AppDomain.GetAssemblies()` をスキャンして属性を探すロジックは、クラス数に比例して起動時間が延びる。
2. **Native AOT**: リフレクションによる動的型生成やスキャニングは、事前のトリミングが困難であり、ランタイムエラーの原因となる。

## Decision (決定事項)

Source Generator を使用して、ビルド時に `[OAuthProvider]` 属性が付与されたクラスを抽出し、それらを DI コンテナに `AddKeyedScoped` する拡張メソッドを自動生成する。これによりリフレクションを排除し、起動時のフットプリントを最小限に抑える。

- ビルド時にマッパーを静的に解析。
- `AddKeyedScoped` による型安全な DI 登録コードを生成。
- 実行時のアセンブリスキャンを完全に廃止。

## Consequences & Mitigation (結果と緩和策)

- **Positive**: リフレクション・ゼロを実現。Native AOT 環境での完全な動作保証と、ミリ秒単位の起動高速化。
- **Negative**: ビルド時間が僅かに増加する。
