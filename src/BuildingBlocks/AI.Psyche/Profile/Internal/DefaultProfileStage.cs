using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Internal;

/// <summary>
/// Pipeline stage responsible for resolving and attaching <see cref="VKProfilePresence"/> metadata before prompt weaving.
/// Follows AP.01 (sealed class default), CS.01, and CS.03.
/// </summary>
internal sealed class DefaultProfileStage : IVKPsychePipelineStage
{
    private readonly VKProfileOptions _options;
    private readonly IVKProfileStore _profileStore;
    private readonly IVKIdentityContext _identityContext;
    private readonly TimeProvider _timeProvider;

    public DefaultProfileStage(
        VKProfileOptions options,
        IVKProfileStore profileStore,
        IVKIdentityContext identityContext,
        TimeProvider? timeProvider = null)
    {
        _options = VKGuard.NotNull(options);
        _profileStore = VKGuard.NotNull(profileStore);
        _identityContext = VKGuard.NotNull(identityContext);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheProfile;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 1. Resolve UserId from Request or IdentityContext
        var userId = context.Request.UserId ?? _identityContext.UserId;
        if (!userId.IsEmpty)
        {
            var profileResult = await _profileStore.GetProfileAsync(userId, cancellationToken).ConfigureAwait(false);
            if (profileResult.IsSuccess && profileResult.Value is not null)
            {
                var profile = profileResult.Value;
                context.SetState(profile);

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
                if (profile.Preferences.Count > 0)
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
            }
        }

        return VKResult.Success();
    }

    private static bool TryFormatUserLocalTime(System.DateTimeOffset nowUtc, string timeZoneId, out string result)
    {
        try
        {
            if (System.TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tzInfo))
            {
                var localTime = System.TimeZoneInfo.ConvertTime(nowUtc, tzInfo);
                result = localTime.ToString("yyyy-MM-dd HH:mm:ss dddd");
                return true;
            }
        }
        catch
        {
            // Ignore time zone parsing errors, fallback to false
        }

        result = string.Empty;
        return false;
    }
}
