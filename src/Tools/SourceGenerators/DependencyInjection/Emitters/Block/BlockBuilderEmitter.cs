using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Block;

internal static class BlockBuilderEmitter
{
    public static void Emit(SourceProductionContext ctx, BlockTargetInfo target)
    {
        EmitInterface(ctx, target);
        EmitImplementation(ctx, target);
    }

    private static void EmitInterface(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Builder contract for the {target.BlockName} building block.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public partial interface IVK{target.BlockName}Builder : IVKBlockBuilder<{target.ClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("}");

        ctx.AddSource($"IVK{target.BlockName}Builder.g.cs", sb.ToString());
    }

    private static void EmitImplementation(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace}.Common.DependencyInjection.Internal;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Default implementation of the {target.BlockName} builder.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed partial class {target.BlockName}BlockBuilder(");
        sb.AppendLine("    IServiceCollection services,");
        sb.AppendLine("    IConfiguration configuration)");
        sb.AppendLine($"    : VKBlockBuilder<{target.ClassName}>(services, configuration), IVK{target.BlockName}Builder");
        sb.AppendLine("{");
        sb.AppendLine("}");

        ctx.AddSource($"{target.BlockName}BlockBuilder.g.cs", sb.ToString());
    }
}
