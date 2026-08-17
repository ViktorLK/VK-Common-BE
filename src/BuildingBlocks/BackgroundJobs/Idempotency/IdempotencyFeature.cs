using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

[VKFeature(typeof(VKBackgroundJobsBlock), OptionsType = typeof(VKIdempotencyOptions))]
internal sealed partial class IdempotencyFeature
{
}
