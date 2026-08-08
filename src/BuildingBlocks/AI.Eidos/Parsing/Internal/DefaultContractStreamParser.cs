using System.Collections.Generic;
using System.Text.Json;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Parsing.Internal;

internal sealed class DefaultContractStreamParser : IVKContractStreamParser
{
    public VKAIEidosStreamChunk ParseChunk(string accumulatedText, VKAIEidosSchema schema)
    {
        VKGuard.NotNull(schema);
        if (string.IsNullOrWhiteSpace(accumulatedText))
        {
            return new VKAIEidosStreamChunk();
        }

        var availableProps = new Dictionary<string, object?>();
        bool isComplete = false;

        try
        {
            using var doc = JsonDocument.Parse(accumulatedText);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    availableProps[prop.Name] = prop.Value.Clone();
                }
                isComplete = true;
            }
        }
        catch (JsonException)
        {
            // Partial JSON
        }

        var chunkType = isComplete ? VKAIEidosChunkType.TextPart : VKAIEidosChunkType.DeltaPart;

        return new VKAIEidosStreamChunk
        {
            ChunkType = chunkType,
            DeltaText = accumulatedText,
            AvailableProperties = availableProps,
            IsComplete = isComplete
        };
    }
}
