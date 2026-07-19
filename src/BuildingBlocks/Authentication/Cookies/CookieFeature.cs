using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Cookies.Internal;
using VK.Blocks.Authentication.Cookies.Protocols;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Hook implementation for Cookie Authentication and Session Limit feature.
/// </summary>
[VKFeature(typeof(VKAuthenticationBlock), OptionsType = typeof(VKCookieOptions))]
internal sealed partial class CookieFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCookieOptions options)
    {
        // 1. Register Session Store if not already overridden
        services.TryAddSingleton<IVKSessionStore, InMemorySessionStore>();

        // 2. Register Cookie Ticket Store Adapter
        services.TryAddSingleton<ITicketStore, CookieSessionTicketStore>();

        // 3. Register the ASP.NET Core Cookie authentication handler
        // Note: The main AddAuthentication builder is resolved from the block registration builder.
        // We resolve it inside our custom registration hook.
        // Using lazy lookup for AddAuthentication builder to avoid multiple calls.
        services.AddOptions<CookieAuthenticationOptions>(options.SchemeName)
            .Configure<ITicketStore>((cookieOpts, ticketStore) =>
            {
                cookieOpts.Cookie.Name = options.CookieName;
                cookieOpts.ExpireTimeSpan = TimeSpan.FromMinutes(options.ExpireMinutes);
                cookieOpts.SlidingExpiration = options.SlidingExpiration;
                cookieOpts.SessionStore = ticketStore;
            });

        // Add the scheme to AuthenticationBuilder
        var authBuilder = services.AddAuthentication();
        authBuilder.AddCookie(options.SchemeName);
    }
}
