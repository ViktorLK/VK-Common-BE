# ADR 009: Dynamic XML Wrapper Tagging in Knowledge Injection

- **Date**: 2026-06-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psyche の知識（Knowledge）注入ステージでは、ユーザーの入力に対して合致した知識エントリ（`VKKnowledgeEntry`）を Prompt の中に差し込む。従来は、これらの知識エントリをフォーマットする際、特定の固定タグ（例: `<knowledge>` または `<important_knowledge>`）が、オブジェクトの内部的な配置位置（絶対座標の Depth 指定の有無など）から暗黙的かつハードコード的に決定されていた。しかし、LLM によっては `<rules>`、`<lore>`、`<context>` といった個別のセマンティックタグを用いた方が、プロンプト指示の理解度や出力精度が著しく向上することが知られている。

## 2. Problem Statement (問題定義)

固定または暗黙的に決定される XML 包装タグの設計には、以下の問題がある：
1. **プロンプトエンジニアリングの制限**: LLM の特性（例: Claude は特定のタグ形式を好むなど）に応じた XML 構造のカスタマイズが不可能であり、プロンプトの最適化幅を狭めてしまう。
2. **意味的区別の喪失**: 世界観設定（Lore）、動的システムルール（System Rules）、および一時的な状況コンテキスト（Context）などの異なる性質のナレッジが、すべて同一の `<knowledge>` タグに混在して出力されるため、LLM が情報の優先順位を誤認識する原因となる。

```csharp
// 悪い例: 深さ(Depth)の有無だけでタグを暗黙決定し、カスタマイズが効かない
string tag = fragment.Depth is not null ? "important_knowledge" : "knowledge";
sb.AppendLine($"<{tag}>");
```

## 3. Decision (決定事項)

プロンプト内での知識表現のセマンティクス制御を柔軟にするため、**「Dynamic XML Wrapper Tagging (動的 XML 包装タグ付け)」**を採用する。

1. **`VKKnowledgeEntry` への `Tag` プロパティの追加**:
   - データモデルに任意の文字列を受け入れる `Tag` プロパティを追加する。デフォルト値は後方互換性を保つために `VKKnowledgeXmlTags.Knowledge` (`"knowledge"`) とする。
2. **フォーマッターの動的タグ抽出化**:
   - `DefaultKnowledgeFormatter` 内で、同一のタグ（`Tag`）および同一の配置座標を持つフラグメントをグループ化する。
   - 描画処理において、ハードコードされた定数ではなく、エントリのメタデータから抽出した `Tag` プロパティの値を直接 XML タグ名として出力する。

### 核心的なデータ構造とフォーマッター設計

```csharp
namespace VK.Blocks.AI.Psyche;

public static class VKKnowledgeXmlTags
{
    public const string Knowledge = "knowledge";
    public const string ImportantKnowledge = "important_knowledge";
}

public sealed record VKKnowledgeEntry : IVKFragmentMetadata
{
    public required string Id { get; init; }
    
    // 動的なXMLタグ名を指定可能 (e.g. "lore", "rules", "profile")
    public string Tag { get; init; } = VKKnowledgeXmlTags.Knowledge;
}

// フォーマッター内のレンダリングロジック
internal sealed class DefaultKnowledgeFormatter : IVKPromptFormatter
{
    private string FormatGroup(IReadOnlyList<VKPromptFragment> group)
    {
        var sb = new System.Text.StringBuilder();
        var firstEntry = (VKKnowledgeEntry)group[0].Metadata!;
        string tag = firstEntry.Tag; // エントリで定義されたカスタムタグを動的に抽出

        sb.AppendLine($"<{tag}>");
        foreach (var frag in group)
        {
            sb.AppendLine(frag.Content);
        }
        sb.AppendLine($"</{tag}>");
        return sb.ToString();
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Global Options Tag Mapping
- **Approach**: `VKKnowledgeOptions` 内で、エントリの種類に応じたグローバルなマッピングテーブルを用意してタグを引く。
- **Rejected Reason**: エントリごとに局所的・動的にタグを決定できず、また別のモジュールが介入して新しいタグ構造を定義する際の柔軟性に欠けるため。

### Option 2: Pre-defined Enum for Allowed Tags
- **Approach**: タグ名を文字列ではなく `enum VKKnowledgeTagType { Knowledge, Rules, Lore }` のように制限する。
- **Rejected Reason**: 新しいモデルの登場やユーザー独自のタグ（例: クライアント企業名タグ `<company_profile>` など）の追加のたびに Enum の定義変更と再コンパイルが必要になるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **セマンティックなプロンプト構築**: LLM に対して「これは背景世界設定である」「これは絶対に遵守すべきルールである」という性質の違いを、明瞭な XML 構造で伝えられるため、指示追従性能が向上する。
- **後方互換性**: デフォルト値が `"knowledge"` に設定されているため、既存のデータやコードを書き換えることなくシームレスに移行可能。

### Negative
- **XML 破壊のリスク**: `Tag` プロパティに XML の仕様上許容されない文字列（スペースや特殊文字、先頭の数字など）を設定された場合、生成されるプロンプトのマークアップ構造が壊れる。

### Mitigation
- エントリの作成・更新時のバリデーションロジック（`VKKnowledgeEntryValidator` など）において、`Tag` 属性値が安全な XML 識別子トークン（正規表現 `^[a-zA-Z_][a-zA-Z0-9_\-\.]*$`）に合致しているかを検証する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Sanitization**: プロンプト注入時のタグ生成処理において、悪意あるユーザーが Tag フィールドへインジェクションを試みることを防ぐため、バリデーションで英数字とハイフン・アンダースコア以外の入力を厳格に遮断する。

## 7. Status
✅ Accepted
