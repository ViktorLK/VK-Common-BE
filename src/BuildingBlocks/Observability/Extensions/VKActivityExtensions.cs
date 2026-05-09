using System.Diagnostics;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// Provides VK.Blocks extension methods for <see cref="Activity"/>.
/// </summary>
public static class VKActivityExtensions
{
    /// <summary>
    /// Records the content of the <see cref="IVKResult"/> into the current span.
    /// </summary>
    /// <param name="activity">The <see cref="Activity"/> to record into. No-op if <c>null</c>.</param>
    /// <param name="result">The operation result to record.</param>
    public static void RecordResult(this Activity? activity, IVKResult result)
    {
        if (activity is null)
        {
            return;
        }

        if (result.IsSuccess)
        {
            activity.SetTag(FieldNames.ResultSuccess, true);
        }
        else
        {
            var firstError = result.FirstError;

            activity.SetTag(FieldNames.ResultSuccess, false);
            activity.SetTag(FieldNames.ResultCode, firstError.Code);
            activity.SetTag(FieldNames.ResultMessage, firstError.Description);
            activity.SetTag(FieldNames.VKErrorType, firstError.Type.ToString());

            // Set span status to error
            activity.SetStatus(ActivityStatusCode.Error, firstError.Code);

            // Record failure event
            var eventTags = new ActivityTagsCollection
            {
                { FieldNames.ResultCode, firstError.Code },
                { FieldNames.ResultMessage, firstError.Description },
                { FieldNames.VKErrorType, firstError.Type.ToString() }
            };

            activity.AddEvent(new ActivityEvent(FieldNames.ResultFailure, tags: eventTags));
        }
    }

    /// <summary>
    /// Sets the tenant ID tag.
    /// </summary>
    public static void SetTenantId(this Activity? activity, string? tenantId)
    {
        if (activity is not null && !string.IsNullOrEmpty(tenantId))
        {
            activity.SetTag(FieldNames.TenantId, tenantId);
        }
    }

    /// <summary>
    /// Sets the user ID tag.
    /// </summary>
    public static void SetUserId(this Activity? activity, string? userId)
    {
        if (activity is not null && !string.IsNullOrEmpty(userId))
        {
            activity.SetTag(FieldNames.UserId, userId);
        }
    }

    /// <summary>
    /// Generic tag setter with null check.
    /// </summary>
    public static void SetTag(this Activity? activity, string key, object? value)
    {
        activity?.SetTag(key, value);
    }
}
