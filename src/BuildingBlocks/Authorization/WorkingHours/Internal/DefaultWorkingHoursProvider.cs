using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;


namespace VK.Blocks.Authorization.WorkingHours.Internal;

/// <summary>
/// Default implementation of <see cref="IVKWorkingHoursProvider"/> that uses global configuration.
/// </summary>
internal sealed class DefaultWorkingHoursProvider(
    IOptions<VKWorkingHoursOptions> options)
    : IVKWorkingHoursProvider
{
    private readonly VKWorkingHoursOptions _options = options.Value;

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
