using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Contracts;
using VK.Blocks.Authorization.Diagnostics.Models;
using VK.Blocks.Authorization.Features.Permissions;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Core.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Diagnostics;

/// <summary>
/// Provides centralized diagnostics and telemetry for the Authorization block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics<AuthorizationBlock>]
public static partial class AuthorizationDiagnostics
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
            AuthorizationDiagnosticsConstants.DecisionCounterName,
            description: AuthorizationDiagnosticsConstants.DecisionCounterDescription);

        _failureReasons = Meter.CreateCounter<long>(
            AuthorizationDiagnosticsConstants.FailureReasonsCounterName,
            description: AuthorizationDiagnosticsConstants.FailureReasonsCounterDescription);

        _evaluationDuration = Meter.CreateHistogram<double>(
            AuthorizationDiagnosticsConstants.EvaluationDurationName,
            unit: AuthorizationDiagnosticsConstants.EvaluationDurationUnit,
            description: AuthorizationDiagnosticsConstants.EvaluationDurationDescription);
    }

    /// <summary>
    /// Gets the compile-time authorization metadata (topology).
    /// </summary>
    /// <returns>A map of endpoint names to authorization information.</returns>
    public static IReadOnlyDictionary<string, EndpointAuthorizationInfo> GetAuthorizationMetadata()
        => AuthorizationMetadata.Endpoints;

    /// <summary>
    /// Gets the deterministic hash of the authorization metadata.
    /// </summary>
    /// <returns>A deterministic hash string representating the metadata state.</returns>
    public static string GetMetadataHash() => AuthorizationMetadata.MetadataHash;

    /// <summary>
    /// Gets runtime information about all registered authorization handlers and permission evaluators.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handler information.</param>
    /// <returns>A collection of registered authorization handlers.</returns>
    public static Task<IEnumerable<AuthorizationHandlerInfo>> GetRegisteredHandlersAsync(IServiceProvider serviceProvider)
    {
        var handlers = serviceProvider.GetServices<IAuthorizationHandler>();
        var evaluators = serviceProvider.GetServices<IPermissionEvaluator>();

        var result = new List<AuthorizationHandlerInfo>();

        foreach (var handler in handlers)
        {
            result.Add(new AuthorizationHandlerInfo
            {
                HandlerType = handler.GetType().Name,
                IsPermissionEvaluator = handler is IPermissionEvaluator,
                DisplayName = handler.GetType().Name.Replace("Handler", "")
            });
        }

        // Add evaluators that might not be registered as IAuthorizationHandler (rare but possible)
        foreach (var evaluator in evaluators)
        {
            if (result.All(r => r.HandlerType != evaluator.GetType().Name))
            {
                result.Add(new AuthorizationHandlerInfo
                {
                    HandlerType = evaluator.GetType().Name,
                    IsPermissionEvaluator = true,
                    DisplayName = evaluator.GetType().Name.Replace("Evaluator", "")
                });
            }
        }

        return Task.FromResult<IEnumerable<AuthorizationHandlerInfo>>(result);
    }

    /// <summary>
    /// Records an authorization decision.
    /// </summary>
    /// <param name="policyName">The name of the policy being evaluated.</param>
    /// <param name="isAllowed">Whether the authorization was successful.</param>
    internal static void RecordDecision(string policyName, bool isAllowed)
    {
        _authorizationDecisions.Add(1,
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagDecision, isAllowed ? "Allowed" : "Denied"));
    }

    /// <summary>
    /// Records the specific error code for an authorization failure.
    /// </summary>
    /// <param name="policyName">The name of the policy that failed.</param>
    /// <param name="error">The <see cref="Error"/> representing the failure.</param>
    internal static void RecordFailure(string policyName, Error error)
    {
        _failureReasons.Add(1,
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagErrorCode, error.Code));
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
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagPolicyName, policyName),
            new KeyValuePair<string, object?>(AuthorizationDiagnosticsConstants.TagDecision, isAllowed ? "Allowed" : "Denied"));
    }
}



