using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

[VKFeature(typeof(VKBackgroundJobsBlock), OptionsType = typeof(VKConcurrencyOptions))]
internal sealed partial class ConcurrencyFeature
{
}
