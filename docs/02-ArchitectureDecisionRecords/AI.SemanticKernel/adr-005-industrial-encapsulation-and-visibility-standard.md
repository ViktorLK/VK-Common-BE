# ADR 005: Industrial Encapsulation and Visibility Standard

- **Date**: 2026-05-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: AI.SemanticKernel Module Industrialization

## 2. Context (背景)

AI.SemanticKernel building block の初期実装では、内部インターフェース（`IAISKKernelFactory` など）や内部プロバイダーが、ルートディレクトリや各機能ディレクトリのパブリックコンポーネントと混在していました。これは、内部ロジックの厳格なカプセル化とパブリック API 表面の明確な区分を求める **AP.03** ルールに抵触していました。また、一部の内部型にパブリック型専用の `VK` 接頭辞が付与されており、混乱を招いていました。

## 3. Problem Statement (問題定義)

- **カプセル化の欠如**: 内部的な実装詳細が外部に公開（leaking）されており、将来的なリファクタリング時に破壊的変更（Breaking Changes）を引き起こすリスクがありました。
- **命名規則の不一致**: 内部型に `VK` 接頭辞を使用することで、コンシューマーがどの型を直接使用すべきかの判断が困難になっていました。
- **保守性の低下**: パブリック API と内部実装が物理的に分離されていないため、モジュールの構造理解に時間がかかっていました。

## 4. Decision (決定事項)

厳格な階層化可視性構造（Tiered Visibility Structure）を導入します：

1.  **Internal フォルダの強制**: すべての実装詳細、内部インターフェース、およびヘルパークラスを、各機能ディレクトリ配下の `Internal/` サブディレクトリに移動します。
2.  **可視性の制限**: `Internal/` フォルダ内の型はすべて `internal` アクセス修飾子を使用します。
3.  **命名規則の修正**: 内部型からは `VK` 接頭辞を削除します。
4.  **パブリック API の定義**: レベル 1 フォルダ（ルート直下の機能フォルダ）には、コンシューマーによる拡張や設定に必要な型（Builder, Options, Marker, Public Interface）のみを配置し、これらには引き続き `VK` 接頭辞を付与します。

## 5. Alternatives Considered (代替案の検討)

### Option 1: ネームスペースによる分離のみ
- **Approach**: フォルダ構成は変えず、ネームスペースを `.Internal` に変更する。
- **Rejected Reason**: 物理的な場所が混在しているため、開発者が誤ってパブリックな場所に新しい内部型を作成するリスクが残ります。

### Option 2: すべてを internal にする
- **Approach**: すべての型を internal にし、`InternalsVisibleTo` でテストする。
- **Rejected Reason**: コンシューマーが拡張するためのポイント（Builder 等）まで隠蔽されてしまい、ライブラリとしての利用価値が低下します。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - パブリック API 表面積が最小化され、ライブラリの利用方法が明確になります。
    - 内部実装の変更が外部に影響を与えにくくなり、保守性が向上します。
- **Negative**:
    - ファイルの物理的な移動が必要となり、既存の内部参照の修正コストが発生します。
- **Mitigation**:
    - ツールによる一括リファクタリングと、ビルドチェックによる整合性確認を実施しました。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **ディレクトリ構成例**:
    - `Chat/VKChatOptions.cs` (Public)
    - `Chat/Internal/AISKChatEngine.cs` (Internal)
- **セキュリティ**: 内部的な認証情報（API Key）の保持ロジックなどを `Internal/` に完全に閉じ込めることで、誤った外部公開のリスクを低減します。

---
**Last Updated**: 2026-05-10
