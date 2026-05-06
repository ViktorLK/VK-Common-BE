namespace VK.Blocks.Core;

/// <summary>
/// Provides extension methods for merging local overrides with global defaults.
/// Following Rule 21: Hierarchical Configuration Pattern.
/// </summary>
public static class VKMergeExtensions
{
    /// <summary>
    /// Merges a local override value with a global default value (Rule 21).
    /// Returns the local value if it's not null; otherwise, returns the global value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="local">The local override value (nullable).</param>
    /// <param name="global">The global default value.</param>
    /// <returns>The merged value.</returns>
    public static T MergeWith<T>(this T? local, T global) where T : struct
        => local ?? global;

    /// <summary>
    /// Merges a local override reference with a global default reference (Rule 21).
    /// Returns the local value if it's not null; otherwise, returns the global value.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="local">The local override value (nullable).</param>
    /// <param name="global">The global default value.</param>
    /// <returns>The merged value.</returns>
    public static T MergeWith<T>(this T? local, T global) where T : class
        => local ?? global;
}
