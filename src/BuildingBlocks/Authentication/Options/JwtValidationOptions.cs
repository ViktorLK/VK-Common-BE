namespace VK.Blocks.Authentication.Options;

/// <summary>
/// Configuration options for JWT tokens.
/// </summary>
public class JwtValidationOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets the issuer of the token.
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Gets or sets the intended audience of the token.
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Gets or sets the secret key used to sign the token.
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the access token in minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the clock skew in seconds allowed when validating a token.
    /// </summary>
    public double ClockSkewSeconds { get; internal set; }

    #endregion
}
