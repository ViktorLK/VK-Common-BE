using Microsoft.AspNetCore.Builder;
using VK.Blocks.Identity.Web.Context.Internal;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Pipeline extension methods for Identity Web Block.
/// </summary>
public static class VKIdentityWebBlockExtensions
{
    /// <summary>
    /// Adds the user context resolution middleware to the ASP.NET Core pipeline.
    /// Pushes the active user identity into the ambient execution context.
    /// </summary>
    public static IApplicationBuilder UseIdentityBlock(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserContextMiddleware>();
    }
}
