using System;

namespace VK.Blocks.Core;

/// <summary>
/// Dictates that a property in an IVKBlockOptions class is read-only at request-time.
/// It will not be generated into the request-level Args record when Implicit mode is used.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class VKNoRequestOverrideAttribute : Attribute;
