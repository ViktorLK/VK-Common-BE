using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core.Security;

/// <summary>
/// Provides high-performance caching for property-level security metadata using Static Generic Caching.
/// Optimized for log masking and data redaction.
/// </summary>
public sealed class VKPropertySecurityCache
{
    [ExcludeFromCodeCoverage(Justification = "Utility class with static members only")]
    private VKPropertySecurityCache()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the type has any security-sensitive properties.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasSensitiveProperties<T>() => InnerCache<T>.HasSensitiveProperties;

    /// <summary>
    /// Gets a value indicating whether the type has any security-sensitive properties.
    /// </summary>
    /// <param name="type">The type to check.</param>
    public static bool HasSensitiveProperties(Type type)
    {
        VKGuard.NotNull(type);

        // Fast path for runtime types via reflection to the same static generic cache
        var property = typeof(InnerCache<>)
            .MakeGenericType(type)
            .GetField(nameof(InnerCache<object>.HasSensitiveProperties), BindingFlags.Public | BindingFlags.Static);

        return (bool?)property?.GetValue(null) ?? false;
    }

    /// <summary>
    /// Gets the security level for a specific property.
    /// </summary>
    /// <typeparam name="T">The type containing the property.</typeparam>
    /// <param name="propertyName">Name of the property.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKSecurityLevel GetLevel<T>(string propertyName)
        => InnerCache<T>.GetLevel(propertyName);

    /// <summary>
    /// Gets the security level for a specific property for a runtime type.
    /// </summary>
    /// <param name="type">The type containing the property.</param>
    /// <param name="propertyName">Name of the property.</param>
    public static VKSecurityLevel GetLevel(Type type, string propertyName)
    {
        VKGuard.NotNull(type);
        VKGuard.NotNullOrWhiteSpace(propertyName);

        // Use the generic cache even for runtime types to ensure consistency and single initialization
        var method = typeof(InnerCache<>)
            .MakeGenericType(type)
            .GetMethod(nameof(InnerCache<object>.GetLevel), BindingFlags.Public | BindingFlags.Static);

        return (VKSecurityLevel?)method?.Invoke(null, [propertyName]) ?? VKSecurityLevel.None;
    }

    /// <summary>
    /// Gets all properties that require masking or redaction.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> GetSensitivePropertyNames<T>()
        => InnerCache<T>.SensitivePropertyNames;

    /// <summary>
    /// Static Generic Cache implementation.
    /// This is private to avoid CA1000 on public API while retaining performance.
    /// </summary>
    private static class InnerCache<T>
    {
        private static readonly FrozenDictionary<string, VKSecurityLevel> _levels;

        // Static fields for direct high-speed access
        public static readonly bool HasSensitiveProperties;
        public static readonly IEnumerable<string> SensitivePropertyNames;

        static InnerCache()
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

            _levels = dict.ToFrozenDictionary();
            HasSensitiveProperties = _levels.Count > 0;
            SensitivePropertyNames = _levels.Keys;
        }

        public static VKSecurityLevel GetLevel(string propertyName)
        {
            return _levels.TryGetValue(propertyName, out VKSecurityLevel level)
                ? level
                : VKSecurityLevel.None;
        }
    }
}
