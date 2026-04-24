namespace VK.Blocks.Core;

/// <summary>
/// Specifies the direction of pagination when using cursors.
/// </summary>
public enum VKCursorDirection
{
    /// <summary>
    /// Paginate forward from the current cursor.
    /// </summary>
    Forward,

    /// <summary>
    /// Paginate backward from the current cursor.
    /// </summary>
    Backward
}
