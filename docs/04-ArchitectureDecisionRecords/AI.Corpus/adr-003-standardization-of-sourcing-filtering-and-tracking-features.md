# ADR 003: Standardization of Sourcing, Filtering, and Tracking Features

- **Date**: 2026-06-15
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Corpus

## 1. Context (背景)

AI.Corpus は初期設計（ADR-001/002）においてナレッジライフサイクル管理を導入した。しかし、実際の運用とテスト自動化を進める中で、いくつかの課題が浮き彫りになった。第一に、登録クラスや拡張メソッドに `VKCorpus` と `AICorpus` という表記揺れが存在したこと。第二に、フィルターの命名（`MaxCountFilter`、`GroupFilter` など）が曖昧で、その仕様（エントリ単体の個数制限なのか、グループ全体の最大数制限なのか）が直感的に判別しづらかったこと。第三に、追跡機能（Tracking）の動作検証において、永続化ストア（データベース）の実装がないとテストが行えず、ローカル環境でのゼロインフラ検証（InMemory 実行）が困難であったことである。

## 2. Problem Statement (問題定義)

初期設計における曖昧さとテスト制約には、以下の問題がある：
1. **命名の表記揺れによる認知負荷**: `VKCorpusBlockRegistration` といった命名が他の `AIPsycheBlock` や `AIAfferentBlock` などの AI 関連ブロックの統一規格（`AICorpus` プレフィックス）から乖離し、インテリセンスや命名規則の静的監査（BB.01）で引っかかる。
2. **フィルターのセマンティクス曖昧性**: `ExclusiveGroupFilter` などが実際には「同一グループ内でのスコア上位 N 件の選択（Group Top-N）」であるにもかかわらず、名前からその動作が想起しづらい。
3. **ローカル環境テストの阻害**: 履歴を追跡・検証する `IVKKnowledgeInjectionStore` のオンメモリ実装がないため、単体テストや CI パイプラインで常にデータベース等のインフラを要求されてしまう。

## 3. Decision (決定事項)

モジュールの標準化とゼロインフラテストのサポートを両立するため、**「Standardization of Sourcing, Filtering, and Tracking (ソース・選別・追跡機能の規格化と整備)」**を決定する。

1. **命名規則の統一とプレフィックスの適用**:
   - すべての登録クラスやビルダー拡張メソッドを `AICorpus`（例: `AICorpusBlockRegistration`、`IVKAICorpusBuilder`）に統一する。
2. **フィルター名称の厳密化（Semantics Realignment）**:
   - フィルター各々の役割を明示するため、以下のように名称を変更・整理する：
     - `MaxCountFilter` -> `EntryMaxCountFilter` (個別エントリの数量上限)
     - `GroupFilter` -> `GroupMaxCountFilter` (グループ単位の数量上限)
     - `ExclusiveGroupFilter` -> `GroupTopNFilter` (グループ内でのスコア順選抜)
     - `ExclusionFilter` -> `GlobalExclusionFilter` (グローバル排他制限)
     - パイプラインステージ名を `CorpusFilteringStage` から `DefaultFilteringStage` へと統一規格化する。
3. **`InMemoryKnowledgeInjectionStore` の追加**:
   - 履歴追跡機能の疎結合テストを可能にするため、スレッドセーフな `ConcurrentDictionary` を用いたオンメモリ実装 `InMemoryKnowledgeInjectionStore` を標準追加し、インフラのない統合テスト環境下でデフォルト適用できるようにする。

### 整備後のフィルター・ストア構造

```csharp
namespace VK.Blocks.AI.Corpus.Filtering.Internal;

// セマンティクスが明瞭になったフィルター群
internal sealed class EntryMaxCountFilter : IVKKnowledgeLifecycleFilter { ... }
internal sealed class GroupMaxCountFilter : IVKKnowledgeLifecycleFilter { ... }
internal sealed class GroupTopNFilter : IVKKnowledgeLifecycleFilter { ... }
internal sealed class GlobalExclusionFilter : IVKKnowledgeLifecycleFilter { ... }

namespace VK.Blocks.AI.Corpus.Tracking.Internal;

// ゼロインフラテスト用のオンメモリ追跡ストア
internal sealed class InMemoryKnowledgeInjectionStore : IVKKnowledgeInjectionStore { ... }
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Retain original filter names to avoid renaming churn
- **Approach**: 命名変更に伴うコードの書き換えリスクを避け、元の曖昧な名前（`MaxCountFilter` 等）のまま内部実装のドキュメントを拡充して解決する。
- **Rejected Reason**: 長期的なプロジェクトメンテナンスにおいて、ドキュメントの記述とコードの乖離は確実に発生し、新しく参加した開発者の誤用とバグを誘発するため、今のうちにリファクタリングを断行すべきと判断した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **高い命名整合性**: `AI` シリーズの他のビルディングブロック（AI.Afferent、AI.Psyche）と完璧に揃った一貫性を獲得。
- **直感的なフィルター操作**: フィルターの動作セマンティクスが明確になったため、設定ファイルや Options からのトグル設定ミスが防止される。
- **容易な自動テスト**: インフラレスで動作する `InMemoryKnowledgeInjectionStore` により、CI での実行速度が向上し、テスト記述のハードルが下がった。

### Negative
- **既存のプロジェクト参照の修正**: `VKCorpus` の古い名前空間やビルダーを呼んでいたクライアント側コードにコンパイルエラーが生じる。

### Mitigation
- エラーは単純な名前の置換で解決可能であるため、マイグレーション手順を明確にし、ソースコードの置換をツールを併用して一括適用する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Test Infrastructure Safety**: テスト環境以外（実環境）では、デフォルトでオンメモリの `InMemoryKnowledgeInjectionStore` がロードされず、永続化（RDBMS や CosmosDB 等）向けのストアが依存関係として要求されるよう、DI 登録時のガードコードを強化する。

## 7. Status
✅ Accepted
