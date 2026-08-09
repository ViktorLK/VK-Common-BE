using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKToolProjection
{
    IVKAtomicTool ProjectToTool(VKAIEidosResponseContract contract, bool injectNarrativeField = false, bool allowSegmentation = true);
}
