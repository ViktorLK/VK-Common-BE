using System.Diagnostics;
using VK.Blocks.Core.Results;
using VK.Blocks.Observability.Conventions;

namespace VK.Blocks.Observability.Extensions;

/// <summary>
/// Provides VK.Blocks extension methods for <see cref="Activity"/>.
/// <para>
/// Acts as a bridge to automatically map business operation results (<see cref="IResult"/>)
/// to OpenTelemetry spans.
/// </para>
/// </summary>
public static class ActivityExtensions
{
    #region Public Methods

    /// <summary>
    /// Records the content of the <see cref="IResult"/> into the current span.
    /// </summary>
    /// <param name="activity">The <see cref="Activity"/> to record into. No-op if <c>null</c>.</param>
    /// <param name="result">The operation result to record.</param>
    /// <remarks>
    /// <para><strong>On Success:</strong> Adds the <c>result.success=true</c> tag; status remains <see cref="ActivityStatusCode.Unset"/>.</para>
    /// <para><strong>On Failure:</strong></para>
    /// <list type="bullet">
    ///   <item><description>Sets <c>result.success=false</c></description></item>
    ///   <item><description>Sets <c>result.code</c> to the error code</description></item>
    ///   <item><description>Sets <c>result.message</c> to the error description</description></item>
    ///   <item><description>Sets <c>error.type</c> to the string representation of <see cref="ErrorType"/></description></item>
    ///   <item><description>Sets activity status to <see cref="ActivityStatusCode.Error"/></description></item>
    ///   <item><description>Records an <see cref="ActivityEvent"/> named <c>"result.failure"</c></description></item>
    /// </list>
    /// <para>
    /// If no listeners exist for the activity's source,
    /// tag addition is skipped to minimize memory allocations.
    /// </para>
    /// </remarks>
    public static void RecordResult(this Activity? activity, IResult result)
    {
        // Guard — Guarantee zero allocation when no listeners are present
        if (activity is null)
        {
            return;
        }

        if (result.IsSuccess)
        {
            // Success: Add minimal tag
            activity.SetTag(FieldNames.ResultSuccess, true);
        }
        else
        {
            // Failure: Map full error details to the span
            var firstError = result.FirstError;

            activity.SetTag(FieldNames.ResultSuccess, false);
            activity.SetTag(FieldNames.ResultCode, firstError.Code);
            activity.SetTag(FieldNames.ResultMessage, firstError.Description);
            activity.SetTag(FieldNames.ErrorType, firstError.Type.ToString());

            // Set span status to Error
            activity.SetStatus(ActivityStatusCode.Error, firstError.Code);

            // Record failure event with tags
            var eventTags = new ActivityTagsCollection
            {
                { FieldNames.ResultCode, firstError.Code },
                { FieldNames.ResultMessage, firstError.Description },
                { FieldNames.ErrorType, firstError.Type.ToString() }
            };

            activity.AddEvent(new ActivityEvent(
                FieldNames.ResultFailure,
                tags: eventTags));
        }
    }

    /// <summary>
    /// Sets the tenant ID tag (<c>vk.tenant.id</c>).
    /// </summary>
    /// <param name="activity">The target <see cref="Activity"/>. No-op if <c>null</c>.</param>
    /// <param name="tenantId">The tenant ID. Skips if <c>null</c> or empty.</param>
    public static void SetTenantId(this Activity? activity, string? tenantId)
    {
        if (activity is null || string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        activity.SetTag(FieldNames.TenantId, tenantId);
    }

    /// <summary>
    /// Sets the user ID tag (<c>vk.user.id</c>).
    /// </summary>
    /// <param name="activity">The target <see cref="Activity"/>. No-op if <c>null</c>.</param>
    /// <param name="userId">The user ID. Skips if <c>null</c> or empty.</param>
    public static void SetUserId(this Activity? activity, string? userId)
    {
        if (activity is null || string.IsNullOrEmpty(userId))
        {
            return;
        }

        activity.SetTag(FieldNames.UserId, userId);
    }

    #endregion
}
