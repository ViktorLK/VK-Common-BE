using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Feature;

internal static class FeatureOptionsProviderEmitter
{
    public static void Emit(SourceProductionContext ctx, FeatureTarget target, string? assemblyName)
    {
        var optionsClassName = target.Options.ClassName;
        var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;

        var cleanBaseClassName = baseClassName;
        if (cleanBaseClassName.StartsWith("VK"))
        {
            cleanBaseClassName = cleanBaseClassName.Substring(2);
        }

        var interfaceName = $"IVK{cleanBaseClassName}OptionsProvider";
        var implementationName = $"Default{cleanBaseClassName}OptionsProvider";
        var argsName = $"{baseClassName}Args";

        var argsNamespace = assemblyName ?? target.Options.FullNamespace;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {argsNamespace};");
        if (argsNamespace != target.Options.FullNamespace)
        {
            sb.AppendLine($"using {target.Options.FullNamespace};");
        }
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Options.FullNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public interface {interfaceName} : IVKOptionsProvider<{target.Options.ClassName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    {target.Options.ClassName} GetOptions({argsName}? args = null);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = \"Source-generated options provider boilerplate.\")]");
        sb.AppendLine($"internal sealed class {implementationName}(IOptions<{target.Options.ClassName}> options) : {interfaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IOptions<{target.Options.ClassName}> _options = options;");
        sb.AppendLine();
        sb.AppendLine($"    public {target.Options.ClassName} GetOptions() => _options.Value;");
        sb.AppendLine($"    public {target.Options.ClassName} GetOptions({argsName}? args = null) => args.Merge(_options.Value);");
        sb.AppendLine("}");

        ctx.AddSource($"{interfaceName}.g.cs", sb.ToString());
    }
}
