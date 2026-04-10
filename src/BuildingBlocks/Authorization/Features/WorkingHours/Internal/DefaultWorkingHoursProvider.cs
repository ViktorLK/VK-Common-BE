using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.DependencyInjection;

namespace VK.Blocks.Authorization.Features.WorkingHours.Internal;

/// <inheritdoc />
public sealed class DefaultWorkingHoursProvider(
    IOptions<VKAuthorizationOptions> options) 
    : IWorkingHoursProvider
{
    private readonly VKAuthorizationOptions _options = options.Value;

    /// <inheritdoc />
    public ValueTask<(TimeOnly Start, TimeOnly End)?> GetWorkingHoursAsync(
        ClaimsPrincipal user, 
        CancellationToken ct = default)
    {
        // Return global defaults from options
        return ValueTask.FromResult<(TimeOnly Start, TimeOnly End)?>(
            (_options.WorkStart, _options.WorkEnd));
    }
}
