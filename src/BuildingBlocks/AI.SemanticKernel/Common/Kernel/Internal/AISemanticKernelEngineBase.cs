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
internal abstract class AISemanticKernelEngineBase<TOptions> : AISemanticKernelProviderBase
    where TOptions : class, IVKAIProviderOptions, IVKToggleableBlockOptions, new()
{
    protected VKAIOptions GlobalOptions { get; }
    protected TOptions FeatureOptions { get; }
    protected ILogger Logger { get; }
    protected TimeProvider TimeProvider { get; }

    protected AISemanticKernelEngineBase(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIOptions> globalOptions,
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
    /// Checks if the feature is enabled before execution.
    /// </summary>
    protected async Task<VKResult<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        VKError disabledError,
        CancellationToken cancellationToken = default)
    {
        if (!FeatureOptions.Enabled)
        {
            return VKResult.Failure<T>(disabledError);
        }

        try
        {
            var result = await action(cancellationToken).ConfigureAwait(false);
            return VKResult.Success(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogExecutionError(ex, ex.Message);
            return VKResult.Failure<T>(AISemanticKernelErrorMapper.Map(ex));
        }
    }

    /// <summary>
    /// Overload allowing legacy 4-argument calls with args.
    /// </summary>
    protected Task<VKResult<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        object? args,
        VKError disabledError,
        CancellationToken cancellationToken = default)
    {
        _ = args;
        return ExecuteAsync(action, disabledError, cancellationToken);
    }

    /// <summary>
    /// Executes a streaming operation with feature enablement check.
    /// </summary>
    protected async IAsyncEnumerable<VKResult<T>> ExecuteStreamingAsync<T>(
        Func<CancellationToken, IAsyncEnumerable<T>> action,
        VKError disabledError,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!FeatureOptions.Enabled)
        {
            yield return VKResult.Failure<T>(disabledError);
            yield break;
        }

        IAsyncEnumerator<T>? enumerator = null;
        VKError? initError = null;

        try
        {
            enumerator = action(cancellationToken).GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            initError = AISemanticKernelErrorMapper.Map(ex);
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogExecutionError(ex, ex.Message);
                loopError = AISemanticKernelErrorMapper.Map(ex);
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
    /// Overload allowing legacy 4-argument streaming calls with args.
    /// </summary>
    protected IAsyncEnumerable<VKResult<T>> ExecuteStreamingAsync<T>(
        Func<CancellationToken, IAsyncEnumerable<T>> action,
        object? args,
        VKError disabledError,
        CancellationToken cancellationToken = default)
    {
        _ = args;
        return ExecuteStreamingAsync(action, disabledError, cancellationToken);
    }

    /// <summary>
    /// Gets whether audit is enabled.
    /// </summary>
    protected bool GetEffectiveEnableAudit()
    {
        return (FeatureOptions as IVKAIGovernanceOptions)?.EnableAudit ?? false;
    }
}
