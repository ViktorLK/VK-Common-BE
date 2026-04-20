using System.Collections.Generic;

namespace VK.Blocks.Generators.Authorization.Internal;

/// <summary>
/// Intermediate model for rank enum information used during the generation process.
/// </summary>
/// <param name="Namespace">The namespace of the enum.</param>
/// <param name="Name">The name of the enum.</param>
/// <param name="Members">The list of enum members.</param>
internal sealed record RankEnumInfo(string Namespace, string Name, List<string> Members);
