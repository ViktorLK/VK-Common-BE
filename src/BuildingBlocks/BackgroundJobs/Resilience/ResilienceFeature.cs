using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

[VKFeature(typeof(VKBackgroundJobsBlock), OptionsType = typeof(VKResilienceOptions))]
internal sealed partial class ResilienceFeature
{
}
