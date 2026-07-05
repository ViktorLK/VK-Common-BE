using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.Feature.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Feature.Internal;

internal static class OptionsProviderEmitter
{
    public static void Emit(SourceProductionContext ctx, FeatureTarget target, string? assemblyName)
    {
        var optionsClassName = target.OptionsClassName;
        var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;

        var interfaceName = $"I{baseClassName}OptionsProvider";
        var implementationName = $"{baseClassName}OptionsDefaultProvider";
        var argsName = $"{baseClassName}Args";

        var argsNamespace = assemblyName ?? target.OptionsFullNamespace;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {argsNamespace};");
        if (argsNamespace != target.OptionsFullNamespace)
        {
            sb.AppendLine($"using {target.OptionsFullNamespace};");
        }
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace}.Internal;");
        sb.AppendLine();
        sb.AppendLine($"internal interface {interfaceName} : IVKOptionsProvider<{target.OptionsClassName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    {target.OptionsClassName} GetOptions({argsName}? args = null);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"internal sealed class {implementationName}(IOptions<{target.OptionsClassName}> options) : {interfaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IOptions<{target.OptionsClassName}> _options = options;");
        sb.AppendLine();
        sb.AppendLine($"    public {target.OptionsClassName} GetOptions() => _options.Value;");
        sb.AppendLine($"    public {target.OptionsClassName} GetOptions({argsName}? args = null) => args.Merge(_options.Value);");
        sb.AppendLine("}");

        ctx.AddSource($"{interfaceName}.g.cs", sb.ToString());
    }
}
