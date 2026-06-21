using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Represents a delegate for the next execution in a pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public delegate Task<VKResult<TResponse>> VKPipelineDelegate<TResponse>();
