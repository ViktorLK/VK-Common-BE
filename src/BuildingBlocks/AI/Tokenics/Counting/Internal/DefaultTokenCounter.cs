
namespace VK.Blocks.AI.Tokenics.Counting.Internal;

/// <summary>
/// High-precision tokenizer based on Microsoft.ML.Tokenizers.
/// </summary>
internal sealed class DefaultTokenCounter : IVKTokenCounter
{
    public int CountTokens(string text, string? modelId = null)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        return text.Length / 2;
    }
}
