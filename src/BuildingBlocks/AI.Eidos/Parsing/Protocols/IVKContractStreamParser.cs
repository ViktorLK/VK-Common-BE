using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractStreamParser
{
    VKAIEidosStreamChunk ParseChunk(string accumulatedText, VKAIEidosSchema schema);
}
