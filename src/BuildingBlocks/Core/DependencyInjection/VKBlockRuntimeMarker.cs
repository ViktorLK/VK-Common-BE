namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Internal marker used to track registered building blocks by their unique identifier.
/// This ensures that dependency validation is decoupled from the specific registration type.
/// </summary>
internal sealed record VKBlockRuntimeMarker(string Identifier);
