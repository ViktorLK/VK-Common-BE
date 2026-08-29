namespace VK.Blocks.Core;

/// <summary>
/// Specifies the direction of pagination when using cursors.
/// </summary>
public enum VKCursorDirection : byte
{
    /// <summary>
    /// Paginate forward from the current cursor.
    /// </summary>
    Forward = 0,

    /// <summary>
    /// Paginate backward from the current cursor.
    /// </summary>
    Backward = 1
}
