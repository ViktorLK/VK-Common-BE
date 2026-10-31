# ADR 007: Unified AI Engine Base Pattern

- **Date**: 2026-05-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: AI.SemanticKernel Engine Refactoring

## 2. Context (背景)

AISK における Chat, Embedding, Retrieval の各エンジンは、AI プロバイダーの解決、ガバナンス設定（タイムアウト、リトライ、監査）の適用、および例外から `VKResult` へのマッピングといった共通のロジックを共有しています。以前はこれらのロジックが各エンジンに個別に実装されており、コードの重複と挙動の不一致が生じていました。

## 3. Problem Statement (問題定義)

- **コードの重複**: タイムアウト解決やエラーハンドリングのロジックが複数のクラスに散在しており、修正時の影響範囲が広くなっていました。
- **一貫性の欠如**: 特定のエンジンでガバナンス機能（監査ログ等）が実装漏れになるリスクがありました。
- **保守性の低下**: 新しいエンジン（例：画像生成）を追加する際、多くの定型コードをコピー＆ペーストする必要がありました。

## 4. Decision (決定事项)

すべての Semantic Kernel ベースのエンジンに対する統一基底クラスとして `AISKEngineBase<TOptions>` を導入します：

1.  **共通ロジックの集約**:
    - **接続解決**: `ModelId`, `Provider` の優先順位（Args > Feature > Global）に基づく解決。
    - **ガバナンス適用**: レジリエンス（タイムアウト、リトライ）、監査、セーフティの設定適用。
    - **統一実行**: `ExecuteAsync` メソッドによる、例外トラップと `AISKErrorMapper` を介した `VKResult` への変換。
2.  **ジェネリック制約**: `TOptions` に対して `IVKAIConnectionSettings` および `IVKAIGovernanceOptions` への準拠を強制し、型安全性を確保します。

## 5. Alternatives Considered (代替案の検討)

### Option 1: デコレーターパターン
- **Approach**: 各エンジンの周りにガバナンス用デコレーターを被せる。
- **Rejected Reason**: SK の Kernel 自体が内部でフィルタを持っており、デコレーターを重ねると二重管理になり複雑化します。

### Option 2: 拡張メソッド
- **Approach**: `Kernel` に対する拡張メソッドで共通処理を実装する。
- **Rejected Reason**: 状態（Logger や Options インスタンス）を保持する必要があるため、クラスベースの方が適切です。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - コードの重複が大幅に削減され、DRY 原則が徹底されます。
    - すべての AI 機能で一貫した工業化挙動（エラーレスポンス、タイムアウト等）が保証されます。
- **Negative**:
    - 基底クラスへの依存が強まり、基底クラスの変更が全エンジンに影響します。
- **Mitigation**:
    - 基底クラスは最小限の共通責務のみに特化させ、具体的なビジネスロジックは派生クラスに限定します。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **実装場所**: `Kernel/Internal/AISKEngineBase.cs`
- **セキュリティ**: `ExecuteAsync` 内で捕捉された例外をログ出力する際、PII（個人情報）が含まれないよう `LogExecutionError` を通じて安全に処理します。

---
**Last Updated**: 2026-05-10
