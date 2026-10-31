# ADR 002: Polymorphic Multi-modal Message Schema

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI Multi-modal Support

## 1. Context (背景)

現代の AI アプリケーションは、テキストのみの対話を超えて進化しています。主要な LLM プロバイダー（OpenAI, Anthropic, Gemini 等）は、画像、音声、ドキュメントといったマルチモーダルな入力と出力を標準でサポートするようになっています。VK.Blocks.AI において、これらの多様なデータ形式を統一的、かつ拡張可能な方法で表現することは、ライブラリの汎用性を確保するために極めて重要です。

## 2. Problem Statement (問題定義)

メッセージを単なる文字列（`string`）として扱う従来の設計には、以下の限界があります。

- **Inflexibility**: 画像や音声などの非テキストデータを埋め込むための標準的な方法が欠如している。
- **Vendor Dependency**: 各プロバイダーが独自のマルチモーダル形式（OpenAI の `content` 配列等）を持っており、それらに直接依存するとポータビリティが損なわれる。
- **Complexity**: 複数のデータソース（URL, Base64, バイナリ等）を混在させる際のデータ管理が複雑になる。

## 3. Decision (決定事項)

`VKChatMessage` と `IVKChatMessagePart` を用いたポリモーフィックなメッセージ構造を導入します。

1. **Root Container**:
   - `VKChatMessage` クラスをルートコンテナとし、メッセージのロール（User, Assistant, System）と `IVKChatMessagePart` のコレクションを保持する。
2. **Polymorphic Parts**:
   - すべてのコンテンツタイプの基底インターフェースとして `IVKChatMessagePart` を定義する。
3. **Standard Implementations**:
   - 以下の標準パーツを提供する：
     - `VKTextPart`: プレーンテキスト。
     - `VKImagePart`: 画像データ（URI または Base64）。
     - `VKAudioPart`: 音声データ。
     - `VKFilePart`: ドキュメントやその他の添付ファイル。
4. **Vendor-Neutral Mapping**:
   - このスキーマはプロバイダーに依存しない形式で定義され、各 Engine 実装内でプロバイダー固有のペイロード（例：OpenAI の `UserMessage` クラス）に変換される。

### 核心的なデータ構造

```csharp
public interface IVKChatMessagePart
{
    string Type { get; }
}

public sealed record VKTextPart(string Text) : IVKChatMessagePart
{
    public string Type => "text";
}

public sealed class VKChatMessage
{
    public VKChatRole Role { get; init; }
    public IList<IVKChatMessagePart> Parts { get; } = new List<IVKChatMessagePart>();
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Base64 String Encoding in Text
- **Approach**: 画像などのバイナリデータを Base64 に変換し、テキストメッセージ内に特定のタグ（例：`<img src="...">`）で埋め込む。
- **Rejected Reason**: パース処理が複雑になり、バイナリデータの効率的なストリーミングやキャッシュが困難になるため。

### Option 2: Strictly Typed Multi-modal Message
- **Approach**: `VKChatMessage` を継承して `VKImageMessage` や `VKAudioMessage` を作成する。
- **Rejected Reason**: 1つのメッセージ内にテキストと画像が混在する（Mixed Content）ケースを柔軟に表現できないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Extensibility**: 将来的な新しいメディアタイプ（ビデオ、3D モデル等）に対しても、新しい Part クラスを追加するだけで対応可能。
- **Uniform Handling**: アプリケーションレイヤーは、プロバイダーがマルチモーダルをどのようにパースするかを気にせずにメッセージを構築できる。

### Negative
- **Serialization Complexity**: インターフェースのコレクションをシリアライズ/デシリアライズする際に、ポリモーフィックな型情報の処理が必要になる。

### Mitigation
- `IVKJsonSerializer` において、`Type` プロパティに基づいた型判別ロジックを共通化し、シリアライズの複雑さを隠蔽する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Input Validation**: `VKImagePart` 等を受け取る際、URI のサニタイズやファイルサイズの制限を `VKGuard` で検証する。
- **Data Protection**: 感密な情報を含む可能性のあるファイルや画像のバイナリデータは、ログ出力時に適切にマスキングされるように `Diagnostics` 側で制御する。

## 7. Status
✅ Accepted
