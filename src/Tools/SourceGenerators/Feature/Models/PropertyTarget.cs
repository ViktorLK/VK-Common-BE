namespace VK.Tools.SourceGenerators.Feature.Models;

internal sealed record PropertyTarget(string Name, string Type, bool IsAlreadyNullable, bool ExistsInOptions);
