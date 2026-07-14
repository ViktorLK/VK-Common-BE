namespace VK.Tools.SourceGenerators.DependencyInjection.Models;

internal sealed record PropertyTarget(string Name, string Type, bool IsAlreadyNullable, bool ExistsInOptions);
