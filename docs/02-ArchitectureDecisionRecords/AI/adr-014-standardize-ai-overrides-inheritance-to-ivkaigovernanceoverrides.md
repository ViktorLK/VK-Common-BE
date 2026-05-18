# ADR 014: Standardize AI Overrides Inheritance to IVKAIGovernanceOverrides

## 1. Meta Data
- **Date**: 2026-05-17
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks AI Module Configuration & Governance Standardization

## 2. Context (背景)
VK.BlocksのAIモジュールは、非決定的なLLM呼び出しと多様なセキュリティ要件（Prompt Injection、個人情報保護、コンテンツモデレーション）を保護するため、高度な「ガバナンス階層（Resilience, Quota, Audit, Safety）」を定义しています。

グローバル設定側（Settings/Options）では、[IVKInjectionOptions](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/AI/Guardrails/Injection/Protocols/IVKInjectionOptions.cs) などの各サブ機能インターフェースが [IVKAIGovernanceOptions](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/AI/Common/Governance/IVKAIGovernanceOptions.cs) を継承することにより、レジリエンス、監査（Audit）、割当（Quota）、安全（Safety）といったすべてのガバナンス設定を自動的に集約し、[VKInjectionOptions](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/AI/Guardrails/Injection/VKInjectionOptions.cs) などの具象レコードで保持する構成となっています。

しかしながら、リクエスト個別のオーバーライド（Overrides/Args）侧において、[IVKInjectionOverrides](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/AI/Guardrails/Injection/Protocols/IVKInjectionOverrides.cs) などのオーバーライドインターフェースが、個別の `IVKAIResilienceOverrides` や `IVKAIQuotaOverrides` のみを継承しており、[IVKAIAuditOverrides](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/AI/Common/Governance/Protocols/IVKAIAuditOverrides.cs) および [IVKAISafetyOverrides](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/AI/Common/Governance/Protocols/IVKAISafetyOverrides.cs) の継承が系統的に脱落していました。

## 3. Problem Statement (問題定義)
VK.Blocksは **AP.05 (Strict Overrides Contract - Mode B)** に基づき、ソースジェネレーター（Source Generator）を用いたコンパイル時の型安全なArgs自動生成を行います。AP.05では以下のセキュリティ原則が厳格に適用されます。
* **安全なデフォルト原則（Security by Default）**：`Options` に定義されているプロパティのうち、対応する `Overrides` インターフェースに含まれないメンバは、自動生成される `Args` レコードの公開表面から完全に除外される。

このルールにより、オーバーライドインターフェース（例：`IVKInjectionOverrides`）に `IVKAIAuditOverrides` や `IVKAISafetyOverrides` の継承が欠落している結果、ソース生成された `VKInjectionArgs` などのリクエスト用レコードから `EnableAudit` や `EnableContentFilter` などのオーバーライドプロパティが静的に排除されていました。

これにより、開発者はシングルリクエスト単位で「Prompt注入検知の監査ログの無効化（一時的なホワイトリスト）」や「内容フィルタの強制動的制御」を行うことが不可能となっており、要求仕様と実装のミスマッチ（ガバナンス覆写能力の欠落）が発生していました。

## 4. Decision (決定事項)
すべてのAIサブ機能のオーバーライドインターフェースを、個別の親インターフェース継承から、統合された **[IVKAIGovernanceOverrides](file:///e:/code/github/VK-Common-BE/src/BuildingBlocks/AI/Common/Governance/Protocols/IVKAIGovernanceOverrides.cs)** 継承へと標準化します。

### 影響を受けるインターフェースの変更箇所：
1. `IVKInjectionOverrides`
2. `IVKPrivacyOverrides`
3. `IVKContentOverrides`
4. `IVKTextOverrides`
5. `IVKEmbeddingsOverrides`
6. `IVKRetrievalOverrides`
7. `IVKReRankingOverrides`
8. `IVKChatOverrides`
9. `IVKAgentsOverrides`

### 代表的な修正コード例（IVKInjectionOverrides.cs）：
```csharp
public interface IVKInjectionOverrides :
    IVKAIProviderOverrides,
    IVKAIGovernanceOverrides
{
    float? BlockThreshold { get; init; }
}
```

この変更により、Settings側の `IVKAIGovernanceOptions` と Overrides側の `IVKAIGovernanceOverrides` が完全な対称性（Symmetry）を持つようになり、ソースジェネレーターは安全に `EnableAudit` や `EnableContentFilter` などの覆写プロパティを `VKXxxArgs` に含めて出力するようになります。

## 5. Alternatives Considered (代替案の検討)

### Option 1: Overridesインターフェースごとに個別にプロパティを再定義する
* **概要**: 継承を使わず、`IVKInjectionOverrides` 等に `bool? EnableAudit { get; init; }` を手動で書き込む。
* **却下理由**: 結合度とメンテナンスコストの激増。監査や安全ポリシーのプロパティ名・仕様が将来変更された際に、すべてのサブモジュールの個別ファイルを手動修正する必要が生じるため、DRY原則及びOCPに違反する。

### Option 2: ソースジェネレーターに特例ルールを埋め込む
* **概要**: Overrides インターフェースの宣言に関わらず、特定の設定名を持つものは自動的に Args に出力するロジックをコンパイラ拡張にハードコードする。
* **却下理由**: **AP.05** の「インターフェースが唯一の契約である」という原則を破壊し、メタプログラミングのブラックボックス化を招くため却下。契約ファースト（Contract-First）の原則を遵守すべきである。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive:
* **安全な機能対称性の回復**: すべてのサブモジュールにおいて、リクエスト単位での安全フィルタ・監査ログの動的トグルが型安全に実現される。
* **DRYと保守性の向上**: 治理（Governance）に関する仕様追加が、`IVKAIGovernanceOverrides` 一箇所で完結する。
* **AP.05契約の厳格遵守**: ソースジェネレータがコード定義通りのクリーンなコードを出力する状態を維持。

### Negative:
* **一時的なビルド影響**: `Args` レコードのプロパティ構成要素が増加するため、既存のテスト等のモックや初期化部分で再コンパイルが走る。

### Mitigation (緩和策):
* **No-Opマッピングの自動解決**: 拡張Args（`Args` レコード）はすべて `nullable` かつ `init` プロパティのため、未設定の呼び出し元（既存コード）はデフォルトで `null`（＝Options側のデフォルトを安全に継承）として動作し、後方互換性は100%維持される。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

### 安全なマージ優先度（Null-Coalescing Priority）：
実行エンジンの内部実装では、必ず以下の優先度でガバナンスオプションを適用します：
`args?.Property ?? _options.Property ?? _globalOptions.Property`

### セキュリティ面における注意点：
* **監査ログの意図的バイパス防御**: `EnableAudit` はデフォルトで `true`（監査有効）であり、たとえリクエスト `Args` が `EnableAudit = false` を指定した場合でも、企業ポリシー等で強制監査（Options側でハードコード）が設定されている場合は、エンジンレベルで `args` のオフ指定を無視する防御的実装を組み合わせます。
