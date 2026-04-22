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
internal sealed class PropertySecurityCache<T>
{
    private static readonly FrozenDictionary<string, SecurityLevel> _propertySecurityLevels;

    static PropertySecurityCache()
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var dict = new Dictionary<string, SecurityLevel>(StringComparer.OrdinalIgnoreCase);

        foreach (PropertyInfo prop in properties)
        {
            if (prop.GetCustomAttribute<VKRedactedAttribute>() is not null)
            {
                dict[prop.Name] = SecurityLevel.Redacted;
            }
            else if (prop.GetCustomAttribute<VKSensitiveDataAttribute>() is not null)
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
