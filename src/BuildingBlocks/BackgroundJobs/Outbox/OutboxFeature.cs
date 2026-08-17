using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

[VKFeature(typeof(VKBackgroundJobsBlock), OptionsType = typeof(VKOutboxOptions))]
internal sealed partial class OutboxFeature
{
}
