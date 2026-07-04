using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a configuration options class to trigger the automatic generation of 
/// a provider interface and its default implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VKOptionsProviderAttribute : Attribute;
