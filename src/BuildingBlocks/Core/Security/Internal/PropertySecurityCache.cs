using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;

namespace VK.Blocks.Core.Security.Internal;

/// <summary>
/// Cache for property-level security metadata.
/// Optimized for high-performance log masking and data redaction.
/// </summary>
/// <typeparam name="T">The type to cache property metadata for.</typeparam>
public sealed class PropertySecurityCache<T>
{
    private static readonly FrozenDictionary<string, SecurityLevel> _propertySecurityLevels;

    static PropertySecurityCache()
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var dict = new Dictionary<string, SecurityLevel>(StringComparer.OrdinalIgnoreCase);

        foreach (PropertyInfo prop in properties)
        {
            if (prop.GetCustomAttribute<RedactedAttribute>() is not null)
            {
                dict[prop.Name] = SecurityLevel.Redacted;
            }
            else if (prop.GetCustomAttribute<SensitiveDataAttribute>() is not null)
            {
                dict[prop.Name] = SecurityLevel.Sensitive;
            }
        }

        _propertySecurityLevels = dict.ToFrozenDictionary();
    }

    /// <summary>
    /// Gets a value indicating whether the type has any security-sensitive properties.
    /// </summary>
    public static bool HasSensitiveProperties => _propertySecurityLevels.Count > 0;

    /// <summary>
    /// Gets the security level for a specific property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>The security level.</returns>
    public static SecurityLevel GetLevel(string propertyName)
    {
        return _propertySecurityLevels.TryGetValue(propertyName, out SecurityLevel level)
            ? level
            : SecurityLevel.None;
    }

    /// <summary>
    /// Gets all properties that require masking or redaction.
    /// </summary>
    public static IEnumerable<string> SensitivePropertyNames => _propertySecurityLevels.Keys;
}

/// <summary>
/// Defines the security level of a data property.
/// </summary>
public enum SecurityLevel
{
    /// <summary>No special security handling required.</summary>
    None = 0,

    /// <summary>Contains sensitive data (PII) that should be masked (e.g., ***).</summary>
    Sensitive = 1,

    /// <summary>Contains highly sensitive data that should be fully redacted (hidden).</summary>
    Redacted = 2
}


