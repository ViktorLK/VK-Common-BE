using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKMultiTenancyOptions : IVKBlockOptions
{
    public bool RestoreTenantContext { get; init; } = true;
}
