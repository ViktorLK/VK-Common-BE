using System;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Internal;

/// <summary>
/// Default implementation of <see cref="IVKProfileRenderer"/>.
/// Formats user profile presence (preferred language, local time context, output preferences) into prompt instruction text.
/// Follows AP.01, CS.04 (Span/ValueStringBuilder), CS.06, and AP.04.
/// </summary>
internal sealed class DefaultProfileRenderer : IVKProfileRenderer
{
    private readonly TimeProvider _timeProvider;

    public DefaultProfileRenderer(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string Render(VKProfilePresence profile, DateTimeOffset? referenceTime = null)
    {
        VKGuard.NotNull(profile);

        Span<char> initialBuffer = stackalloc char[512];
        using var sb = new VKValueStringBuilder(initialBuffer);

        // 1. Language Requirement
        if (!string.IsNullOrWhiteSpace(profile.PreferredLanguage))
        {
            sb.Append(ProfileConstants.Prefixes.LanguageStart);
            sb.Append(profile.PreferredLanguage);
            sb.AppendLine(ProfileConstants.Prefixes.LanguageEnd);
        }

        // 2. Local Time Context
        if (!string.IsNullOrWhiteSpace(profile.TimeZone))
        {
            var nowUtc = referenceTime ?? _timeProvider.GetUtcNow();
            var timeStr = TryFormatUserLocalTime(nowUtc, profile.TimeZone, out var formattedLocalTime)
                ? $"{formattedLocalTime} ({profile.TimeZone})"
                : $"{nowUtc:yyyy-MM-dd HH:mm:ss} UTC ({profile.TimeZone})";

            sb.Append(ProfileConstants.Prefixes.TimeContext);
            sb.Append(timeStr);
            sb.AppendLine(".");
        }

        // 3. Custom User Context / Instructions
        if (!string.IsNullOrWhiteSpace(profile.Description))
        {
            sb.AppendLine(profile.Description);
        }

        return sb.ToString().Trim();
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
