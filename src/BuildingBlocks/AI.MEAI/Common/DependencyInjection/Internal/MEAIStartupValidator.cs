using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.MEAI.Common.DependencyInjection.Internal;

/// <summary>
/// Validates <see cref="VKAIMEAIOptions"/> resilience dependencies and time-budget invariants at startup.
/// </summary>
// [AP.01] [AP.04] [AP.06] [CS.07]
internal sealed class MEAIStartupValidator(IServiceProvider serviceProvider) : IValidateOptions<VKAIMEAIOptions>
{
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);

    public ValidateOptionsResult Validate(string? name, VKAIMEAIOptions options)
    {
        _ = name;
        VKGuard.NotNull(options);

        VKAIMEAITransportResilienceOptions? transport = options.TransportResilience;
        if (transport is null)
        {
            return ValidateOptionsResult.Fail("VKAIMEAIOptions.TransportResilience must not be null.");
        }

        List<string> failures = new();

        if (string.IsNullOrWhiteSpace(transport.PipelineName))
        {
            failures.Add("VKAIMEAIOptions.TransportResilience.PipelineName must not be empty.");
        }

        if (transport.MaxRetries < 0)
        {
            failures.Add("VKAIMEAIOptions.TransportResilience.MaxRetries must be greater than or equal to 0.");
        }

        if (transport.AttemptTimeout <= TimeSpan.Zero)
        {
            failures.Add("VKAIMEAIOptions.TransportResilience.AttemptTimeout must be greater than TimeSpan.Zero.");
        }

        if (transport.InitialRetryDelay < TimeSpan.Zero)
        {
            failures.Add("VKAIMEAIOptions.TransportResilience.InitialRetryDelay must be greater than or equal to TimeSpan.Zero.");
        }

        if (double.IsNaN(transport.BackoffMultiplier) ||
            double.IsInfinity(transport.BackoffMultiplier) ||
            transport.BackoffMultiplier < 1.0)
        {
            failures.Add("VKAIMEAIOptions.TransportResilience.BackoffMultiplier must be finite and at least 1.0.");
        }

        if (transport.MaxTotalAttempts < 1)
        {
            failures.Add("VKAIMEAIOptions.TransportResilience.MaxTotalAttempts must be at least 1.");
        }

        // [CS.07] Documented fallback when root VKAIOptions is omitted in standalone MEAI setups.
        VKAIOptions? aiOptions = _serviceProvider.GetService<IOptions<VKAIOptions>>()?.Value
            ?? _serviceProvider.GetService<VKAIOptions>();
        bool allowNoOp = transport.AllowNoOpResilience || (aiOptions?.AllowNoOpResilience == true);

        if (!allowNoOp)
        {
            IVKResilienceExecutor? executor = _serviceProvider.GetService<IVKResilienceExecutor>();
            if (executor is null || executor.IsNoOp)
            {
                failures.Add(
                    "VK.Blocks.AI.MEAI requires a non-NoOp IVKResilienceExecutor. Register VK.Blocks.Resilience or set AllowNoOpResilience = true.");
            }

            IVKPolicyRegistry? registry = _serviceProvider.GetService<IVKPolicyRegistry>();
            if (registry is null)
            {
                failures.Add(
                    "VK.Blocks.AI.MEAI requires IVKPolicyRegistry to configure outbound transport resilience pipelines.");
            }
        }

        if (transport.MaxRetries >= 0 && transport.AttemptTimeout > TimeSpan.Zero)
        {
            TimeSpan turnDeadline = aiOptions?.TurnDeadline ?? VKAIConstants.DefaultTurnDeadline;
            long singleConnectionAttempts = (long)transport.MaxRetries + 1;
            if ((Int128)transport.AttemptTimeout.Ticks * singleConnectionAttempts > turnDeadline.Ticks)
            {
                failures.Add(
                    $"Transport time budget violation: (MaxRetries + 1) * AttemptTimeout ({singleConnectionAttempts} * {transport.AttemptTimeout.TotalSeconds:0.###}s) exceeds TurnDeadline ({turnDeadline.TotalSeconds:0.###}s).");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
