# ADR 003: Hierarchical Configuration Pattern

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI Configuration Design

## 1. Context (背景)

AI BuildingBlock は、Chat、Embeddings、Search といった複数の機能を内包しており、それぞれが独自のモデル名、エンドポイント、パラメータ（Temperature 等）の設定を必要とします。これらの膨大な設定項目を `appsettings.json` のフラットな構造で管理すると、設定の重複や命名の衝突が発生し、可読性とメンテナンス性が著しく低下します。.NET の Options パターンを最大限に活かし、クリーンで構造化された設定管理を実現するための標準が必要です。

## 2. Problem Statement (問題定義)

フラットな設定管理（例：`AI_Chat_Model`, `AI_Embeddings_Model`）には、以下の問題があります。

- **Naming Collisions**: 異なる機能間で同じプロパティ名（例：`Model`）を使用する際、プレフィックスが肥大化する。
- **Inconsistent Resolution**: 各機能が独自の方法で IConfiguration からセクションを読み取ると、バインドミスやデフォルト値の不整合が発生しやすい。
- **Poor Readability**: `appsettings.json` の視覚的な階層構造が、実際の C# Options クラスの構造と一致しない。

## 3. Decision (決定事項)

階層化されたセクションパターン（Hierarchical Section Pattern）を標準として採用します。

1. **Module Root Section**:
   - モジュールのルートセクションを `VK:AI` (または `VK.AI`) とする。
2. **Feature Nested Sections**:
   - 各機能はルートの下にネストされたセクションを持つ（例：`VK:AI:Chat`, `VK:AI:Embeddings`）。
3. **Primary Options (VKAIOptions)**:
   - `VKAIOptions` はモジュール全体の有効/無効を制御する `Enabled` フラグのみを保持する。
4. **Feature-Specific Options**:
   - `VKChatOptions` や `VKEmbeddingOptions` は独立したレコードとして定義され、DI 登録時に結合されたセクションパス（`VK:AI:Chat` 等）からバインドされる。
5. **Static SectionName Property**:
   - 各 Options レコードに `static string SectionName` プロパティを定義し、設定パスの単一情報源（Single Source of Truth）とする。

### 核心的なコードパターン

```csharp
public sealed record VKAIOptions : IVKBlockOptions
{
    public static string SectionName => "VK:AI";
    public bool Enabled { get; init; } = true;
}

public sealed record VKChatOptions : IVKBlockOptions
{
    // 親セクション名と結合して階層を表現
    public static string SectionName => VKAIOptions.SectionName + ":Chat";
    public string Model { get; init; } = "gpt-4";
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Flat Key-Value Pair
- **Approach**: すべてのプロパティを `VK:AI:ChatModel`, `VK:AI:EmbeddingsModel` のようにフラットに配置する。
- **Rejected Reason**: オプションクラスの `IOptionsSnapshot` を用いた部分的なバインドが難しく、構成が複雑になるにつれて管理が困難になるため。

### Option 2: Monolithic AI Options
- **Approach**: `VKAIOptions` クラスの中に `ChatOptions` クラスをプロパティとして含める。
- **Rejected Reason**: 不要な設定まで一度にメモリにロードされること、および特定の機能だけを DI 経由で受け取ることが難しくなる（ISP 違反）ため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Clear Hierarchy**: `appsettings.json` の構造が明確になり、人間にとっても理解しやすい。
- **Granular Injection**: 必要な機能のオプション（例：`IOptions<VKChatOptions>`）だけをサービスに注入できる。
- **Type Safety**: 強力な型付けと `IValidateOptions` による検証を階層ごとに適用できる。

### Negative
- **Path Verbosity**: 階層が深くなると、設定ファイル内でのインデントが深くなる。

### Mitigation
- 共通のベースパス（`VK:AI`）を変数化し、コード内での再利用を徹底することで、パスの不整合を防止する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Validation**: 各階層のオプションに対して `AIOptionsValidator` や `ChatOptionsValidator` を実装し、起動時に `ValidateOnStart` を実行する。
- **Sensitive Data**: API キーなどの機密情報は、この階層構造を維持したまま `secrets.json` や Azure Key Vault でオーバーライドすることを推奨する。

## 7. Status
✅ Accepted
