using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Block;

internal static class BlockAnchorEmitter
{
    public static void Emit(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated registration and validation hub for {target.BlockName} block.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed partial class {target.ClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    [global::System.ThreadStatic]");
        sb.AppendLine("    private static bool _isRegistering;");
        sb.AppendLine();
        sb.AppendLine($"    internal static void Register(IVK{target.BlockName}Builder builder)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(builder);");
        sb.AppendLine("        if (_isRegistering)");
        sb.AppendLine("        {");
        sb.AppendLine($"            throw new global::System.InvalidOperationException($\"Recursive block registration detected for {target.ClassName}! Do not call Register(builder) inside RegisterBlockCustom(builder).\");");
        sb.AppendLine("        }");
        sb.AppendLine("        _isRegistering = true;");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            RegisterBlockCustom(builder);");
        sb.AppendLine("        }");
        sb.AppendLine("        finally");
        sb.AppendLine("        {");
        sb.AppendLine("            _isRegistering = false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    static partial void RegisterBlockCustom(IVK{target.BlockName}Builder builder);");
        sb.AppendLine($"    static partial void ValidateBlockCustom(VK{target.BlockName}Options options, List<string> failures);");
        sb.AppendLine();
        sb.AppendLine($"    internal sealed class OptionsValidator : IValidateOptions<VK{target.BlockName}Options>");
        sb.AppendLine("    {");
        sb.AppendLine($"        public ValidateOptionsResult Validate(string? name, VK{target.BlockName}Options options)");
        sb.AppendLine("        {");
        sb.AppendLine("            VKGuard.NotNull(options);");
        sb.AppendLine();
        if (target.Toggleable)
        {
            sb.AppendLine("            if (!options.Enabled) return ValidateOptionsResult.Success;");
            sb.AppendLine();
        }
        sb.AppendLine("            var failures = new List<string>();");
        sb.AppendLine($"            {target.ClassName}.ValidateBlockCustom(options, failures);");
        sb.AppendLine("            if (failures.Count > 0) return ValidateOptionsResult.Fail(string.Join(\", \", failures));");
        sb.AppendLine("            return ValidateOptionsResult.Success;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{target.BlockName}Block.g.cs", sb.ToString());
    }
}
