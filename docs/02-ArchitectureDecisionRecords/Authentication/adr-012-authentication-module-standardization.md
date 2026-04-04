# ADR 012: Authentication Module Standardization and Idempotent DI Registration

**Date**: 2026-03-31  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Authentication

## 2. Context (背景)

Authenticationモジュールの再利用性と拡張性を向上させるため、DI（Dependency Injection）登録のべき等性を確保し、各種認証スキーム（JWT, ApiKey, OAuth）の構成を柔軟にする必要があります。

## 3. Problem Statement (問題定義)

これまでの実装では以下の課題がありました：
- **スキーム名のハードコード**: "Bearer" や "ApiKey" といった文字列がコード内に分散しており、構成による変更が困難であった。
- **DI登録の重複**: `AddXxx` メソッドを複数回呼び出した際に、同じサービスが重複して登録されるリスクがあった。
- **登録順序の不透明性**: 依存関係を持つコンポーネントの登録順序が保証されておらず、ランタイムエラーの一因となっていた。

## 4. Decision (決定事項)

認証基盤の安定化と柔軟性向上のため、以下の標準化を行いました：

1.  **べき等なDI登録の徹底**: すべての登録を `TryAdd`、`TryAddTransient`、`TryAddScoped` に変換。複数登録を許容するインターフェースには `TryAddEnumerable` を使用する。
2.  **スキーム名の抽象化**: `JwtOptions` および `ApiKeyOptions` に `SchemeName` プロパティを追加し、OS等の外部構成からスキーム名を動的に決定可能にした。
3.  **パイプラインの順序固定**: 登録プロセスを「Core Setup -> Specific Strategies -> Global Pipeline」のフェーズに厳格に分離。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 従来の AddXXX 方式の継続**
  - **Rejected Reason**: テスト環境やプラグインアーキテクチャにおいて、サービスの二重登録による予期せぬ動作を回避できないため、`TryAdd` パターンへの移行が必須と判断した。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: サービスの二重登録を防止し、ユニットテストや統合テストの安定性が向上。スキーム名の柔軟な変更が可能になり、マルチテナントやレガシー統合が容易になる。
- **Negative**: `TryAdd` の特性上、後から登録したものが無視されるため、カスタマイズを行う際は登録順序（先にカスタムを登録する）に注意が必要。
- **Mitigation**: 開発ドキュメントに「カスタム実装は標準モジュールの前に登録すること」を明記する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- スキーム名の動的化により、デバッグ時やプロキシ経由の通信において、標準的な名称を避けるといったセキュリティ上の微調整も可能になります。
