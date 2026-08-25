using Microsoft.CodeAnalysis;

namespace VK.Tools.SourceGenerators.DependencyInjection.Models;

internal sealed record BlockTargetInfo(
    string Namespace,
    string ClassName,
    string BlockName,
    bool GenerateToggleableMembers,
    bool Toggleable,
    bool HasGeneratedFeature,
    bool HasPersistEntities,
    bool IsPartial,
    Location Location);
