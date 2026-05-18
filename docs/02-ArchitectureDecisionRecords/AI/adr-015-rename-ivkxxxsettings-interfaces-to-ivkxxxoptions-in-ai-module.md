# ADR 015: Rename IVKxxxSettings interfaces to IVKxxxOptions in AI module

## 1. Meta Data
- **Date**: 2026-05-18
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks AI Module Configuration & Terminology Standardization

## 2. Context (背景)
VK.BlocksのAIモジュールは、非決定的なLLM呼び出しと高スループットな処理を保護するため、レジリエンス、監査（Audit）、割当（Quota）、安全（Safety）といった高度なガバナンス階層を定義しています。

当初の設定管理インターフェースは、`IVKAIProviderOptions` や `IVKAIQuotaOptions` などの `Settings` サフィックスを用いて命名されていました。しかしながら、VK.Blocksの統一アーキテクチャ設計標準（**AP.04/BB.05**）においては、すべての構成データ型およびインターフェースに対して `Options` サフィックスを体系的に適用することが規定されています。これに基づき、モジュール内で定義されるすべての設定契約インターフェースを `Settings` から `Options` に統一変更し、命名規則の不整合を排除する決定を行いました。

## 3. Problem Statement (問題定義)
従来の `Settings` サフィックスの使用は、以下の課題を生じさせていました：
1. **命名規則の不整合（Inconsistency）**：具象レコードクラス（例：`VKChatOptions`）が `Options` であるのに対し、実装するインターフェース（例：`IVKChatOptions`）が `Settings` となっており、命名対称性が欠如していました。
2. **AP.04/BB.05 への違反**：フレームワーク標準である `IVKBlockOptions` の構造化命名規則に準拠しておらず、開発者がDI登録や機能拡張を行う際の設定用語に混乱を招いていました。
3. **将来の自動生成との整合性**：AP.05 (Strict Overrides Contract) に基づくArgs自動生成など、型ベースのコードメタプログラミングにおいて、一貫性のあるパターンマッチング（サフィックスの置換規則）を定義する際の障害となっていました。

## 4. Decision (決定事項)
`src/BuildingBlocks/AI` モジュールで定義されているすべての `IVKxxxSettings` インターフェースを **`IVKxxxOptions`** へと改名します。また、インターフェースが定義されている各 `.cs` ファイルの名称も変更します。

### 対象となる19のインターフェースおよびファイル：
1. `IVKAIProviderOptions` -> `IVKAIProviderOptions`
2. `IVKAIGovernanceOptions` -> `IVKAIGovernanceOptions`
3. `IVKAIQuotaOptions` -> `IVKAIQuotaOptions`
4. `IVKAIResilienceOptions` -> `IVKAIResilienceOptions`
5. `IVKAISafetyOptions` -> `IVKAISafetyOptions`
6. `IVKAIAuditOptions` -> `IVKAIAuditOptions`
7. `IVKRetrievalOptions` -> `IVKRetrievalOptions`
8. `IVKReRankingOptions` -> `IVKReRankingOptions`
9. `IVKEmbeddingsOptions` -> `IVKEmbeddingsOptions`
10. `IVKBudgetingOptions` -> `IVKBudgetingOptions`
11. `IVKTextOptions` -> `IVKTextOptions`
12. `IVKPrivacyOptions` -> `IVKPrivacyOptions`
13. `IVKInjectionOptions` -> `IVKInjectionOptions`
14. `IVKContentOptions` -> `IVKContentOptions`
15. `IVKGenerationOptions` -> `IVKGenerationOptions`
16. `IVKChatOptions` -> `IVKChatOptions`
17. `IVKSpeechOptions` -> `IVKSpeechOptions`
18. `IVKTranscriptionOptions` -> `IVKTranscriptionOptions`
19. `IVKAgentsOptions` -> `IVKAgentsOptions`

### 代表的なコード変更例（IVKAIProviderOptions.cs）：
```csharp
namespace VK.Blocks.AI;

public interface IVKAIProviderOptions
{
    VKAIProviderType? Provider { get; init; }
    string? ModelId { get; init; }
    VKSensitiveString? ApiKey { get; init; }
    string? Endpoint { get; init; }
}
```

この変更に伴い、AIモジュール内部および下流モジュール（`AI.SemanticKernel` や `AI.Cognitive` 等）におけるすべての参照箇所を一括で修正し、ビルドの健全性を確保します。

## 5. Alternatives Considered (代替案の検討)

### Option 1: 既存の Settings インターフェースを非推奨（Obsolete）として残し、Options インターフェースを新規追加する
* **概要**: 旧型を `[Obsolete]` 指定して維持し、段階的に移行を促す。
* **却下理由**: BuildingBlocks のコア部分は開発時の機敏性とクリーンさを優先すべきであり、不要な非推奨型を長期間維持することはコードベースの肥大化と保守コストの増大（Technical Debt）を招くため。一括リファクタリングが最適であると判断。

### Option 2: 具象クラス側を `VKxxxSettings` に改名して `Settings` に統一する
* **概要**: 標準の `Options` パターンを放棄し、すべての設定項目を `Settings` 用語に揃える。
* **却下理由**: .NET標準の `IOptions<TOptions>` パターンとの高い親和性と、すでに確立されている他のVK.Blocksモジュール（Authentication 等）との統一性を維持するため、フレームワーク全体の整合性として `Options` サフィックスを標準とすべきである。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive:
* **アーキテクチャの完全な一貫性**：インターフェース（`IVK...Options`）と具象クラス（`VK...Options`）の名前が対称になり、コードの読みやすさと理解のしやすさが極限まで向上。
* **標準への厳格な準拠**：AP.04/BB.05 標準に100%準拠。

### Negative:
* **広範なリファクタリング**：依存する下流プロジェクト（`AI.SemanticKernel` 等）にも変更が波及するため、一時的に大規模なビルドエラーが発生する。

### Mitigation (緩和策):
* 自動置換およびリファクタリングツールを活用し、すべてのプロジェクト参照箇所を一括で正確に置換することで、移行に伴う影響を最小化する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

### 実装ステップ：
1. `src/BuildingBlocks/AI` 内の19個のインターフェースファイルをリネームし、ファイル内のインターフェース名を置換。
2. AIモジュール内の具象クラス（`VKRetrievalOptions` 等）の `implements` 宣言を `IVKxxxOptions` に更新。
3. `AI.SemanticKernel`、`AI.Cognitive` などの下流モジュールおよびテストプロジェクトにおける `IVKxxxSettings` の参照をすべて置換。
4. 完全なソリューションビルドを実行し、すべてがコンパイル可能であることを検証。
