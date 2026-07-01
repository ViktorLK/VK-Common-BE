using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Represents a delegate for the next execution in a pipeline.
/// </summary>
public delegate Task<VKResult> VKPipelineDelegate();
