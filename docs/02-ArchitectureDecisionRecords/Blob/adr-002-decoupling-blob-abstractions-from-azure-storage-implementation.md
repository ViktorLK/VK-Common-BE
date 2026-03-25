# ADR 002: Decoupling Blob Abstractions from Azure Storage Implementation

## 1. Meta Data

- **ADR 编号与标题**: ADR 002: Decoupling Blob Abstractions from Azure Storage Implementation
- **Date**: 2026-03-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Blob Module Refactoring

## 2. Context (背景)

これまでの `VK.Blocks.Blob` モジュールは、抽象化（Interface/Contract）と Azure SDK による具体的な実装が同一プロジェクト内に混在していました。この構成では、インターフェースのみを利用したい消費者（Consumer）であっても、Azure SDK への過剰かつ不要な依存関係（Transient Dependencies）を引き込んでしまう問題がありました。

## 3. Problem Statement (問題定義)

1. **抽象化の漏洩 (Leaky Abstraction)**: `Abstractions` フォルダ内のインターフェースが間接的に Azure の型に依存したり、プロジェクト全体が Azure SDK を参照しているため、インフラの隠蔽が不完全でした。
2. **テスト可能性の低下**: インフラ依存が密結合しているため、単体テストにおいて Azure SDK の複雑なモデルをプロキシしたり Mock 化するコストが高くなっていました。
3. **OCP (Open-Closed Principle) 違反**: 将来的に AWS S3 や Google Cloud Storage 実装を追加する際、既存の `VK.Blocks.Blob` プロジェクトを修正せざるを得ず、拡張性に制約がありました。

## 4. Decision (決定事項)

`VK.Blocks.Blob` モジュールを以下の 2 つのプロジェクトに分離します。

1. **`VK.Blocks.Blob` (Base/Abstractions)**
   - 責務: 純粋なインターフェース、コントラクト（Records）、定数、および共通のガードロジックの提供。
   - 依存先: `VK.Blocks.Core` のみ。Azure SDK への依存は **ゼロ** です。
2. **`VK.Blocks.Blob.Azure` (Implementation)**
   - 責務: Azure Storage SDK を使用した具体的なサービス実装と DI 登録ロジックの提供。
   - 依存先: `VK.Blocks.Blob` および Azure SDK 関連パッケージ。

**名前空間の維持**:
既存のコードへの影響を最小限に抑えるため、`VK.Blocks.Blob.Azure` プロジェクトの `RootNamespace` も `VK.Blocks.Blob` のまま維持します。これにより、利用側はプロジェクト参照を切り替えるだけで、`using` ステートメントを修正することなく、引き続き同じ名前空間でサービスを利用可能です。

## 5. Alternatives Considered (代替案の検討)

### Option 1: 属性（Attributes）による条件付きコンパイル
- **Approach**: 同一プロジェクト内で `#if USE_AZURE` 等のプリプロセッサディレクティブを使用する。
- **Rejected Reason**: ビルドパイプラインが複雑化し、IDE の解析が困難になる。また、依存関係の完全な分離は達成できない。

### Option 2: 共有プロジェクト (.shproj) の利用
- **Approach**: 共通コードを共有プロジェクトに配置し、各インフラ実装プロジェクトから参照する。
- **Rejected Reason**: NuGet パッケージとしての配布性が悪く、VK.Blocks のモジュール化戦略に適合しない。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- **クリーンな依存関係**: 消費者は Azure SDK を一切意識せずに Blob インターフェースを利用可能。
- **将来の拡張性**: `VK.Blocks.Blob.S3` 等の新しい実装を、既存コードに影響を与えずに独立して追加可能。
- **テストの容易性**: 抽象層が Azure のモデルから完全に隔離されているため、Mock の作成が極めて容易。

### Negative
- **プロジェクト数の増加**: 1 つだったプロジェクトが 2 つに増え、管理コストが僅かに増加する。

### Mitigation
| 負面影響 | 緩和策 |
|---|---|
| ファイル移動による混乱 | `RootNamespace` を維持することで、コード上の変更（Breaking Changes）を回避。 |
| ソリューション構成の複雑化 | ソリューションフォルダを活用し、`BuildingBlocks` 配下で整理して配置。 |

## 7. Implementation & Security (実装詳細とセキュリティ考察)

### 実装詳細
- 既存の `BlobOptions` は抽象プロジェクトに配置し、バリデーターのみ実装プロジェクトへ移動します。
- `CancellationToken` の命名を `ct` から `cancellationToken` に標準化し、VK.Blocks の最新の命名規約に合わせます。

### セキュリティ考察
- **Path Traversal 防止**: 抽象プロジェクトに含まれる `BlobGuard.IsValidSafePath` を引き続き全エントリーポイントで強制し、インフラ実装が変わっても安全性を担保します。
- **機密情報の保護**: 接続文字列等の機密情報は `BlobOptions` を通じて `Blob.Azure` 側でのみ処理され、ログ出力時にはマスクされる設計を維持します。
