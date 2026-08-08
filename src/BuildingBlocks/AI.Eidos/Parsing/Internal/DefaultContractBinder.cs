using System.Text.Json;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Parsing.Internal;

internal sealed class DefaultContractBinder : IVKContractBinder
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public VKResult<T> Bind<T>(string rawJson) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(rawJson);

        try
        {
            var obj = JsonSerializer.Deserialize<T>(rawJson, DefaultSerializerOptions);
            if (obj is null)
            {
                return VKResult.Failure<T>(VKError.Validation("Eidos.BindingNull", "Deserialized contract object is null."));
            }
            return VKResult.Success(obj);
        }
        catch (JsonException ex)
        {
            return VKResult.Failure<T>(VKError.Validation("Eidos.BindingJsonError", $"Failed to bind JSON to contract object: {ex.Message}"));
        }
    }
}
