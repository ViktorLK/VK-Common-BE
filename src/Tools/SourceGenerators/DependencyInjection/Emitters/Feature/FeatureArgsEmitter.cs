using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Feature;

internal static class FeatureArgsEmitter
{
    public static void Emit(SourceProductionContext ctx, FeatureTarget target, string? assemblyName)
    {
        var optionsClassName = target.Options.ClassName;
        var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;
        var argsClassName = $"{baseClassName}Args";
        var extensionsClassName = $"{argsClassName}Extensions";

        var argsNamespace = assemblyName ?? target.Options.FullNamespace;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {target.Options.FullNamespace};");
        if (target.ArgsBase is not null && argsNamespace != target.ArgsBase.FullNamespace)
        {
            sb.AppendLine($"using {target.ArgsBase.FullNamespace};");
        }
        sb.AppendLine();
        sb.AppendLine($"namespace {argsNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated request-scoped arguments for <see cref=\"{target.Options.ClassName}\"/>.");
        sb.AppendLine("/// </summary>");

        var interfaceList = new List<string> { $"IVKArgs<{argsClassName}>" };
        if (target.ArgsBase is not null)
        {
            interfaceList.Insert(0, target.ArgsBase.TypeName);
        }

        var interfaces = " : " + string.Join(", ", interfaceList);

        sb.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = \"Source-generated request-scoped arguments record.\")]");
        sb.AppendLine($"public partial record {argsClassName}{interfaces}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static {argsClassName} Empty {{ get; }} = new();");
        sb.AppendLine();

        var basePropNames = new HashSet<string>();
        if (target.ArgsBase is not null)
        {
            foreach (var baseProp in target.ArgsBase.Properties)
            {
                var nullableType = baseProp.IsAlreadyNullable || baseProp.Type.EndsWith("?") ? baseProp.Type : $"{baseProp.Type}?";
                // Context dictionary is initialized to non-null dictionary, handle it or similar defaults
                if (baseProp.Name == "Context" && (baseProp.Type.Contains("IDictionary") || baseProp.Type.Contains("Dictionary")))
                {
                    sb.AppendLine("    /// <inheritdoc />");
                    sb.AppendLine($"    public {baseProp.Type} Context {{ get; init; }} = new System.Collections.Generic.Dictionary<string, object>();");
                }
                else
                {
                    sb.AppendLine("    /// <inheritdoc />");
                    sb.AppendLine($"    public {nullableType} {baseProp.Name} {{ get; init; }}");
                }
                sb.AppendLine();
                basePropNames.Add(baseProp.Name);
            }
        }

        foreach (var prop in target.Properties)
        {
            if (basePropNames.Contains(prop.Name))
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
        sb.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = \"Source-generated request-scoped arguments extensions.\")]");
        sb.AppendLine($"public static partial class {extensionsClassName}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static {target.Options.ClassName} Merge(this {argsClassName}? args, {target.Options.ClassName} options)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (args is null) return options;");
        sb.AppendLine();
        sb.AppendLine("        return options with");
        sb.AppendLine("        {");

        // Merge properties from base interface if they exist in options
        if (target.ArgsBase is not null)
        {
            foreach (var baseProp in target.ArgsBase.Properties)
            {
                if (!baseProp.ExistsInOptions)
                {
                    continue;
                }

                if (baseProp.Type.EndsWith("Options") || baseProp.Type.EndsWith("Options?") || baseProp.Type.EndsWith("Args") || baseProp.Type.EndsWith("Args?"))
                {
                    sb.AppendLine($"            {baseProp.Name} = args.{baseProp.Name}.Merge(options.{baseProp.Name}),");
                }
                else
                {
                    sb.AppendLine($"            {baseProp.Name} = args.{baseProp.Name} ?? options.{baseProp.Name},");
                }
            }
        }

        foreach (var prop in target.Properties)
        {
            if (basePropNames.Contains(prop.Name))
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

        // Special fallback merge for Timeout if it is present on Options but not explicitly overridden
        if (target.ArgsBase is not null
            && target.ArgsBase.Properties.Any(p => p.Name == "Timeout")
            && target.Options.IsTimeoutPresent
            && !target.ArgsBase.Properties.First(p => p.Name == "Timeout").ExistsInOptions
            && target.Properties.All(p => p.Name != "Timeout"))
        {
            sb.AppendLine("            Timeout = args.Timeout ?? options.Timeout,");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{argsClassName}.g.cs", sb.ToString());
    }
}
