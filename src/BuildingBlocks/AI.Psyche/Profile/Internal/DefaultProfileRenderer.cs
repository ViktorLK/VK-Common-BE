using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Internal;

/// <summary>
/// Default implementation of <see cref="IVKProfileRenderer"/>.
/// Formats user profile presence (output language, user addressing, tone, verbosity, emoji policy, custom instructions)
/// into structured XML tags.
/// Follows AP.01, CS.04 (Span/ValueStringBuilder).
/// </summary>
internal sealed class DefaultProfileRenderer : IVKProfileRenderer // [AP.01] sealed
{
    public string Render(VKProfilePresence profile)
    {
        VKGuard.NotNull(profile); // [AP.01]

        Span<char> initialBuffer = stackalloc char[1024]; // [CS.04]
        using var sb = new VKValueStringBuilder(initialBuffer);

        // 1. Output Language
        if (!string.IsNullOrWhiteSpace(profile.PreferredLanguage))
        {
            sb.Append('<');
            sb.Append(ProfileConstants.XmlTags.OutputLanguage);
            sb.Append('>');
            sb.Append(profile.PreferredLanguage);
            sb.Append("</");
            sb.Append(ProfileConstants.XmlTags.OutputLanguage);
            sb.AppendLine(">");
        }

        // 2. User Addressing (Actionable directive)
        if (!string.IsNullOrWhiteSpace(profile.AddressingTerm))
        {
            sb.Append('<');
            sb.Append(ProfileConstants.XmlTags.UserAddressing);
            sb.Append('>');
            sb.Append(ProfileConstants.Directives.Addressing.Prefix);
            sb.Append(profile.AddressingTerm);
            sb.Append(ProfileConstants.Directives.Addressing.Suffix);
            sb.Append("</");
            sb.Append(ProfileConstants.XmlTags.UserAddressing);
            sb.AppendLine(">");
        }

        // 3. Interaction Tone (Actionable directive, rendered only if configured)
        if (profile.InteractionTone.HasValue)
        {
            var toneDirective = profile.InteractionTone.Value switch
            {
                VKInteractionTone.Concise => ProfileConstants.Directives.Tone.Concise,
                VKInteractionTone.Friendly => ProfileConstants.Directives.Tone.Friendly,
                VKInteractionTone.Academic => ProfileConstants.Directives.Tone.Academic,
                _ => ProfileConstants.Directives.Tone.Professional
            };
            sb.Append('<');
            sb.Append(ProfileConstants.XmlTags.InteractionTone);
            sb.Append('>');
            sb.Append(toneDirective);
            sb.Append("</");
            sb.Append(ProfileConstants.XmlTags.InteractionTone);
            sb.AppendLine(">");
        }

        // 4. Response Verbosity (Actionable directive, rendered only if configured)
        if (profile.ResponseVerbosity.HasValue)
        {
            var verbosityDirective = profile.ResponseVerbosity.Value switch
            {
                VKResponseVerbosity.Concise => ProfileConstants.Directives.Verbosity.Concise,
                VKResponseVerbosity.Detailed => ProfileConstants.Directives.Verbosity.Detailed,
                VKResponseVerbosity.StepByStep => ProfileConstants.Directives.Verbosity.StepByStep,
                _ => ProfileConstants.Directives.Verbosity.Standard
            };
            sb.Append('<');
            sb.Append(ProfileConstants.XmlTags.ResponseVerbosity);
            sb.Append('>');
            sb.Append(verbosityDirective);
            sb.Append("</");
            sb.Append(ProfileConstants.XmlTags.ResponseVerbosity);
            sb.AppendLine(">");
        }

        // 5. Emoji Policy (Actionable directive, rendered only if configured)
        if (profile.EmojiPolicy.HasValue)
        {
            var emojiDirective = profile.EmojiPolicy.Value switch
            {
                VKEmojiPolicy.Minimal => ProfileConstants.Directives.Emoji.Minimal,
                VKEmojiPolicy.Rich => ProfileConstants.Directives.Emoji.Rich,
                _ => ProfileConstants.Directives.Emoji.None
            };
            sb.Append('<');
            sb.Append(ProfileConstants.XmlTags.EmojiPolicy);
            sb.Append('>');
            sb.Append(emojiDirective);
            sb.Append("</");
            sb.Append(ProfileConstants.XmlTags.EmojiPolicy);
            sb.AppendLine(">");
        }

        // 6. Custom Instructions / Background
        if (!string.IsNullOrWhiteSpace(profile.Description))
        {
            sb.Append('<');
            sb.Append(ProfileConstants.XmlTags.CustomInstructions);
            sb.AppendLine(">");
            sb.AppendLine(profile.Description.Trim());
            sb.Append("</");
            sb.Append(ProfileConstants.XmlTags.CustomInstructions);
            sb.AppendLine(">");
        }

        return sb.ToString().Trim();
    }
}
