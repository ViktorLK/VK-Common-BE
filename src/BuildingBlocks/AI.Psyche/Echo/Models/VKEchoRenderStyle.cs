namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the formatting style options for rendering short-term conversation echo history.
/// </summary>
public enum VKEchoRenderStyle
{
    /// <summary>
    /// Raw message content without any role formatting or headers.
    /// </summary>
    Raw,

    /// <summary>
    /// Role header format (e.g. User: Hello).
    /// </summary>
    Header,

    /// <summary>
    /// XML tag format (e.g. &lt;user&gt;Hello&lt;/user&gt;).
    /// </summary>
    Xml,

    /// <summary>
    /// ChatML token format (e.g. &lt;|im_start|&gt;user\nHello&lt;|im_end|&gt;).
    /// </summary>
    ChatML,

    /// <summary>
    /// Bracket role format (e.g. [User]: Hello).
    /// </summary>
    Bracket
}
