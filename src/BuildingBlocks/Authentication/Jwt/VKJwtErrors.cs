using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Errors associated with JWT authentication.
/// </summary>
public static class VKJwtErrors
{
    /// <summary>
    /// VKError when the provided token is null or whitespace.
    /// </summary>
    public static readonly VKError EmptyToken = new("Auth.EmptyToken", "The provided token is empty.", VKErrorType.Validation);

    /// <summary>
    /// VKError when authentication infrastructure is not properly configured.
    /// </summary>
    public static readonly VKError ConfigurationError = new("Auth.ConfigurationError", "Authentication is not configured.", VKErrorType.Failure);

    /// <summary>
    /// VKError when the token is not a valid JWT.
    /// </summary>
    public static readonly VKError InvalidFormat = new("Auth.InvalidFormat", "The provided token is not a valid JWT.", VKErrorType.Validation);

    /// <summary>
    /// VKError when the token has been explicitly revoked.
    /// </summary>
    public static readonly VKError Revoked = new("Auth.Revoked", "The token has been revoked.", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError when the token has expired.
    /// </summary>
    public static readonly VKError Expired = new("Auth.Expired", "The token has expired.", VKErrorType.Unauthorized);

    /// <summary>
    /// General error for an invalid token.
    /// </summary>
    public static readonly VKError Invalid = new("Auth.Invalid", "The token is invalid.", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError when the token issuer does not match configuration.
    /// </summary>
    public static readonly VKError IssuerInvalid = new("Auth.IssuerInvalid", "The token issuer is invalid.", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError when the token audience does not match configuration.
    /// </summary>
    public static readonly VKError AudienceInvalid = new("Auth.AudienceInvalid", "The token audience is invalid.", VKErrorType.Unauthorized);
}






