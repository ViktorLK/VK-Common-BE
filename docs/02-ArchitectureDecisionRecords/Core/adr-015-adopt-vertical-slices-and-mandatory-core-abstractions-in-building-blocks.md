# ADR 015: Adopt Vertical Slices and Mandatory Core Abstractions in Building Blocks

- **Date**: 2026-04-25
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks Architecture Normalization & Deterministic Logic

## 2. Context (背景)

VK.Blocks の初期設計では、BuildingBlock 内部の構成要素を `Abstractions/`（インターフェース）や `Common/`（共通ユーティリティ）といった「技術レイヤー」ベースのフォルダでグループ化していた。しかし、プロジェクトの規模拡大に伴い、これらのフォルダが「何でも屋（Bucket）」となり、特定のドメイン機能に関連するコードが分散し、発見可能性（Discoverability）が低下するという課題が生じた。

また、`Guid.NewGuid()` や `DateTime.UtcNow` といった非決定論的（Non-deterministic）なシステム API がライブラリコード内で直接呼び出されており、単体テストにおける時間や ID 生成の制御、および将来的な決定論的シミュレーションの導入を困難にしていた。

## 3. Problem Statement (問題定義)

1.  **技術レイヤーによる凝集度の低下**: `Abstractions/` や `Common/` への配置は、開発者が機能単位ではなく物理的な型でコードを探すことを強いており、ドメインの凝集度（Cohesion）を損なっていた。
2.  **不確定要素の直接依存**: 非決定論的な API への直接依存は、外部インフラ（DB や Redis）との統合時に一貫性を保証することを難しくし、テスト時のモック化を妨げていた。
3.  **可視性境界の曖昧化**: どのクラスが外部公開用（Level 1）で、どれが内部実装用（Level 2+）であるかの物理的なガイドラインが不足していた。

## 4. Decision (決定事項)

### 4.1 垂直スライスによる組織化 (Vertical Slices)
BuildingBlock 内部のコード組織を、技術的な役割ではなく**ドメイン機能単位の垂直スライス**に強制的に移行する。

*   **構造**: `{FeatureName}/` を第1レベルのフォルダとする（例: `Guids/`, `Serialization/`, `Results/`）。
*   **配置**: インターフェース、実装、バリデーター、拡張メソッドなどはすべてそのスライス内に配置する。
*   **Internal 隠蔽**: スライス内の内部実装は `Internal/` サブフォルダに配置し、`internal` 修飾子を付与する。

### 4.2 共通フォルダの降級
`Abstractions/` および `Common/` フォルダの使用を**非推奨（Optional）**とし、どの垂直スライスにも属さない真に横断的なロジックに限定する。

### 4.3 核心抽象の強制利用 (Rule 5.1)
決定論的なロジックを保証するため、以下の Core 抽象の利用を全 BuildingBlock で義務付ける。

*   **GUID 生成**: 直接的な `Guid.NewGuid()` を禁止し、`IVKGuidGenerator` を使用する。
*   **時間取得**: 直接的な `DateTime.UtcNow` 等を禁止し、`TimeProvider` を使用する。
*   **JSON 操作**: 直接的な STJ `JsonSerializer` を禁止し、`IVKJsonSerializer` を使用する。

## 5. Alternatives Considered (代替案の検討)

### Option 1: 現状維持 (Technical Layering)
*   **Approach**: `Abstractions/` と `Common/` を維持し、命名規則のみで整理する。
*   **Rejected Reason**: 「共通」という言葉の曖昧さにより、結局は無秩序なコードの混在が再発するため。

### Option 2: 柱（Pillars）への集約
*   **Approach**: 複数の機能を少数の大きな「柱」ディレクトリに統合する。
*   **Rejected Reason**: 1つのディレクトリが肥大化し、依存関係のクリーンな分離が難しくなるため。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
*   **発見可能性の向上**: フォルダ名がそのまま機能を表すため、コードの探索が容易になる。
*   **テスト容易性の向上**: システム API が抽象化されることで、テスト時に時間や ID を完全に制御できる。
*   **アーキテクチャの自己説明性**: フォルダ構造を見るだけで、その BuildingBlock が提供する Capability が一目で理解できる。

### Negative
*   **Namespace の断片化**: 小さな機能ごとに Namespace が分かれるため、`using` が増える傾向にある。

### Mitigation
*   **ルート Namespace の活用**: 公開 API（Level 1）については、物理フォルダに関わらず `VK.Blocks.{ModuleName}` 根命名空間を使用することで、利用側の利便性を維持する（Rule 14）。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

*   **実装**: `Core` ブロックにて `DefaultGuidGenerator`, `SystemTextJsonSerializer` を提供し、他のブロックはこれらを DI 経由で利用する。
*   **セキュリティ**: 決定論的な ID 生成（SequentialGuid）を標準化することで、データベースのインデックスパフォーマンスとページネーションの安全性を向上させる。

---
**Last Updated**: 2026-04-25
