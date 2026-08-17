using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Core background job interface.
/// </summary>
public interface IVKJob<in TData>
{
    Task<VKJobExecutionResult> ExecuteAsync(TData data, VKJobContext context, CancellationToken ct);
}

/// <summary>
/// Alias contract for background job handler.
/// </summary>
public interface IVKJobHandler<in TData> : IVKJob<TData>
{
}
