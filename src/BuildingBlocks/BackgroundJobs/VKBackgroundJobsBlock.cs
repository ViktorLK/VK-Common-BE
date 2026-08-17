using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Marker type for the BackgroundJobs building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKBackgroundJobsBlock
{
}
