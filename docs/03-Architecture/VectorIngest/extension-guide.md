# Extension Guide: VectorIngest Subsystem

- **Scope**: `VK.Blocks.VectorIngest`
- **Specification Type**: Custom Parser & Chunker Implementation Guide (`ARCH-11`, `AP.06`)
- **Status**: ✅ Active

---

## 1. 🔌 カスタム DocumentParser の実装手順

新規ファイル形式（例: Word ドキュメント `.docx`）をサポートするには、`IVKDocumentParser` を実装し、サービスコレクションに登録します。

### 1.1 ゴールデンコード例: カスタム DOCX パーサー

```csharp
namespace VK.Blocks.VectorIngest.Custom;

public sealed class DocxDocumentParser : IVKDocumentParser
{
    public IReadOnlyList<string> SupportedMimeTypes { get; } = 
    [
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".docx"
    ];

    public async ValueTask<VKResult<VKParsedDocument>> ParseAsync(
        Stream documentStream,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(documentStream);

        try
        {
            // ストリームからの非同期構文解析処理
            string extractedText = await ExtractTextFromDocxAsync(documentStream, cancellationToken).ConfigureAwait(false);
            
            var parsed = new VKParsedDocument(
                Title: "Untitled Document",
                Content: extractedText,
                Metadata: new Dictionary<string, string>());

            return VKResult<VKParsedDocument>.Success(parsed);
        }
        catch (Exception ex)
        {
            return VKResult<VKParsedDocument>.Failure(
                VKVectorIngestErrors.DocumentParsingFailed.WithException(ex));
        }
    }

    private static ValueTask<string> ExtractTextFromDocxAsync(Stream stream, CancellationToken ct)
    {
        return ValueTask.FromResult("Extracted text...");
    }
}
```

---

## 2. ⚠️ 頻出アンチパターン

1. **ストリーム全体のメモリバッファ化 (Anti-Pattern)**:
   - ❌ `new MemoryStream()` に巨大なファイルをすべて読み込んでからパースする（OOM の原因）。
   - ⭕ `StreamReader` や SAX 方式のパーサーを使用し、ストリーミングで処理する。
2. **ColBERT 多ベクトルの無差別有効化 (Anti-Pattern)**:
   - ❌ すべてのチャンクに対して `GenerateMultiVectorEmbeddingsAsync` を実行する（ストレージが数百倍に膨張）。
   - ⭕ `EnableMultiVector` オプションが真である場合に限定し、かつ重要なドキュメント群にのみ適用する。

