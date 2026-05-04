using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;

namespace VK.Blocks.Core.Security;

/// <summary>
/// Cache for property-level security metadata.
/// Optimized for high-performance log masking and data redaction.
/// </summary>
/// <typeparam name="T">The type to cache property metadata for.</typeparam>
public sealed class VKPropertySecurityCache<T>
{
    private static readonly FrozenDictionary<string, VKSecurityLevel> _propertySecurityLevels;

    static VKPropertySecurityCache()
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var dict = new Dictionary<string, VKSecurityLevel>(StringComparer.OrdinalIgnoreCase);

        foreach (PropertyInfo prop in properties)
        {
            if (prop.GetCustomAttribute<VKRedactedAttribute>() is not null)
            {
                dict[prop.Name] = VKSecurityLevel.Redacted;
            }
            else if (prop.GetCustomAttribute<VKSensitiveDataAttribute>() is not null)
            {
                dict[prop.Name] = VKSecurityLevel.Sensitive;
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
    public static VKSecurityLevel GetLevel(string propertyName)
    {
        return _propertySecurityLevels.TryGetValue(propertyName, out VKSecurityLevel level)
            ? level
            : VKSecurityLevel.None;
    }

    /// <summary>
    /// Gets all properties that require masking or redaction.
    /// </summary>
    public static IEnumerable<string> SensitivePropertyNames => _propertySecurityLevels.Keys;
}
