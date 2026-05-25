using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// A generic interface to extract fragments from a specific source type.
/// </summary>
public interface IVKPromptExtractor<in TSource>
{
    VKResult<IReadOnlyList<VKPromptFragment>> Extract(TSource source, VKWeavingContext context);
}
