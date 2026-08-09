using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKSchemaProjection
{
    string ProjectToSchema(VKAIEidosResponseContract contract, bool injectNarrativeField = false, bool allowSegmentation = true);
}
