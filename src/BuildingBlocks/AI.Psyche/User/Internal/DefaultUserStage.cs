using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.User.Internal;

/// <summary>
/// Pipeline stage responsible for resolving and attaching <see cref="VKUserPresence"/> metadata before prompt weaving.
/// Follows AP.01 (sealed class default), CS.01, and CS.03.
/// </summary>
internal sealed class DefaultUserStage : IVKPsychePipelineStage
{
    private readonly VKUserOptions _options;
    private readonly IVKUserStore _userStore;
    private readonly IVKIdentityContext _identityContext;
    private readonly TimeProvider _timeProvider;

    public DefaultUserStage(
        VKUserOptions options,
        IVKUserStore userStore,
        IVKIdentityContext identityContext,
        TimeProvider? timeProvider = null)
    {
        _options = VKGuard.NotNull(options);
        _userStore = VKGuard.NotNull(userStore);
        _identityContext = VKGuard.NotNull(identityContext);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheUser;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 1. Resolve UserId from IdentityContext
        var userId = _identityContext.UserId;
        if (!userId.IsEmpty)
        {
            var presenceResult = await _userStore.GetPresenceAsync(userId, cancellationToken).ConfigureAwait(false);
            if (presenceResult.IsSuccess && presenceResult.Value is not null)
            {
                var presence = presenceResult.Value;
                context.SetState(presence);

                // 2. Inject PreferredLanguage directive if defined (only if non-null/non-empty)
                if (!string.IsNullOrWhiteSpace(presence.PreferredLanguage))
                {
                    context.AddFragment(new VKPromptFragment
                    {
                        TierType = VKPromptTierType.Directive,
                        RenderOrder = PromptLayout.DefaultRenderOrders[VKPromptTierType.Directive] + 10,
                        Metadata = presence,
                        Segment = new VKPromptSegment
                        {
                            Role = VKChatRole.System,
                            Content = $"[Language Requirement]: Please respond using the user's preferred language ({presence.PreferredLanguage})."
                        }
                    });
                }

                // 3. Inject TimeZone & Local Time directive if defined (only if non-null/non-empty)
                if (!string.IsNullOrWhiteSpace(presence.TimeZone))
                {
                    var nowUtc = _timeProvider.GetUtcNow();
                    var timeStr = TryFormatUserLocalTime(nowUtc, presence.TimeZone, out var formattedLocalTime)
                        ? $"{formattedLocalTime} ({presence.TimeZone})"
                        : $"{nowUtc:yyyy-MM-dd HH:mm:ss} UTC ({presence.TimeZone})";

                    context.AddFragment(new VKPromptFragment
                    {
                        TierType = VKPromptTierType.Directive,
                        RenderOrder = PromptLayout.DefaultRenderOrders[VKPromptTierType.Directive] + 5,
                        Metadata = presence,
                        Segment = new VKPromptSegment
                        {
                            Role = VKChatRole.System,
                            Content = $"[Current Time Context]: {timeStr}."
                        }
                    });
                }

                // 4. Inject Preferences directive if defined (only if non-empty dictionary)
                if (presence.Preferences.Count > 0)
                {
                    var prefsStr = string.Join("; ", presence.Preferences.Select(kv => $"{kv.Key}: {kv.Value}"));
                    context.AddFragment(new VKPromptFragment
                    {
                        TierType = VKPromptTierType.Directive,
                        RenderOrder = PromptLayout.DefaultRenderOrders[VKPromptTierType.Directive] + 15,
                        Metadata = presence,
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
