using System;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Core.Diagnostics;

/// <summary>
/// Type-safe attribute to automatically generate ActivitySource and Meter for a building block.
/// </summary>
/// <typeparam name="TBlock">The building block marker type (implements IVKBlockMarker).</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public sealed class VKBlockDiagnosticsAttribute<TBlock> : Attribute where TBlock : IVKBlockMarker;
