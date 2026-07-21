using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using VK.Blocks.Authorization.Generated;

namespace VK.Blocks.Authorization;

/// <summary>
/// Provides extension methods for mapping VK.Blocks.Authorization API endpoints.
/// </summary>
public static class VKAuthorizationEndpointExtensions
{
    /// <summary>
    /// Maps a GET endpoint returning the serialized JSON catalog of all system permissions.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The URI pattern (defaults to "/api/authorization/permissions").</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapVKPermissionsEndpoint(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/authorization/permissions")
    {
        endpoints.MapGet(pattern, () => 
        {
            return Results.Content(PermissionsCatalog.CatalogJson, "application/json");
        });
        
        return endpoints;
    }
}
