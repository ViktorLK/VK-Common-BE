namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Protocol for extracting JSON block content from raw LLM text responses.
/// </summary>
public interface IVKContractExtractor
{
    string ExtractJsonBlock(string rawText);
}
