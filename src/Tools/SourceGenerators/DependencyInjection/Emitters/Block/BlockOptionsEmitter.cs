using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Block;

internal static class BlockOptionsEmitter
{
    public static void Emit(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Configuration options for the {target.BlockName} building block.");
        sb.AppendLine("/// </summary>");
        var baseInterface = target.GenerateToggleableMembers ? " : IVKToggleableBlockOptions" : " : IVKBlockOptions";
        sb.AppendLine($"public sealed partial record VK{target.BlockName}Options" + baseInterface);
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// The configuration section name.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static string SectionName => $\"{{VKBlocksConstants.VKBlocksConfigPrefix}}:{target.BlockName}\";");
        sb.AppendLine();
        if (target.GenerateToggleableMembers)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets or sets a value indicating whether the block is enabled.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public bool Enabled { get; init; } = true;");
        }
        sb.AppendLine("}");

        ctx.AddSource($"VK{target.BlockName}Options.g.cs", sb.ToString());
    }
}
