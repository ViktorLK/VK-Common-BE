using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Options for Eidos Registration Feature slice.
/// </summary>
public sealed partial record VKSchemaOptions : IVKBlockOptions
{
    public bool EnableAutoMigration { get; init; } = true;
    public bool InjectSchemaHeader { get; init; } = true;
    public bool EnableSchemaCache { get; init; } = true;
    public string SchemaIdBaseUri { get; init; } = "https://vkblocks.io/schemas/";
}
