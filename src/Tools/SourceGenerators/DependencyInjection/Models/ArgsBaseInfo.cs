using System.Collections.Immutable;

namespace VK.Tools.SourceGenerators.DependencyInjection.Models;

internal sealed record ArgsBaseInfo(
    string TypeName,
    string FullNamespace,
    ImmutableArray<PropertyTarget> Properties
);