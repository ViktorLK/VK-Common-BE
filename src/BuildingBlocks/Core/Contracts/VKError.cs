namespace VK.Blocks.Core;

/// <summary>
/// Represents a standardized error with a code and description.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Type">The error type (e.g., Validation, NotFound, Failure).</param>
public sealed record VKError(string Code, string Description, VKErrorType Type = VKErrorType.Failure)
{
    public static readonly VKError None = new(string.Empty, string.Empty, VKErrorType.None);
    public static readonly VKError NullValue = new("VKError.NullValue", "The specified result value is null.", VKErrorType.Failure);
    public static readonly VKError ConditionNotMet = new("VKError.ConditionNotMet", "The specified condition was not met.", VKErrorType.Failure);

    public static VKError Validation(string code, string description) => new(code, description, VKErrorType.Validation);
    public static VKError Unauthorized(string code, string description) => new(code, description, VKErrorType.Unauthorized);
    public static VKError Forbidden(string code, string description) => new(code, description, VKErrorType.Forbidden);
    public static VKError NotFound(string code, string description) => new(code, description, VKErrorType.NotFound);
    public static VKError Conflict(string code, string description) => new(code, description, VKErrorType.Conflict);
    public static VKError PreconditionFailed(string code, string description) => new(code, description, VKErrorType.PreconditionFailed);
    public static VKError TooManyRequests(string code, string description) => new(code, description, VKErrorType.TooManyRequests);
    public static VKError Failure(string code, string description) => new(code, description, VKErrorType.Failure);
    public static VKError ExternalError(string code, string description) => new(code, description, VKErrorType.ExternalError);
    public static VKError ServiceUnavailable(string code, string description) => new(code, description, VKErrorType.ServiceUnavailable);
    public static VKError Timeout(string code, string description) => new(code, description, VKErrorType.Timeout);
}
