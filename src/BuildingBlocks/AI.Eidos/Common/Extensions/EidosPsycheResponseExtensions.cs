using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Extension methods for retrieving Eidos structured contract models from VKPsycheResponse metadata.
/// Follows AP.03 (Public extensions in root namespace).
/// </summary>
public static class EidosPsycheResponseExtensions
{
    /// <summary>
    /// Well-known metadata key where AI.Eidos stores the materialized contract model or envelope.
    /// </summary>
    public const string ModelResultMetadataKey = "vk.eidos.model";

    /// <summary>
    /// Safely retrieves the structured model result produced by AI.Eidos from the response metadata.
    /// Supports direct instances as well as envelope-wrapped models.
    /// </summary>
    /// <typeparam name="TDto">The expected DTO type.</typeparam>
    /// <param name="response">The Psyche pipeline response.</param>
    /// <returns>The casted DTO instance if found and valid; otherwise, <c>null</c>.</returns>
    public static TDto? GetEidosModel<TDto>(this VKPsycheResponse response) where TDto : class
    {
        if (response is null)
        {
            return null;
        }

        if (response.Metadata.TryGetValue(ModelResultMetadataKey, out var obj) && obj is not null)
        {
            if (obj is TDto direct)
            {
                return direct;
            }

            var modelProp = obj.GetType().GetProperty("Model");
            if (modelProp?.GetValue(obj) is TDto envelopeModel)
            {
                return envelopeModel;
            }
        }

        return null;
    }
}
