using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a record, class, or struct as a DDD Single-Value Object (e.g. <c>VKEmail</c> wrapping a primitive <c>string Value</c>).
/// Enables Source Generators to automatically unpack (.Value) when mapping to Persistence Entities
/// and pack (.Create(value).Value!) when hydrating Domain Snapshots.
/// Follows AP.01, AP.03, CS.01.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class VKValueObjectAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the underlying raw value property. Defaults to "Value".
    /// </summary>
    public string ValuePropertyName { get; init; } = "Value";
}
