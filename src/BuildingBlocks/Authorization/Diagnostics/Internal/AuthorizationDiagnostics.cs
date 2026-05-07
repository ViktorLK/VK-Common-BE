using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Diagnostics.Internal;

/// <summary>
/// Provides centralized diagnostics and telemetry for the Authorization block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockDiagnostics<VKAuthorizationBlock>]
internal static partial class AuthorizationDiagnostics
{
    private static readonly Counter<long> _authorizationDecisions;

    private static readonly Counter<long> _failureReasons;

    private static readonly Histogram<double> _evaluationDuration;

    /// <summary>
    /// Initializes static members of the <see cref="AuthorizationDiagnostics"/> class.
    /// </summary>
    static AuthorizationDiagnostics()
    {
        _authorizationDecisions = Meter.CreateCounter<long>(
            VKAuthorizationDiagnosticsConstants.DecisionCounterName,
            description: VKAuthorizationDiagnosticsConstants.DecisionCounterDescription);

        _failureReasons = Meter.CreateCounter<long>(
            VKAuthorizationDiagnosticsConstants.FailureReasonsCounterName,
            description: VKAuthorizationDiagnosticsConstants.FailureReasonsCounterDescription);

        _evaluationDuration = Meter.CreateHistogram<double>(
            VKAuthorizationDiagnosticsConstants.EvaluationDurationName,
            unit: VKAuthorizationDiagnosticsConstants.EvaluationDurationUnit,
            description: VKAuthorizationDiagnosticsConstants.EvaluationDurationDescription);
    }

    /// <summary>
    /// Gets the compile-time authorization metadata (topology).
    /// </summary>
    /// <returns>A map of endpoint names to authorization information.</returns>
    public static IReadOnlyDictionary<string, VKEndpointAuthorizationInfo> GetAuthorizationMetadata()
        => AuthorizationMetadata.Endpoints;

    /// <summary>
    /// Gets the deterministic hash of the authorization metadata.
    /// </summary>
    /// <returns>A deterministic hash string representating the metadata state.</returns>
    public static string GetMetadataHash() => AuthorizationMetadata.MetadataHash;

    /// <summary>
    /// Gets runtime information about all registered authorization handlers and VKPermission evaluators.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handler information.</param>
    /// <returns>A collection of registered authorization handlers.</returns>
    public static Task<IEnumerable<VKAuthorizationHandlerInfo>> GetRegisteredHandlersAsync(IServiceProvider serviceProvider)
    {
        var handlers = serviceProvider.GetServices<IAuthorizationHandler>();
        var evaluators = serviceProvider.GetServices<IVKPermissionEvaluator>();

        var result = new List<VKAuthorizationHandlerInfo>();

        foreach (var handler in handlers)
        {
            result.Add(new VKAuthorizationHandlerInfo
            {
                HandlerType = handler.GetType().Name,
                IsPermissionEvaluator = handler is IVKPermissionEvaluator,
                DisplayName = handler.GetType().Name.Replace("Handler", "")
            });
        }

        // Add evaluators that might not be registered as IAuthorizationHandler (rare but possible)
        foreach (var evaluator in evaluators)
        {
            if (result.All(r => r.HandlerType != evaluator.GetType().Name))
            {
                result.Add(new VKAuthorizationHandlerInfo
                {
                    HandlerType = evaluator.GetType().Name,
                    IsPermissionEvaluator = true,
                    DisplayName = evaluator.GetType().Name.Replace("Evaluator", "")
                });
            }
        }

        return Task.FromResult<IEnumerable<VKAuthorizationHandlerInfo>>(result);
    }

    /// <summary>
    /// Records an authorization decision.
    /// </summary>
    /// <param name="policyName">The name of the policy being evaluated.</param>
    /// <param name="isAllowed">Whether the authorization was successful.</param>
    internal static void RecordDecision(string policyName, bool isAllowed)
    {
        _authorizationDecisions.Add(1,
            new KeyValuePair<string, object?>(VKAuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(VKAuthorizationDiagnosticsConstants.TagDecision, isAllowed ? "Allowed" : "Denied"));
    }

    /// <summary>
    /// Records the specific error code for an authorization failure.
    /// </summary>
    /// <param name="policyName">The name of the policy that failed.</param>
    /// <param name="error">The <see cref="VKError"/> representing the failure.</param>
    internal static void RecordFailure(string policyName, VKError error)
    {
        _failureReasons.Add(1,
            new KeyValuePair<string, object?>(VKAuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(VKAuthorizationDiagnosticsConstants.TagErrorCode, error.Code));
    }

    /// <summary>
    /// Records the duration of an authorization evaluation.
    /// </summary>
    /// <param name="policyName">The name of the policy being evaluated.</param>
    /// <param name="durationMs">The duration in milliseconds.</param>
    /// <param name="isAllowed">Whether the authorization was successful.</param>
    internal static void RecordEvaluationDuration(string policyName, double durationMs, bool isAllowed)
    {
        _evaluationDuration.Record(durationMs,
            new KeyValuePair<string, object?>(VKAuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(VKAuthorizationDiagnosticsConstants.TagDecision, isAllowed ? "Allowed" : "Denied"));
    }
}
