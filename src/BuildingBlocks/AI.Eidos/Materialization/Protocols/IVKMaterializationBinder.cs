using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Binds raw validated JSON content into strongly-typed C# contract models with tolerance support.
/// Includes integrated raw text extraction. Complies with [AP.03].
/// </summary>
public interface IVKMaterializationBinder
{
    /// <summary>
    /// Extracts raw JSON block from LLM markdown response or surrounding text.
    /// </summary>
    string ExtractJsonBlock(string rawText);

    /// <summary>
    /// Binds the given JSON string into the target model type T.
    /// </summary>
    VKResult<T> Bind<T>(string rawJson, VKMaterializationToleranceMode toleranceMode = VKMaterializationToleranceMode.Strict) where T : class;

    /// <summary>
    /// Binds the given JSON string into the specified runtime target type.
    /// </summary>
    VKResult<object> Bind(string rawJson, Type targetType, VKMaterializationToleranceMode toleranceMode = VKMaterializationToleranceMode.Strict);
}
