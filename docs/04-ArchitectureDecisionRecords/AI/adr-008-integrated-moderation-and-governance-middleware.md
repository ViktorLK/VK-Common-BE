# ADR 008: Integrated Moderation and Governance Middleware

- **Date**: 2026-05-10
- **Status**: 📝 Draft
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI / Governance

## 1. Context (背景)

生成 AI のエンタープライズ利用において、入力（Prompt）および出力（Response）の安全性確保は必須要件です。不適切なコンテンツ（ヘイトスピーチ、自傷行為、性的内容など）や機密情報（PII: 個人情報）の漏洩を自動的に検出し、遮断するガバナンス機構が求められています。

## 2. Problem Statement (問題定義)

1. **実装の重複**: 各アプリケーションや各機能で個別に `Moderation` API を呼び出すロジックを書くと、実装漏れやメンテナンスコストの増大につながる。
2. **横断的関心の分離**: AI ロジックの本質は「生成」であり、「検閲」はそれとは独立した横断的な関心事（Cross-cutting concerns）であるべき。
3. **バイパスのリスク**: 開発者が意図的または過失によりセーフティチェックをスキップできてしまう構造は、コンプライアンス上のリスクとなる。

## 3. Decision (決定事項)

1. **Decorator パターンによる「Safety-First」パイプライン**:
   `IVKChatEngine` や `IVKEmbeddingEngine` をラップする `VKGovernanceDecorator` を導入し、透過的に検閲ロジックを注入します。

2. **自動アタッチメント構成**:
   設定（`VKAIOptions` または `VKChatOptions`）の `EnableContentFilter` が true の場合、DI コンテナが自動的に `Moderation` チェッカーをエンジンに連結します。

3. **標準化された `VKGovernanceException` / `AIErrors.SafetyViolation`**:
   安全性の問題でブロックされた場合、一貫したエラーコードとメッセージを返します。

4. **二段階の動作モード**:
   - **Blocking Mode**: 検知した瞬間にリクエストを中断し、エラーを返す（本番環境推奨）。
   - **Audit-Only Mode**: リクエストは継続するが、警告をログ出力し、診断データとして記録する（分析・テスト用）。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 各 Engine クラスの中で直接 Moderation API を呼ぶ**
  - **Rejected Reason**: Single Responsibility Principle (SRP) に違反し、テストが困難になる。
- **Option 2: プロバイダー側（Azure OpenAI 等）のビルトインフィルターのみに頼る**
  - **Rejected Reason**: プロバイダーによってフィルターの強さや挙動が異なるため、VK.Blocks として一貫したガバナンスを保証できない。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - 開発者は安全性を意識せずに生成ロジックに集中できる。
  - セキュリティポリシーを一元管理（設定ファイルのみで変更）できる。
- **Negative**:
  - 生成リクエストの前（または後）に追加の API コールが発生するため、レイテンシが増大する。
- **Mitigation**:
  - 高速なオンプレミス（ローカル）モデルによる一次検閲や、ストリーミングと並行した非同期検閲のサポートを検討する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: `VKGuard` の拡張として `VKAIBlock.EnsureSafeContent()` のようなメソッドを提供し、デコレーター内で共通利用します。
- **セキュリティ考察**: 検閲自体を回避しようとする「Jailbreak（脱獄）」プロンプトのパターンを常に更新し、検閲エンジン自体も最新の状態に保つプロセスが必要です。

**Last Updated**: 2026-05-10
