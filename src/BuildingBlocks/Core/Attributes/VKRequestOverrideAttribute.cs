using System;

namespace VK.Blocks.Core;

/// <summary>
/// Dictates that a property in an IVKBlockOptions class is overrideable at request-time.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class VKRequestOverrideAttribute : Attribute;
