using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.DependencyInjection;
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Common;

/// <summary>
/// Transition-layer extensions for simplifying authorization execution.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Checks if the user is a SuperAdmin based on global configuration.
    /// </summary>
    public static bool IsSuperAdmin(this ClaimsPrincipal user, VKAuthorizationOptions options)
    {
        return !string.IsNullOrEmpty(options.SuperAdminRole) && 
               user.HasClaim(options.RoleClaimType, options.SuperAdminRole);
    }

    /// <summary>
    /// Stops the stopwatch and records decision and duration to diagnostics.
    /// </summary>
    public static void RecordEvaluation(this Stopwatch sw, string policyName, Result<bool> result)
    {
        sw.Stop();
        var isAllowed = result.IsSuccess && result.Value;
        
        AuthorizationDiagnostics.RecordDecision(policyName, isAllowed);
        AuthorizationDiagnostics.RecordEvaluationDuration(policyName, sw.Elapsed.TotalMilliseconds, isAllowed);
        
        if (!isAllowed && !result.IsSuccess)
        {
            AuthorizationDiagnostics.RecordFailure(policyName, result.FirstError);
        }
    }

    /// <summary>
    /// Maps a <see cref="Result{T}"/> to the standard ASP.NET Core authorization context.
    /// </summary>
    public static void ApplyResult(
        this AuthorizationHandlerContext context,
        IVKAuthorizationRequirement requirement,
        Result<bool> result,
        IAuthorizationHandler handler)
    {
        if (result.IsSuccess && result.Value)
        {
            context.Succeed(requirement);
        }
        else if (result.IsSuccess)
        {
            // Successful evaluation but logical access denied
            context.Fail(new AuthorizationFailureReason(handler, requirement.DefaultError.Description));
        }
        else
        {
            // Evaluation error (Infrastructure, Provider, etc.)
            context.Fail(new AuthorizationFailureReason(handler, result.FirstError.Description));
        }
    }
}
