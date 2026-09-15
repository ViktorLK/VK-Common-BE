using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    private readonly IVKProfileRenderer _profileRenderer;
    private readonly ILogger<DefaultProfileStage> _logger;

    public DefaultProfileStage(
        VKProfileOptions options,
        IVKPsycheProfileRepository profileRepository,
        IVKProfileRenderer profileRenderer,
        ILogger<DefaultProfileStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _profileRepository = VKGuard.NotNull(profileRepository);
        _profileRenderer = VKGuard.NotNull(profileRenderer);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheProfile;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var profileArgs = context.Args<VKProfileArgs>();
        var isEnabled = profileArgs?.Enabled ?? _options.Enabled;
        if (!isEnabled)
        {
            return VKResult.Success();
        }

        // 1. Resolve ProfileId from Request
        var profileId = context.Request.ProfileId;
        if (profileId.IsNullOrEmpty())
        {
            return VKResult.Success();
        }

        var profileResult = await _profileRepository.FindByIdAsync(profileId.Value, cancellationToken).ConfigureAwait(false);
        if (profileResult.IsFailure)
        {
            return VKResult.Failure(profileResult.Errors);
        }

        var profile = profileResult.Value;
        if (profile is null)
        {
            return VKResult.Success();
        }

        context.SetState(profile);

        _logger.ProfileResolved(
            profile.Id,
            profile.PreferredLanguage ?? "None",
            profile.TimeZone ?? "None");

        var content = _profileRenderer.Render(profile, context.CreatedAt);
        if (!string.IsNullOrWhiteSpace(content))
        {
            _logger.ProfileRendered(profile.Id, content.Length);
            context.AddSegment(new VKPromptSegment
            {
                Role = VKChatRole.System,
                Content = content,
                TagName = profile.TagName ?? ProfileConstants.Defaults.TagName,
                RelativeDepth = profile.RelativeDepth ?? ProfileConstants.Defaults.RelativeDepth,
                DepthPriority = profile.DepthPriority,
                TimelineDepth = profile.TimelineDepth,
                Tier = VKPromptTierType.Profile,
                TokenCount = profile.TokenCount
            });
        }

        ProfileDiagnostics.RecordProfilesResolved(1, "Profile");

        return VKResult.Success();
    }
}
