using System;
using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Block;

internal static class BlockExtensionsEmitter
{
    public static void Emit(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {target.Namespace}.Common.DependencyInjection.Internal;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Service collection extensions for the {target.BlockName} building block.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static partial class VK{target.BlockName}BlockExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Adds the {target.BlockName} building block services using configuration.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static IVK{target.BlockName}Builder AddVK{target.BlockName}Block(");
        sb.AppendLine("        this IServiceCollection services,");
        sb.AppendLine("        IConfiguration configuration,");
        sb.AppendLine($"        Func<VK{target.BlockName}Options, VK{target.BlockName}Options>? transform = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(services);");
        sb.AppendLine("        VKGuard.NotNull(configuration);");
        sb.AppendLine($"        return {target.BlockName}BlockRegistration.Register(services, configuration, transform);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"VK{target.BlockName}BlockExtensions.g.cs", sb.ToString());
    }
}
