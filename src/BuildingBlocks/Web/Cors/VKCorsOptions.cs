using VK.Blocks.Core;
namespace VK.Blocks.Web;

/// <summary>
/// Options for configuring Cross-Origin Resource Sharing (CORS).
/// </summary>
public sealed record VKCorsOptions : IVKBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Web:Cors";

    /// <summary>
    /// Gets the name of the default policy.
    /// </summary>
    public const string DefaultPolicyName = "VK.DefaultCorsPolicy";

    /// <summary>
    /// Gets or sets a value indicating whether CORS is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the list of allowed origins.
    /// Default is empty (restrictive).
    /// </summary>
    public string[] AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Gets or sets the list of allowed methods.
    /// Default is ["GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH"].
    /// </summary>
    public string[] AllowedMethods { get; init; } = ["GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH"];

    /// <summary>
    /// Gets or sets the list of allowed headers.
    /// Default is ["*"].
    /// </summary>
    public string[] AllowedHeaders { get; init; } = ["*"];

    /// <summary>
    /// Gets or sets a value indicating whether to allow credentials.
    /// Default is false.
    /// </summary>
    public bool AllowCredentials { get; init; } = false;

    /// <summary>
    /// Gets or sets the list of exposed headers.
    /// </summary>
    public string[] ExposedHeaders { get; init; } = ["X-Correlation-Id", "X-Pagination"];
}
