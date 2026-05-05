# ADR 014: Automated Block Identity & Metadata via Source Generation

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization / VK.Blocks.Generators

## 1. Context (背景)

各 BuildingBlock（Authorization, Authentication, Core 等）は、テレメトリ（Meter 名、ActivitySource 名）、構成パス（appsettings.json のセクション名）、および診断情報において一意の識別子を必要とする。従来、これらの文字列（例: "VK.Blocks.Authorization"）は各ブロック内で手動で定義されていた。また、診断情報として提供されるモジュールバージョンも AssemblyInfo 等から手動で取得・提供されており、実装の重複と不整合が発生しやすい状態にあった。

## 2. Problem Statement (問題定義)

手動による識別子管理には以下の問題があった：
1. **名前の不一致**: ある箇所では "VK.Blocks.Auth"、別の箇所では "VK.Blocks.Authorization" と定義されるなど、命名の揺れが発生し、テレメトリの集約に支障をきたしていた。
2. **マジックストリングの散在**: 識別子が文字列定数として多くのファイルに散在しており、変更時の影響範囲が不明確であった。
3. **メンテナンスコスト**: 新しい BuildingBlock を作成するたびに、同じようなボイラープレート（識別子の定義、バージョンの取得ロジック）を再実装する必要があった。
4. **型安全性の欠如**: 文字列ベースの識別子はコンパイル時のチェックが効かず、実行時に初めてタイポが判明するリスクがあった。

## 3. Decision (決定事項)

Source Generator（`VKBlockDiagnosticsGenerator`）を活用し、BuildingBlock の識別子およびメタデータを自動合成する仕組みを導入した：

1. **IVKBlockMarker による自動検出**:
   - `IVKBlockMarker` インターフェースを実装するすべての部分クラス（例: `VKAuthorizationBlock`）をソースジェネレータの対象とする。
2. **定数の自動合成**:
   - ジェネレータは、クラス名から自動的に `BlockIdentifier`（完全な URN: "VK.Blocks.Authorization"）と `BlockName`（短縮名: "Authorization"）を抽出し、定数として合成する。
3. **バージョン情報の自動解決**:
   - MSBuild プロパティ（`Version`）または Assembly の `InformationalVersion` から、ビルド時にバージョンを抽出し、定数として埋め込む。
4. **インターフェースの自動実装**:
   - `IVKBlockMarker` のプロパティ（`Name`, `Identifier`, `Version`）を、合成された定数を参照する形で自動実装する。

### 生成されるコードの例：
```csharp
// ソースコード
public sealed partial class VKAuthorizationBlock : IVKBlockMarker;

// ジェネレータによる生成後
partial class VKAuthorizationBlock
{
    public const string BlockName = "Authorization";
    public const string BlockIdentifier = "VK.Blocks.Authorization";
    public const string BlockVersion = "1.0.2";

    public string Name => BlockName;
    public string Identifier => BlockIdentifier;
    public string Version => BlockVersion;
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 基底クラスによる共通化
- **Approach**: 共通の基底クラスでリフレクションを用いて自身の型名から識別子を取得する。
- **Rejected Reason**: リフレクションのコストが発生すること、および実行時に名前が解決されるため定数として（属性の引数等に）使用できないため。

### Option 2: 共通定数クラスでの集中管理
- **Approach**: `VKBlocksConstants` のような一箇所にすべての識別子を定義する。
- **Rejected Reason**: 新しいブロックを追加するたびに共通ライブラリを修正する必要があり、モジュール間の疎結合性が損なわれるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な一貫性**: 命名規則に基づき機械的に生成されるため、タイポや命名の揺れが根絶される。
- **ゼロ・ボイラープレート**: 開発者はマーカークラスを宣言するだけで、必要なすべてのメタデータが型安全に提供される。
- **コンパイル時定数の活用**: 生成された `BlockIdentifier` は定数であるため、`ActivitySource` の初期化や属性の引数として直接使用できる。

### Negative
- **ジェネレータへの依存**: ビルドプロセスが Source Generator に依存するが、これは VK.Blocks の標準方針であり許容される。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ**: 識別子が一元化されることで、ログや監査トレースにおけるモジュール特定の信頼性が向上する。
- **パフォーマンス**: すべてのメタデータが定数として埋め込まれるため、実行時のオーバーヘッド（リフレクションや assembly スキャン）は一切発生しない。

**Last Updated**: 2026-04-24
**Status**: ✅ Accepted
