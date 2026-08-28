namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the formatting style options for rendering short-term conversation echo history.
/// </summary>
public enum VKEchoRenderStyle : byte
{
    /// <summary>
    /// Raw message content without any role formatting or headers.
    /// </summary>
    Raw = 0,

    /// <summary>
    /// Role header format (e.g. User: Hello).
    /// </summary>
    Header = 1,

    /// <summary>
    /// XML tag format (e.g. &lt;user&gt;Hello&lt;/user&gt;).
    /// </summary>
    Xml = 2,

    /// <summary>
    /// ChatML token format (e.g. &lt;|im_start|&gt;user\nHello&lt;|im_end|&gt;).
    /// </summary>
    ChatML = 3,

    /// <summary>
    /// Bracket role format (e.g. [User]: Hello).
    /// </summary>
    Bracket = 4
}
