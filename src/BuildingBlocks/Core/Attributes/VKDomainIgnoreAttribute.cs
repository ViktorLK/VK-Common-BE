using System;

namespace VK.Blocks.Core;

/// <summary>
/// Specifies that a snapshot property should be ignored when source generating domain aggregate forwarding properties.
/// Used when custom forwarding logic (e.g. value object wrapping) is explicitly implemented on the aggregate.
/// Follows AP.01, AP.03.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class VKDomainIgnoreAttribute : Attribute
{
}
