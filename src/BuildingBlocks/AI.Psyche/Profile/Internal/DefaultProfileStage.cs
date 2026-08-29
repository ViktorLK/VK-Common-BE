using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI.Psyche.Profile.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Internal;

/// <summary>
/// Pipeline stage responsible for resolving and attaching <see cref="VKProfilePresence"/> metadata before prompt weaving.
/// Follows AP.01 (sealed class default), CS.01, CS.03, BB.04, and OR.01.
/// </summary>
[VKTrace("psyche.stage.profile")]
internal sealed class DefaultProfileStage : IVKPsychePipelineStage
{
    private readonly VKProfileOptions _options;
    private readonly IVKPsycheProfileRepository _profileRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DefaultProfileStage> _logger;

    public DefaultProfileStage(
        VKProfileOptions options,
        IVKPsycheProfileRepository profileRepository,
        TimeProvider? timeProvider = null,
        ILogger<DefaultProfileStage>? logger = null)
    {
        _options = VKGuard.NotNull(options);
        _profileRepository = VKGuard.NotNull(profileRepository);
        _timeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger ?? NullLogger<DefaultProfileStage>.Instance;
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheProfile;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 1. Resolve ProfileId from Request
        var profileId = context.Request.ProfileId;
        if (!profileId.HasValue || profileId.Value.IsEmpty)
        {
            return VKResult.Success();
        }

        var profileResult = await _profileRepository.FindByIdAsync(profileId.Value, cancellationToken).ConfigureAwait(false);
        if (profileResult.IsSuccess && profileResult.Value is not null)
        {
            var profile = profileResult.Value;
            context.SetState(profile);

            _logger.ProfileResolved(
                profile.Id.Value.ToString(),
                profile.PreferredLanguage ?? "None",
                profile.TimeZone ?? "None");

            // 2. Inject PreferredLanguage directive if defined (only if non-null/non-empty)
            if (!string.IsNullOrWhiteSpace(profile.PreferredLanguage))
            {
                context.AddFragment(new VKPromptFragment
                {
                    TierType = VKPromptTierType.Directive,
                    RenderOrder = PromptLayout.DefaultRenderOrders[VKPromptTierType.Directive] + 10,
                    Metadata = profile,
                    Segment = new VKPromptSegment
                    {
                        Role = VKChatRole.System,
                        Content = $"[Language Requirement]: Please respond using the user's preferred language ({profile.PreferredLanguage})."
                    }
                });
            }

            // 3. Inject TimeZone & Local Time directive if defined (only if non-null/non-empty)
            if (!string.IsNullOrWhiteSpace(profile.TimeZone))
            {
                var nowUtc = _timeProvider.GetUtcNow();
                var timeStr = TryFormatUserLocalTime(nowUtc, profile.TimeZone, out var formattedLocalTime)
                    ? $"{formattedLocalTime} ({profile.TimeZone})"
                    : $"{nowUtc:yyyy-MM-dd HH:mm:ss} UTC ({profile.TimeZone})";

                context.AddFragment(new VKPromptFragment
                {
                    TierType = VKPromptTierType.Directive,
                    RenderOrder = PromptLayout.DefaultRenderOrders[VKPromptTierType.Directive] + 5,
                    Metadata = profile,
                    Segment = new VKPromptSegment
                    {
                        Role = VKChatRole.System,
                        Content = $"[Current Time Context]: {timeStr}."
                    }
                });
            }

            // 4. Inject Preferences directive if defined (only if non-empty dictionary)
            if (profile.Preferences is { Count: > 0 })
            {
                var prefsStr = string.Join("; ", profile.Preferences.Select(kv => $"{kv.Key}: {kv.Value}"));
                context.AddFragment(new VKPromptFragment
                {
                    TierType = VKPromptTierType.Directive,
                    RenderOrder = PromptLayout.DefaultRenderOrders[VKPromptTierType.Directive] + 15,
                    Metadata = profile,
                    Segment = new VKPromptSegment
                    {
                        Role = VKChatRole.System,
                        Content = $"[User Output Preferences]: {prefsStr}."
                    }
                });
            }

            ProfileDiagnostics.RecordProfilesResolved(1, "Profile");
        }

        return VKResult.Success();
    }

    private static bool TryFormatUserLocalTime(DateTimeOffset nowUtc, string timeZoneId, out string result)
    {
        try
        {
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localTime = TimeZoneInfo.ConvertTime(nowUtc, tzInfo);
            result = localTime.ToString("yyyy-MM-dd HH:mm:ss");
            return true;
        }
        catch
        {
            result = string.Empty;
            return false;
        }
    }
}
