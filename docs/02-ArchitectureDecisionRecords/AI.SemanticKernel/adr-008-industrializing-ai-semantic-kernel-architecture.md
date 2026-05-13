# ADR 008: Industrializing AI Semantic Kernel Architecture

**Date**: 2026-05-13  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: AI.SemanticKernel Module Industrialization

## 1. Context (背景)

`AI.SemanticKernel` ビルディングブロックは、VK.Blocks における Semantic Kernel 統合の中核を担っています。しかし、初期の Lab フェーズの実装では、インフラストラクチャの漏洩（Infrastructure Leakage）や、パフォーマンス最適化の断片化が課題となっていました。特に、カーネルの構築プロセスが重い一方で、プロジェクトごとに場当たり的なキャッシュ実装が行われていたため、標準化された高性能な実行基盤が必要とされていました。

## 2. Problem Statement (問題定義)

1.  **インフラストラクチャの漏洩 (Infrastructure Leakage)**:
    `IVKAISKKernelFactory` や `IVKAISKPluginProvider` が `public` で公开されており、戻り値として `Microsoft.SemanticKernel` の型を直接返していました。これにより、利用側のプロジェクトが SK SDK に直接依存することを强制され、疎結合性が损なわれていました。
2.  **キャッシュ実装の断片化**:
    `PersonaWeavePulsar` (PWP) などの Lab プロジェクトで、独自のカーネルキャッシュ装飾（Decorator）が手動で実装されており、ロジックが重複・散在していました。
3.  **非決定的な実行 (Non-deterministic Execution)**:
    実行タイムアウトやキャンセル処理がシステムクロックに直接依存しており、テスト環境での決定的な動作確認や、クロックをモックした制御が困難でした。

## 3. Decision (決定事項)

工業化 DNA（Industrial DNA）への完全準拠を目指し、以下の設計変更を決定しました：

1.  **ファクトリとプロバイダーの内部化 (Internalization)**:
    `IAISKKernelFactory` および `IAISKPluginProvider` を `internal` に変更。パブリック API 表面積から SK 固有の型を排除し、`IVKChatEngine` などの抽象化されたインターフェースのみを公開します。
2.  **ネイティブ・カーネルキャッシュのデフォルト有効化 (Performance by Default)**:
    `AISKCachedKernelFactory` を内部デコレータとして実装し、`EnableKernelCaching` をデフォルトで `true` に設定。構築コストの高い Kernel インスタンスを自動的にキャッシュします。
3.  **確定的なセキュア・ハッシュ指纹 (Secure & Deterministic Fingerprint)**:
    キャッシュキーの生成に **SHA256** を採用。`Provider`, `ModelId`, `Endpoint`, `ApiKey` (hashed), およびプラグイン構成を統合した確定的なハッシュ値を生成。メモリダンプ等での機密情報漏洩を防ぎつつ、同一構成での再利用性を担保します。
4.  **リソース・ガバナンス (Resource Governance)**:
    `IMemoryCache` の `SizeLimit` に対応。各 Kernel インスタンスにサイズ配額を割り当て、さらに **30分間の絶対期限** と **10分間のスライド期限** を設定することで、メモリ肥大化と認証情報の失効に対応します。
5.  **Scrutor による DI 装飾の標準化**:
    DI コンテナ内での安全なサービス装飾（Decoration）を実現するため、**Scrutor** を採用。手動での `ServiceDescriptor` 操作を排除し、保守性を向上させます。
6.  **TimeProvider の全面注入**:
    全てのエンジン（Chat, Embedding, Retrieval, Text）および基底クラスに `TimeProvider` を注入。タイムアウト処理を `new CancellationTokenSource(timeout, TimeProvider)` 形式に統一し、決定的なテストを可能にします。

## 4. Alternatives Considered (代替案の検討)

*   **Option 1: パブリックインターフェースを維持したままの抽象化**
    *   **Approach**: `Kernel` クラスを自前の `IVKKernel` でラップする。
    *   **Rejected Reason**: ラッパーの維持コストが極めて高く、SK の急速なアップデートに追従できなくなるリスクがある。
*   **Option 2: 各プロジェクトでの手动キャッシュ継続**
    *   **Approach**: PWP などの Lab プロジェクトで引き続き手動でデコレータを登録する。
    *   **Rejected Reason**: VK.Blocks の「工業化」の方針に反し、再利用性と信頼性が向上しない。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
*   **ゼロリーク (Zero Leakage)**: 外部プロジェクトが SK SDK に依存することなく AI 機能を利用可能。
*   **高性能**: カーネル構築のオーバーヘッドが最小化され、高頻度なリクエストに対してミリ秒単位でのレスポンス向上が期待できる。
*   **高いテスト性**: `TimeProvider` により、時間経過を伴う複雑なキャンセルロジックを正確にテスト可能。

### Negative
*   **依存関係の増加**: `Scrutor` NuGet パッケージへの依存が追加される。
*   **内部構造の複雑化**: デコレータパターンの導入により、DI 構成のデバッグ難易度がわずかに上昇する。

### Mitigation
*   `AISKBlockRegistration` 内で登録順序を厳格に管理し、ソースコード内に `// [RuleID]` コメントを残すことで、DI 構成の意図を明文化する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

*   **キャッシュ指纹 (Fingerprint)**:
    `VKAISKOptions` の設定内容（Model, TemplateFormat, Planners 等）に基づきキャッシュキーを生成。設定変更が即座に Kernel の再構築に反映されるように設計。
*   **タイムアウト攻撃への防御**:
    `TimeProvider` を用いた厳格なキャンセルポリシーを全エンジンに適用。リソースの長時間占有を防ぎ、DoS 攻撃への耐性を向上させる。

---
**Last Updated**: 2026-05-13
