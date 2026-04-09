# ADR 009: Permission Naming and Isolation Strategy

**Date**: 2026-04-08  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: VK.Blocks.Authorization - Domain Governance

## 1. Context (背景)

VK.Blocks はマルチモジュール構成を目指しており、単一のアプリケーション内で複数のドメイン（Identity, Finance, Logistics 等）が共存します。Source Generator による権限属性の自動生成を導入する際、単に定数名を使ってクラスを生成すると、異なるモジュール間で「Read」や「Update」といった汎用的な名前が競合し、コンパイルエラーや意図しないポリシーの混同を招くという課題がありました。

## 2. Problem Statement (問題定義)

權限の命名規則と、SG による属性生成時の名前空間（Namespace/ClassName）管理が標準化されていないため、モジュール間の境界（Bounded Context）が曖昧になり、認可ロジックの安全性が損なわれるリスクがあります。

## 3. Decision (決定事項)

堅牢な「権限ネーミング・アイソレーション」戦略を導入し、すべてを明示的に分類します。

1. **定義済み権限のプレフィックス強制**：
   `[GeneratePermissions]` で定義されたクラス名を「モジュール識別子」として使用します。
   - 例：`Finance` というクラス内の `Refund` 定数は、`RequireFinanceRefundAttribute` として生成される。
   - これにより、モジュール間のグローバルな名前空間干渉を 100% 回避します。
2. **匿名権限（Misc）の導入**：
   定数定義に属さず、Controller 上で直接文字列リテラルとして使用されている権限については、自動的に `Misc` プレフィックスを付与します。
   - 例：`[AuthorizePermission("User.Delete")]` は `RequireMiscUserDeleteAttribute` として認識される。
3. **治理の可視化**：
   `Misc` プレフィックスの存在は「未管理・未歸底」の権限であることを開発者に視覚的に示唆し、適切なモジュール定義クラスへの移行（治理）を促すシグナルとして機能させます。
4. **識別子の正規化**：
   権限文字列内のドット（.）やハイフン（-）は、SG 内部で C# の有効な識別子（PascalCase）に変換し、一貫性のある属性名を保証します。

## 4. Alternatives Considered (代替案の検討)

### Option 1: Global Uniqueness Responsibility (開発者責任)
- **Approach**: 開発者が自分で重複しない名前（Identity_Read 等）を定数に付ける。
- **Rejected Reason**: ガバナンスとしての強制力が弱く、大規模チームでは必ず衝突が発生する。

### Option 2: Full Namespace Support
- **Approach**: C# の Namespace をそのまま生成クラスの構造に反映する。
- **Rejected Reason**: Attribute の名称が極端に長くなり、コードの可読性を損なうため、モジュール名によるプレフィックスにとどめた。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive (メリット)
- **競合の根絶**：大規模統合プロジェクトにおいても、他チームの権限定義を気にすることなく開発が可能。
- **ガバナンスの向上**：`Misc` プレフィックスを検索することで、リファクタリングが必要な箇所を一瞬で特定できる。

### Negative (デメリット)
- **命名の冗長性**：属性名が若干長くなる（例：`RequireRefund` ではなく `RequireFinanceRefund`）。
- **緩和策**: インテリセンスが効くため、タイプの手間は最小限に抑えられる。

---
**Last Updated**: 2026-04-08  
