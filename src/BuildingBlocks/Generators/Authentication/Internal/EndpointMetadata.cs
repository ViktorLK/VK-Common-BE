namespace VK.Blocks.Generators.Authentication.Internal;

/// <summary>
/// Intermediate model for endpoint authentication metadata used during the generation process.
/// </summary>
/// <param name="EndpointName">The full display name of the endpoint.</param>
/// <param name="AuthGroup">The name of the authentication group, if any.</param>
/// <param name="Schemes">The required authentication schemes.</param>
/// <param name="Policies">The required authorization policies.</param>
/// <param name="IsAnonymous">Whether the endpoint allows anonymous access.</param>
internal sealed record EndpointMetadata(
    string EndpointName,
    string? AuthGroup,
    string[] Schemes,
    string[] Policies,
    bool IsAnonymous);
