using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Contract for compressing or extracting sentence-window context around a matched offset.
/// </summary>
public interface IVKContextCompressor
{
    /// <summary>
    /// Compresses parent text by extracting a sentence window around the specified character offset range.
    /// </summary>
    /// <param name="parentContent">The raw full text of the parent document/chunk.</param>
    /// <param name="startOffset">The start character offset of the hit chunk.</param>
    /// <param name="endOffset">The end character offset of the hit chunk.</param>
    /// <param name="sentenceWindow">The number of sentences to expand before and after the hit range. Default is 2.</param>
    /// <returns>A result containing the compressed context text.</returns>
    VKResult<string> Compress(string parentContent, int startOffset, int endOffset, int sentenceWindow = 2);
}
