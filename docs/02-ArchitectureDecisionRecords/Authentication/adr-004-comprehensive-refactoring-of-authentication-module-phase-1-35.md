# ADR 004: Comprehensive Refactoring of Authentication Module (Phase 1-3.5)

**Date**: 2026-03-05  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: `src/BuildingBlocks/Authentication`

## 1. Context (背景)

これまでのアーキテクチャ監査により、`VK.Blocks.Authentication` モジュールにおいて以下の課題が特定されていました。

1. エラーを表現するマジックストリングがハードコードされており、保守性が低い。
2. トークン無効化処理の抽象化が不十分であり、また Refresh Token の TTL が30日間に固定化されていた。
3. JWTの無効化処理（`ITokenBlacklist`）とAPIキーの無効化処理が混在しており、Redis等のキャッシュ上でプレフィックスの衝突によるセキュリティリスクが存在した。
4. 認証の成功・失敗、APIキーのレートリミット到達などに対する「可観測性（Observability, テレメトリ・メトリクス）」の仕組みが欠如していた。

## 2. Problem Statement (問題定義)

現在の実装には以下の重大な技術的負債およびセキュリティリスクが内在していました：

- **保守性と一貫性の欠如 (Maintainability)**: エラーの生成箇所ごとに `new Error("Auth", "Invalid")` のような呼び出しが散在し、同一のエラー理由であっても異なる表現が使われるリスクがあった。
- **拡張性とテスト性のボトルネック (Testability & Extensibility)**: Refresh Token TTL の固定化によりテナントやポリシーごとの柔軟な運用ができなかった。
- **セキュリティ・バグのリスク (Security Risk)**: `ITokenBlacklist` の実装において、JWTの `jti` と API Key ID が区別されずにキャッシュへ格納される可能性があり、意図しないトークンの無効化（またはその逆）が発生しうる。
- **運用監視の死角 (Observability Gap)**: リクエストの成功率や遅延、特定のAPIキーに対する不正なアクセス（レートリミット到達）を定量的に監視する手段がなく、SREチームによる SLO (Service Level Objective) の計測が不可能であった。

## 3. Decision (決定事項)

上記の問題を根本から解決するため、以下のフェーズに分けたアーキテクチャの大規模リファクタリングを実施することを決定しました。

1. **フェーズ1: 基盤修正 (Foundation Fixes)**
    - `AuthenticationErrors` クラスを導入し、所有的認証エラー（Jwt, ApiKey, RefreshToken）を `static readonly Error` 定数として集約。
    - `ITokenRevocationService` のインターフェースを定義し、依存関係の逆転 (DIP) を適用。
    - `VKAuthenticationOptions` を通じて Refresh Token の TTL を外部から注入（`IOptionsMonitor` 経由）できるように改修。

2. **フェーズ2: セキュリティ強化 (Security Enhancements)**
    - APIキー決済用に特化した `IApiKeyBlacklist` を新設。
    - 分散キャッシュの実装において、キープレフィックスを明確に分離（例: `"revoked_apikey:"`）し、JWTとの衝突リスクを排除。

3. **フェーズ3-3.5: 可観測性の統合 (Observability Integration)**
    - 独自の Source Generator である `VK.Blocks.Generators`（`[VKBlockDiagnostics("VK.Blocks.Authentication")]`）を活用し、定型的な `ActivitySource` と `Meter` の生成を自動化。
    - 手動のボイラープレートコード（`AuthenticationActivitySource`, `AuthenticationMetrics`）を排除し、`AuthenticationDiagnostics` クラスとして集約。
    - 各バリデーションサービスにおいて、`StartActivity` によるトレースと `CreateCounter` によるメトリクス計上を実装。

## 4. Alternatives Considered (代替案の検討)

### Option 1: 手動でのテレメトリクラス実装の維持（Source Generator を使わない）

- **Approach**: `AuthenticationMetrics` や `AuthenticationActivitySource` を引き続き手動で保守し、各所でインスタンスを生成する。
- **Rejected Reason**: システム全体（`VK.Blocks`）で統一された計装のルールを強制することが難しくなり、将来的に他の Block で同様の実装を行う際にボイラープレートの重複を招くため却下。

### Option 2: `ITokenBlacklist` 内での Prefix 分岐

- **Approach**: 新しい `IApiKeyBlacklist` を作らず、既存の `DistributedCacheTokenBlacklist` のメソッド引数（あるいは内部ロジック）で JWT と API Key の Prefix を動的に切り替える。
- **Rejected Reason**: SRP (単一責任の原則) に違反する。JWT の `jti` を扱う概念と、API Key を扱う概念は本来独立したライフサイクルを持つべきであり、将来的なスケーリング（保存先 DB の分割など）の妨げになるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - マジックストリングの排除によるコンパイル時の型安全性と追跡可能性の向上。
    - テレメトリ（OpenTelemetry に準拠）が組み込まれたことで、Datadog 等の監視基盤での認証パフォーマンス・異常検知が即座に可能となる。
    - Source Generator の利用により開発者の Cognitive Load（認知的負荷）が低下。
- **Negative**:
    - Source Generator (`VK.Blocks.Generators`) に対する暗黙的な依存が強まる。コンパイル時のツールチェインに対する知識が必要になる。
- **Mitigation**:
    - Source Generator の仕様と動作に関するドキュメント（すでに `docs/04-AuditReports/Generators` 等に存在）をチームに周知し、ブラックボックス化を避ける。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **データ構造**: キャッシュの TTL は JWT/API Key 各自の有効期限（`ExpiresAt`）に連動して削除されるように構成し、メモリリークおよび無効なキャッシュエントリの滞留を防止する。
- **パフォーマンスとロック**: Source Generator によって生成される `static readonly` な `Meter` および `ActivitySource` を再利用することで、リクエストごとの多重インスタンス化および GC フレックスのペナルティを回避する。
- **オブザーバビリティ**: セキュリティインシデント（例: 連続した無効なAPIキーの試行やレートリミット到達）は `RecordRateLimitExceeded` パターンを用いて明確な Counter 属性として記録され、即時アラートのトリガーとして活用できる。
