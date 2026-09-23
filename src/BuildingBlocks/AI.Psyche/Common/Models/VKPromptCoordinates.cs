using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents layout segment coordinates, positioning strategy, and rendering priority.
/// Immutable Value Object. Follows AP.01.
/// </summary>
public sealed record VKPromptCoordinates
{
    private readonly int _depthPriority = 0;

    /// <summary>
    /// Gets the target chat role when this segment is rendered as a standalone chat message.
    /// Defaults to <see cref="VKChatRole.System"/>.
    /// </summary>
    public VKChatRole Role { get; init; } = VKChatRole.System;

    /// <summary>
    /// Gets the relative spatial anchor within the static persona/directive template, if relative positioning is used; otherwise, null.
    /// </summary>
    public VKPromptRelativeDepth? RelativeDepth { get; init; }

    /// <summary>
    /// Gets the temporal slot depth along the dialogue timeline (Echoes and UserInput), if timeline positioning is used; otherwise, null.
    /// <list type="bullet">
    ///   <item><description><c>0</c>: Injected immediately after current UserInput (prompt tail / suffix).</description></item>
    ///   <item><description><c>1</c>: Injected before current UserInput (between dialogue history and prompt trigger).</description></item>
    ///   <item><description><c>2 .. N</c>: Interleaved between historical dialogue echo turns.</description></item>
    ///   <item><description><c>-1</c>: Injected immediately before the oldest historical echo (preface slot, never precedes static persona/directives).</description></item>
    /// </list>
    /// </summary>
    public int? TimelineDepth { get; init; }

    /// <summary>
    /// Gets the rendering priority order among segments at the same relative depth or timeline slot. Priority must be between 0 and 999.
    /// </summary>
    public int DepthPriority
    {
        get => _depthPriority;
        init => _depthPriority = VKGuard.InRange(value, 0, 999, nameof(DepthPriority));
    }

    /// <summary>
    /// Gets the optional XML tag name used to enclose this segment in prompt output.
    /// Contiguous segments sharing the same TagName are automatically coalesced into a single XML root element during weaving.
    /// If null, the content is presented as raw text without XML wrapping.
    /// </summary>
    public string? TagName { get; init; }

    /// <summary>
    /// Gets the default coordinates instance (System role, AfterDirective, priority 0).
    /// </summary>
    public static VKPromptCoordinates Default { get; } = new()
    {
        Role = VKChatRole.System,
        RelativeDepth = VKPromptRelativeDepth.AfterDirective,
        DepthPriority = 0
    };

    /// <summary>
    /// Gets the prompt tier classification for pipeline scheduling.
    /// Internal to VK.Blocks.AI.Psyche; not mapped to persistence entities.
    /// </summary>
    internal VKPromptTierType Tier { get; init; } = VKPromptTierType.Dynamic;

    /// <summary>
    /// Computes the linearized layout ordering key based on relative depth anchors, tier, and depth priority.
    /// Internal to VK.Blocks.AI.Psyche; used for deterministic pipeline sorting.
    /// </summary>
    internal int LayoutOrder
    {
        get
        {
            int slotBase = RelativeDepth switch
            {
                VKPromptRelativeDepth.BeforeDirective => PsycheConstants.LayoutSlots.BeforeDirective,
                VKPromptRelativeDepth.AfterDirective => PsycheConstants.LayoutSlots.AfterDirective,
                VKPromptRelativeDepth.BeforePersona => PsycheConstants.LayoutSlots.BeforePersona,
                VKPromptRelativeDepth.AfterPersona => PsycheConstants.LayoutSlots.AfterPersona,
                _ => Tier switch
                {
                    VKPromptTierType.Directive => PsycheConstants.LayoutSlots.Directive,
                    VKPromptTierType.Persona => PsycheConstants.LayoutSlots.Persona,
                    _ => PsycheConstants.LayoutSlots.AfterPersona
                }
            };

            return slotBase + DepthPriority;
        }
    }

    /// <summary>
    /// Creates an assembled prompt segment by pairing these coordinates with a text payload.
    /// </summary>
    public VKPromptSegment ToSegment(string content, int tokenCount = 0) => new()
    {
        Coordinates = this,
        Payload = new VKPromptPayload { Content = content, TokenCount = tokenCount }
    };

    /// <summary>
    /// Creates an assembled prompt segment by pairing these coordinates with a payload value object.
    /// </summary>
    public VKPromptSegment ToSegment(VKPromptPayload payload) => new()
    {
        Coordinates = this,
        Payload = VKGuard.NotNull(payload, nameof(payload))
    };
}
