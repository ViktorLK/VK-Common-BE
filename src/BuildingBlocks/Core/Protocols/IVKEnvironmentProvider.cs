namespace VK.Blocks.Core;

/// <summary>
/// Prohibit direct use of non-deterministic system APIs (CS.06).
/// This provider abstracts environment variable access.
/// </summary>
public interface IVKEnvironmentProvider
{
    /// <summary>
    /// Retrieves the value of an environment variable from the current process.
    /// </summary>
    /// <param name="name">The name of the environment variable.</param>
    /// <returns>The value of the environment variable, or null if it does not exist.</returns>
    string? GetVariable(string name);
}

