using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.Feature.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Feature.Internal;

internal static class ArgsEmitter
{
    public static void Emit(SourceProductionContext ctx, FeatureTarget target, string? assemblyName)
    {
        var optionsClassName = target.OptionsClassName;
        var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;
        var argsClassName = $"{baseClassName}Args";
        var extensionsClassName = $"{argsClassName}Extensions";

        var argsNamespace = assemblyName ?? target.OptionsFullNamespace;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {target.OptionsFullNamespace};");
        if (target.OptionsFullNamespace.Contains(".AI") && argsNamespace != "VK.Blocks.AI")
        {
            sb.AppendLine("using VK.Blocks.AI;");
        }
        sb.AppendLine();
        sb.AppendLine($"namespace {argsNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated request-scoped arguments for <see cref=\"{target.OptionsClassName}\"/>.");
        sb.AppendLine("/// </summary>");

        var isAI = target.OptionsFullNamespace.Contains(".AI");
        var interfaceList = isAI
            ? new List<string> { "IVKAIArgs", $"IVKArgs<{argsClassName}>" }
            : new List<string> { $"IVKArgs<{argsClassName}>" };

        foreach (var overrideInterface in target.ImplementedOverrides)
        {
            if (!interfaceList.Contains(overrideInterface))
            {
                interfaceList.Add(overrideInterface);
            }
        }

        var interfaces = " : " + string.Join(", ", interfaceList);

        sb.AppendLine($"public partial record {argsClassName}{interfaces}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static {argsClassName} Empty {{ get; }} = new();");
        sb.AppendLine();

        if (isAI)
        {
            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public System.Collections.Generic.IDictionary<string, object> Context { get; init; } = new System.Collections.Generic.Dictionary<string, object>();");
            sb.AppendLine();
            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public string? UserId { get; init; }");
            sb.AppendLine();

            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public TimeSpan? Timeout { get; init; }");
            sb.AppendLine();
        }

        foreach (var prop in target.Properties)
        {
            // Skip base AI properties as they are handled explicitly above
            if (isAI && (prop.Name == "Context" || prop.Name == "UserId" || prop.Name == "Timeout"))
            {
                continue;
            }

            var propType = prop.Type;
            if (propType.EndsWith("Options"))
                propType = propType.Substring(0, propType.Length - 7) + "Args";
            else if (propType.EndsWith("Options?"))
                propType = propType.Substring(0, propType.Length - 8) + "Args?";

            var nullableType = prop.IsAlreadyNullable || propType.EndsWith("?") ? propType : $"{propType}?";
            sb.AppendLine($"    public {nullableType} {prop.Name} {{ get; init; }}");
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"public static partial class {extensionsClassName}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static {target.OptionsClassName} Merge(this {argsClassName}? args, {target.OptionsClassName} options)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (args is null) return options;");
        sb.AppendLine();
        sb.AppendLine("        return options with");
        sb.AppendLine("        {");

        foreach (var prop in target.Properties)
        {
            if (isAI && (prop.Name == "Context" || prop.Name == "UserId" || prop.Name == "Timeout"))
            {
                continue;
            }

            if (!prop.ExistsInOptions)
            {
                continue; // Skip merging since it doesn't exist on the Options class
            }

            if (prop.Type.EndsWith("Options") || prop.Type.EndsWith("Options?") || prop.Type.EndsWith("Args") || prop.Type.EndsWith("Args?"))
            {
                sb.AppendLine($"            {prop.Name} = args.{prop.Name}.Merge(options.{prop.Name}),");
            }
            else
            {
                sb.AppendLine($"            {prop.Name} = args.{prop.Name} ?? options.{prop.Name},");
            }
        }

        if (isAI && target.IsTimeoutPresent && target.Properties.All(p => p.Name != "Timeout"))
        {
            sb.AppendLine("            Timeout = args.Timeout ?? options.Timeout,");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{argsClassName}.g.cs", sb.ToString());
    }
}
