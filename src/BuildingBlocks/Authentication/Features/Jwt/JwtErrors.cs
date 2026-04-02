using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.Jwt;

/// <summary>
/// Errors associated with JWT authentication.
/// </summary>
public static class JwtErrors
{
    #region Fields

    /// <summary>
    /// Error when the provided token is null or whitespace.
    /// </summary>
    public static readonly Error EmptyToken = new("Auth.EmptyToken", "The provided token is empty.", ErrorType.Validation);

    /// <summary>
    /// Error when authentication infrastructure is not properly configured.
    /// </summary>
    public static readonly Error ConfigurationError = new("Auth.ConfigurationError", "Authentication is not configured.", ErrorType.Failure);

    /// <summary>
    /// Error when the token is not a valid JWT.
    /// </summary>
    public static readonly Error InvalidFormat = new("Auth.InvalidFormat", "The provided token is not a valid JWT.", ErrorType.Validation);

    /// <summary>
    /// Error when the token has been explicitly revoked.
    /// </summary>
    public static readonly Error Revoked = new("Auth.Revoked", "The token has been revoked.", ErrorType.Unauthorized);

    /// <summary>
    /// Error when the token has expired.
    /// </summary>
    public static readonly Error Expired = new("Auth.Expired", "The token has expired.", ErrorType.Unauthorized);

    /// <summary>
    /// General error for an invalid token.
    /// </summary>
    public static readonly Error Invalid = new("Auth.Invalid", "The token is invalid.", ErrorType.Unauthorized);

    /// <summary>
    /// Error when the token issuer does not match configuration.
    /// </summary>
    public static readonly Error IssuerInvalid = new("Auth.IssuerInvalid", "The token issuer is invalid.", ErrorType.Unauthorized);

    /// <summary>
    /// Error when the token audience does not match configuration.
    /// </summary>
    public static readonly Error AudienceInvalid = new("Auth.AudienceInvalid", "The token audience is invalid.", ErrorType.Unauthorized);

    #endregion
}
