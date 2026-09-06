using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Protocol for incremental streaming parsing of JSON chunks.
/// Complies with [AP.03] & [BB.01].
/// </summary>
public interface IVKStreamingParser
{
    VKStreamingChunk ParseChunk(string accumulatedText, VKAIEidosSchema schema);
}
