namespace VK.Blocks.Core.Abstractions;

/// <summary>
/// Abstraction over the system clock.
/// Enables deterministic time control in tests and consistent UTC usage in production.
/// </summary>
public interface IDateTime
{
    #region Properties

    /// <summary>Gets the current UTC date and time.</summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>Gets the current UTC date (no time component).</summary>
    DateOnly Today { get; }

    #endregion
}
