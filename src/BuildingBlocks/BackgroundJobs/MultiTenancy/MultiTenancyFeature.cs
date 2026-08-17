using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

[VKFeature(typeof(VKBackgroundJobsBlock), OptionsType = typeof(VKMultiTenancyOptions))]
internal sealed partial class MultiTenancyFeature
{
}
