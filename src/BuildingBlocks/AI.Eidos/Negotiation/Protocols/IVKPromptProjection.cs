using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKPromptProjection
{
    string ProjectToPrompt(VKAIEidosResponseContract contract, bool injectNarrativeField = false);
}
