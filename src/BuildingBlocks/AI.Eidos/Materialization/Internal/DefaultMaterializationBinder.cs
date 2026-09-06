using System;
using System.Text.Json;
using VK.Blocks.AI.Eidos.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Materialization.Internal;

internal sealed class DefaultMaterializationBinder(IVKJsonSerializer jsonSerializer) : IVKMaterializationBinder // [AP.01]
{
    private readonly IVKJsonSerializer _jsonSerializer = VKGuard.NotNull(jsonSerializer);

    public string ExtractJsonBlock(string rawText)
    {
        return JsonBlockExtractor.ExtractJsonBlock(rawText);
    }

    public VKResult<T> Bind<T>(string rawJson, VKMaterializationToleranceMode toleranceMode = VKMaterializationToleranceMode.Strict) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(rawJson);

        if (toleranceMode != VKMaterializationToleranceMode.Strict)
        {
            // [CS.01] Result only, no throw in business flow
            return VKResult.Failure<T>(VKAIEidosErrors.Binding.ToleranceModeNotSupported(toleranceMode));
        }

        try
        {
            var cleanedJson = ExtractJsonBlock(rawJson);
            var result = _jsonSerializer.Deserialize<T>(cleanedJson);
            if (result is null)
            {
                return VKResult.Failure<T>(VKAIEidosErrors.Binding.NullResult);
            }

            return VKResult.Success(result);
        }
        catch (JsonException ex)
        {
            return VKResult.Failure<T>(VKAIEidosErrors.Binding.JsonError(ex.Message));
        }
    }

    public VKResult<object> Bind(string rawJson, Type targetType, VKMaterializationToleranceMode toleranceMode = VKMaterializationToleranceMode.Strict)
    {
        VKGuard.NotNullOrWhiteSpace(rawJson);
        VKGuard.NotNull(targetType);

        if (toleranceMode != VKMaterializationToleranceMode.Strict)
        {
            // [CS.01] Result only, no throw in business flow
            return VKResult.Failure<object>(VKAIEidosErrors.Binding.ToleranceModeNotSupported(toleranceMode));
        }

        try
        {
            var cleanedJson = ExtractJsonBlock(rawJson);
            // [CS.06] Use injected IVKJsonSerializer instead of raw JsonSerializer
            var result = _jsonSerializer.Deserialize(cleanedJson, targetType);
            if (result is null)
            {
                return VKResult.Failure<object>(VKAIEidosErrors.Binding.NullResult);
            }

            return VKResult.Success(result);
        }
        catch (JsonException ex)
        {
            return VKResult.Failure<object>(VKAIEidosErrors.Binding.JsonError(ex.Message));
        }
    }
}
