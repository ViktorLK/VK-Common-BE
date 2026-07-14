using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Block;

internal static class BlockOptionsProviderEmitter
{
    public static void Emit(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var interfaceName = $"IVK{target.BlockName}OptionsProvider";
        var implementationName = $"Default{target.BlockName}OptionsProvider";
        var optionsClassName = $"VK{target.BlockName}Options";

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated provider interface for <see cref=\"{optionsClassName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public partial interface {interfaceName} : IVKOptionsProvider<{optionsClassName}>;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated default implementation of <see cref=\"{interfaceName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed class {implementationName}(IOptions<{optionsClassName}> options) : {interfaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IOptions<{optionsClassName}> _options = options;");
        sb.AppendLine();
        sb.AppendLine($"    public {optionsClassName} GetOptions() => _options.Value;");
        sb.AppendLine("}");

        ctx.AddSource($"{interfaceName}.g.cs", sb.ToString());
    }
}
