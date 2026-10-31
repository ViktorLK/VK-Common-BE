# ADR 006: Establishing Granular Capability-Based Structure and Namespace Alignment in Core

## 1. Meta Data

- **Date**: 2026-04-17
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Core Module Restructuring and Explicit Usings Migration

## 2. Context (背景)

`src/BuildingBlocks/Core` モジュールは、プロジェクトの主要な基盤として機能していますが、成長の過程でクラスの配置ルールが曖昧になり、徐々に名前空間（Namespace）とフォルダ構造が一致しないケースが発生していました。また、一時期、全機能を「5つの柱（Semantic Pillars）」へと大きくまとめる案が立案されましたが、実際の適用においては「Utilities」や「Configuration」といった粒度の粗い集約（The Utility Bucket アンチパターン）が逆にモジュール内の密結合を引き起こすことが懸念されました。

## 3. Problem Statement (問題定義)

- **The Utility Bucket リスク**: 汎用的なフォルダ（Utilities等）にキャッシュ機構やDIロジックを詰め込むと、将来的な機能切り出し（独立したライブラリ化）が困難になる。
- **名前空間の不透明さ**: フォルダ構造と名前空間（Namespace）が厳密に一致していないため、使用側（Web, Infrastructure 等）が何を参照しているのか直感的に推測できない。

## 4. Decision (決定事項)

過度なグループ化（5つの柱案）を却下し、**明示的な機能・責務単位での高解像度なフォルダ分割（12のCapabilityエリア）を最終決定とし、フォルダ名と完全修飾名前空間（Fully Qualified Namespace）を厳密に 1:1 で一致させる** アーキテクチャルールを確立しました。

現在の Core に確立された主な機能エリア（`VK.Blocks.Core.*`）:

- **`Abstractions/`**: 他に依存を持たない横断的な抽象インターフェース
- **`Caches/`**: 高速な式ツリーキャッシュ・メタデータ処理用モデル
- **`Constants/`**: グローバル定数群
- **`DependencyInjection/`**: 各種 Block の DI 登録用拡張メソッド
- **`Diagnostics/`**: 可観測性（OpenTelemetry等）やジェネレーター用の属性群
- **`Domain/`**: DDD要素（Entity, AggregateRoot）およびインターフェース
- **`Exceptions/`**: エラーハンドリング・例外処理基盤
- **`Internal/`**: Core 内部でのみ利用される非公開クラス（`internal` 修飾）
- **`Models/`**: 外部に公開するデータ構造
- **`Pagination/`**: ページング処理結果モデル
- **`Results/`**: `Result<T>` パターンの実装中核
- **`Security/`**: 認可・プロパティレベルのセキュリティキャッシュ

## 5. Alternatives Considered (代替案の検討)

- **Option: 「5つの柱（5 Semantic Pillars）」案（案B）を採用する**
    - **Approach**: Caches や DependencyInjection を `Utilities` や `Configuration` に統合してフォルダ数を減らす。
    - **Rejected Reason**: `Configuration` というフォルダ名が `IConfiguration` や DI フレームワークの責務と認知的な衝突を起こす点、および独立性の高い機能を強引に同じ階層に置くことによる設計の劣化を避けるため却下。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive (メリット)**:
    - **Single Responsibility**: 各フォルダおよび名前空間の関心事が極めて明確化された。
    - **Decoupling Friendly**: 将来的に機能規模が拡大し、`Caches` や `Results` などを別々のアセンブリ（DLL）として独立させる際、依存関係を綺麗に分離できる。
    - **Explicit External Usings**: 明示的な `using` 宣言への移行プロジェクトとも綺麗に連携し、消費側が必要な機能領域（名前空間）だけを選択して Import できるようになった。
- **Negative (デメリット)**:
    - フォルダ・名前空間の細分化により、上位モジュールでの `using` 宣言文（import行）の数がわずかに増加する。
- **Mitigation (緩和策)**:
    - 自動化スクリプト（PowerShell）を利用して全ソリューションの `using` ステートメントの不足と重複を自動修正しているため、開発者への負担増はゼロに抑えられている。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Rule Enforcement**: Coreモジュール内に新規ファイルを追加する際は、必ずこれら12の明確な機能バウンダリのいずれかに分類すること。既存フォルダに強引に押し込むのではなく、全く新しい概念を持つ場合は新しくトップレベルフォルダを作成することを基本方針とします。
- **Source Generators**: Generatorsは常に再編後の最新の名前空間（例: `VK.Blocks.Core.Diagnostics`）を探索するように修正されており、属性ベースのコードのコンパイルと安全性が保たれています。
