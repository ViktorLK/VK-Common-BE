using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

/// <summary>
/// Base class for Semantic Kernel based AI engines with enhanced industrial capabilities.
/// </summary>
/// <typeparam name="TOptions">The specific feature options type.</typeparam>
internal abstract class AISKEngineBase<TOptions>
    where TOptions : class, IVKBlockOptions, IVKAIConnectionSettings, IVKAIGovernanceSettings
{
    protected Microsoft.SemanticKernel.Kernel Kernel { get; }
    protected VKAIOptions GlobalOptions { get; }
    protected TOptions FeatureOptions { get; }
    protected ILogger Logger { get; }

    protected AISKEngineBase(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIOptions> globalOptions,
        IOptions<TOptions> featureOptions,
        ILogger logger)
    {
        Kernel = VKGuard.NotNull(kernel);
        GlobalOptions = VKGuard.NotNull(globalOptions?.Value);
        FeatureOptions = VKGuard.NotNull(featureOptions?.Value);
        Logger = VKGuard.NotNull(logger);
    }

    /// <summary>
    /// Resolves a service from the Kernel, supporting keyed services.
    /// </summary>
    protected T GetService<T>(string? serviceId = null) where T : class
        => serviceId is not null
            ? Kernel.GetRequiredService<T>(serviceId)
            : Kernel.GetRequiredService<T>();

    /// <summary>
    /// Calculates the effective timeout using: Args -> Feature -> Global priority.
    /// </summary>
    protected TimeSpan GetEffectiveTimeout(IVKAIArgs? args)
        => args?.Timeout ?? FeatureOptions.Timeout ?? GlobalOptions.Timeout;

    /// <summary>
    /// Calculates the effective retry count.
    /// </summary>
    protected int GetEffectiveRetryCount(IVKAIArgs? args)
        => FeatureOptions.RetryCount ?? GlobalOptions.RetryCount;

    /// <summary>
    /// Calculates the effective provider using: Args -> Feature -> Global priority.
    /// </summary>
    protected VKAIProviderType GetEffectiveProvider(IVKAIArgs? args)
        => (args as IVKAIConnectionSettings)?.Provider ?? FeatureOptions.Provider ?? GlobalOptions.Provider;

    /// <summary>
    /// Calculates the effective model identifier using: Args -> Feature priority.
    /// </summary>
    protected string? GetEffectiveModelId(IVKAIArgs? args)
        => (args as IVKAIConnectionSettings)?.ModelId ?? FeatureOptions.ModelId;

    /// <summary>
    /// Calculates the effective audit enabling flag using: Feature -> Global priority.
    /// </summary>
    protected bool GetEffectiveEnableAudit()
        => FeatureOptions.EnableAudit ?? GlobalOptions.EnableAudit;

    /// <summary>
    /// Wraps an AI operation with unified error handling and logging.
    /// </summary>
    protected async Task<VKResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action().ConfigureAwait(false);
            return VKResult.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogExecutionError(ex, ex.Message);
            return VKResult.Failure<T>(AISKErrorMapper.Map(ex));
        }
    }
}
