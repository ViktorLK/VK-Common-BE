using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

/// <summary>
/// Base class for Semantic Kernel based AI engines with enhanced industrial capabilities.
/// </summary>
/// <typeparam name="TOptions">The specific feature options type.</typeparam>
internal abstract class AISKEngineBase<TOptions>
    where TOptions : class, IVKToggleableBlockOptions, IVKAIProviderSettings, IVKAIGovernanceSettings
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
        => (args as IVKAIProviderOverrides)?.Provider ?? FeatureOptions.Provider ?? GlobalOptions.Provider;

    /// <summary>
    /// Calculates the effective model identifier using: Args -> Feature priority.
    /// </summary>
    protected string? GetEffectiveModelId(IVKAIArgs? args)
        => (args as IVKAIProviderOverrides)?.ModelId ?? FeatureOptions.ModelId;

    /// <summary>
    /// Calculates the effective audit enabling flag using: Feature -> Global priority.
    /// </summary>
    protected bool GetEffectiveEnableAudit()
        => FeatureOptions.EnableAudit ?? GlobalOptions.EnableAudit;

    /// <summary>
    /// Wraps an AI operation with unified error handling, logging, and timeout management.
    /// Checks if the feature is enabled before execution and applies the effective timeout.
    /// </summary>
    protected async Task<VKResult<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        IVKAIArgs? args,
        VKError disabledError,
        CancellationToken cancellationToken = default)
    {
        if (!FeatureOptions.Enabled)
        {
            return VKResult.Failure<T>(disabledError);
        }

        var timeout = GetEffectiveTimeout(args);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            var result = await action(cts.Token).ConfigureAwait(false);
            return VKResult.Success(result);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            Logger.LogExecutionError(new TimeoutException($"AI operation timed out after {timeout.TotalSeconds}s"), "AI operation timed out.");
            return VKResult.Failure<T>(VKAIErrors.Timeout);
        }
        catch (Exception ex)
        {
            Logger.LogExecutionError(ex, ex.Message);
            return VKResult.Failure<T>(AISKErrorMapper.Map(ex));
        }
    }

    /// <summary>
    /// Wraps a streaming AI operation with timeout management.
    /// Checks if the feature is enabled before execution.
    /// </summary>
    protected async IAsyncEnumerable<VKResult<T>> ExecuteStreamingAsync<T>(
        Func<CancellationToken, IAsyncEnumerable<T>> action,
        IVKAIArgs? args,
        VKError disabledError,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!FeatureOptions.Enabled)
        {
            yield return VKResult.Failure<T>(disabledError);
            yield break;
        }

        var timeout = GetEffectiveTimeout(args);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        IAsyncEnumerator<T>? enumerator = null;
        VKError? initError = null;

        try
        {
            enumerator = action(cts.Token).GetAsyncEnumerator(cts.Token);
        }
        catch (Exception ex)
        {
            Logger.LogExecutionError(ex, ex.Message);
            initError = AISKErrorMapper.Map(ex);
        }

        if (initError != null)
        {
            yield return VKResult.Failure<T>(initError);
            yield break;
        }

        if (enumerator == null)
            yield break;

        await using (enumerator.ConfigureAwait(false))
        {
            while (true)
            {
                T? item = default;
                VKError? iterationError = null;

                try
                {
                    if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        break;
                    }
                    item = enumerator.Current;
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    Logger.LogExecutionError(new TimeoutException($"AI streaming operation timed out after {timeout.TotalSeconds}s"), "AI streaming operation timed out.");
                    iterationError = VKAIErrors.Timeout;
                }
                catch (Exception ex)
                {
                    Logger.LogExecutionError(ex, ex.Message);
                    iterationError = AISKErrorMapper.Map(ex);
                }

                if (iterationError != null)
                {
                    yield return VKResult.Failure<T>(iterationError);
                    yield break;
                }

                if (item != null)
                {
                    yield return VKResult.Success(item);
                }
            }
        }
    }
}
