using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// Base class for all Semantic Kernel based engines.
/// Provides common orchestration, observability, and resilience logic.
/// </summary>
/// <typeparam name="TOptions">The feature options type.</typeparam>
internal abstract class AISKEngineBase<TOptions> : AISKProviderBase
    where TOptions : class, IVKAIProviderOptions, IVKAIGovernanceOptions, IVKToggleableBlockOptions, new()
{
    protected VKAIDefaultsOptions GlobalOptions { get; }
    protected TOptions FeatureOptions { get; }
    protected ILogger Logger { get; }
    protected TimeProvider TimeProvider { get; }

    protected AISKEngineBase(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIDefaultsOptions> globalOptions,
        IOptions<TOptions> featureOptions,
        ILogger logger,
        TimeProvider? timeProvider = null)
        : base(kernel, featureOptions.Value.ModelId ?? "Unknown")
    {
        GlobalOptions = VKGuard.NotNull(globalOptions?.Value);
        FeatureOptions = VKGuard.NotNull(featureOptions?.Value);
        Logger = VKGuard.NotNull(logger);
        TimeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Gets a service from the kernel.
    /// </summary>
    protected TService GetService<TService>(string? serviceId = null) where TService : class
    {
        return Kernel.GetRequiredService<TService>(serviceId);
    }

    /// <summary>
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
        using var timeoutCts = new CancellationTokenSource(timeout, TimeProvider);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            var result = await action(linkedCts.Token).ConfigureAwait(false);
            return VKResult.Success(result);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
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
    /// Executes a streaming operation with feature enablement check and timeout.
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
        using var timeoutCts = new CancellationTokenSource(timeout, TimeProvider);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        IAsyncEnumerator<T>? enumerator = null;
        VKError? initError = null;

        try
        {
            enumerator = action(linkedCts.Token).GetAsyncEnumerator(linkedCts.Token);
        }
        catch (Exception ex)
        {
            initError = AISKErrorMapper.Map(ex);
        }

        if (initError is not null)
        {
            yield return VKResult.Failure<T>(initError);
            yield break;
        }

        VKError? loopError = null;
        while (true)
        {
            T? item = default;
            try
            {
                if (!await enumerator!.MoveNextAsync().ConfigureAwait(false))
                {
                    break;
                }
                item = enumerator.Current;
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                Logger.LogExecutionError(new TimeoutException($"AI operation timed out after {timeout.TotalSeconds}s"), "AI operation timed out.");
                loopError = VKAIErrors.Timeout;
                break;
            }
            catch (Exception ex)
            {
                Logger.LogExecutionError(ex, ex.Message);
                loopError = AISKErrorMapper.Map(ex);
                break;
            }

            yield return VKResult.Success(item!);
        }

        if (loopError is not null)
        {
            yield return VKResult.Failure<T>(loopError);
        }

        if (enumerator is not null)
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the effective timeout from arguments or options.
    /// </summary>
    protected TimeSpan GetEffectiveTimeout(IVKAIArgs? args)
    {
        return args?.Timeout
            ?? FeatureOptions.Timeout
            ?? GlobalOptions.Timeout;
    }

    /// <summary>
    /// Gets the effective model ID from arguments or options.
    /// </summary>
    protected string? GetEffectiveModelId(IVKAIArgs? args)
    {
        return (args as IVKAIProviderOverrides)?.ModelId
            ?? FeatureOptions.ModelId;
    }

    /// <summary>
    /// Gets the effective audit enablement from arguments or options.
    /// </summary>
    protected bool GetEffectiveEnableAudit()
    {
        return FeatureOptions.EnableAudit ?? GlobalOptions.EnableAudit;
    }
}
