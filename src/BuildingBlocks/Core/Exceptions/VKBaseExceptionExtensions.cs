namespace VK.Blocks.Core;

/// <summary>
/// Extension methods for <see cref="VKBaseException"/> to enable fluent configuration.
/// </summary>
public static class VKBaseExceptionExtensions
{
    /// <summary>
    /// Adds an extension property to the exception and returns the concrete exception type for fluent chaining.
    /// </summary>
    /// <typeparam name="TException">The concrete exception type.</typeparam>
    /// <param name="ex">The exception instance.</param>
    /// <param name="key">The extension key.</param>
    /// <param name="value">The extension value.</param>
    /// <returns>The same exception instance.</returns>
    public static TException WithExtension<TException>(this TException ex, string key, object? value)
        where TException : VKBaseException
    {
        ex.SetExtension(key, value);
        return ex;
    }
}
