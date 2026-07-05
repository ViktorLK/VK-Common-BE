using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.Feature.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Feature.Internal;

internal static class FeatureAnchorEmitter
{
    public static void Emit(SourceProductionContext ctx, FeatureTarget target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Diagnostics.Metrics;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {target.OptionsFullNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace}.Internal;");
        sb.AppendLine();
        sb.AppendLine($"[VKFeatureMarker(\"{target.FeatureName}\", typeof({target.ParentBlockTypeFullName}))]");

        var interfaceList = new List<string> { "IVKFeatureMarker", $"IVKBlockMarkerProvider<{target.FeatureName}Feature>", $"IValidateOptions<{target.OptionsClassName}>" };

        sb.AppendLine($"internal sealed partial class {target.FeatureName}Feature : {string.Join(", ", interfaceList)}");
        sb.AppendLine("{");
        sb.AppendLine($"    public const string FeatureName = \"{target.FeatureName}\";");
        sb.AppendLine($"    public static string FeatureIdentifier => {target.ParentBlockTypeFullName}.BlockIdentifier + \".\" + FeatureName;");
        sb.AppendLine("    public static string BlockIdentifier => FeatureIdentifier;");
        sb.AppendLine();
        sb.AppendLine($"    public static {target.FeatureName}Feature Instance {{ get; }} = new();");
        sb.AppendLine($"    static IVKBlockMarker IVKBlockMarkerProvider<{target.FeatureName}Feature>.Instance => Instance;");
        sb.AppendLine();
        sb.AppendLine("    public string Name => FeatureName;");
        sb.AppendLine("    public string Identifier => FeatureIdentifier;");
        sb.AppendLine("    public string Version => \"1.0.0\";");
        sb.AppendLine();
        sb.AppendLine($"    public string ParentBlockIdentifier => {target.ParentBlockTypeFullName}.BlockIdentifier;");
        sb.AppendLine("    public bool IsOptional => true;");
        sb.AppendLine();
        sb.AppendLine($"    public IReadOnlyList<IVKBlockMarker> Dependencies => (IVKBlockMarker[])[{target.ParentBlockTypeFullName}.Instance];");
        sb.AppendLine();
        sb.AppendLine("    public string ActivitySourceName => FeatureIdentifier;");
        sb.AppendLine("    public string MeterName => FeatureIdentifier;");
        sb.AppendLine();
        sb.AppendLine($"    public static readonly ActivitySource Source = new(FeatureIdentifier);");
        sb.AppendLine($"    public static readonly Meter Meter = new(FeatureIdentifier);");
        sb.AppendLine();
        sb.AppendLine("    // --- Registration Logic ---");
        sb.AppendLine();
        sb.AppendLine($"    public static {target.BuilderTypeFullName} Register(");
        sb.AppendLine($"        {target.BuilderTypeFullName} builder,");
        sb.AppendLine($"        Func<{target.OptionsClassName}, {target.OptionsClassName}>? transform = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        var services = builder.Services;");
        sb.AppendLine();
        sb.AppendLine($"        if (services.IsVKBlockRegistered<{target.FeatureName}Feature>())");
        sb.AppendLine("        {");
        sb.AppendLine("            if (transform is not null)");
        sb.AppendLine("            {");
        sb.AppendLine($"                _ = services.AddVKBlockOptions<{target.OptionsClassName}>(builder.Configuration!, transform);");
        sb.AppendLine("            }");
        sb.AppendLine("            return builder;");
        sb.AppendLine("        }");
        sb.AppendLine();

        if (target.ParentBlockTypeFullName.EndsWith("Feature"))
        {
            sb.AppendLine("        // Ensure parent feature is registered (Implicit Pull-up)");
            sb.AppendLine($"        _ = {target.ParentBlockTypeFullName}.Register(builder);");
            sb.AppendLine();
        }

        sb.AppendLine($"        var options = services.AddVKBlockOptions<{target.OptionsClassName}>(builder.Configuration!, transform);");
        sb.AppendLine($"        services.AddVKBlockMarker<{target.FeatureName}Feature>();");
        sb.AppendLine();

        sb.AppendLine($"        services.TryAddEnumerableSingleton<IValidateOptions<{target.OptionsClassName}>, {target.FeatureName}Feature>();");

        if (target.GenerateArgs)
        {
            var optionsClassName = target.OptionsClassName;
            var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;
            sb.AppendLine($"        services.TryAddSingleton<I{baseClassName}OptionsProvider, {baseClassName}OptionsDefaultProvider>();");
        }

        if (target.IsToggleable)
        {
            sb.AppendLine();
            sb.AppendLine("        if (!options.Enabled) return builder;");
        }

        sb.AppendLine();
        sb.AppendLine("        RegisterFeatureCustom(services, options);");
        sb.AppendLine();
        sb.AppendLine("        return builder;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    // [SG Hook] Optional registration hook");
        sb.AppendLine($"    static partial void RegisterFeatureCustom(IServiceCollection services, {target.OptionsClassName} options);");
        sb.AppendLine();

        sb.AppendLine("    // --- Validation Logic ---");
        sb.AppendLine();
        sb.AppendLine($"    ValidateOptionsResult IValidateOptions<{target.OptionsClassName}>.Validate(string? name, {target.OptionsClassName} options)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (options is null) return ValidateOptionsResult.Fail(\"Options cannot be null.\");");
        sb.AppendLine();
        if (target.IsToggleable)
        {
            sb.AppendLine("        if (!options.Enabled) return ValidateOptionsResult.Success;");
            sb.AppendLine();
        }
        sb.AppendLine("        var failures = new List<string>();");
        sb.AppendLine("        ValidateFeatureCustom(options, failures);");
        sb.AppendLine("        if (failures.Count > 0) return ValidateOptionsResult.Fail(string.Join(\", \", failures));");
        sb.AppendLine("        return ValidateOptionsResult.Success;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    // [SG Hook] Optional validation hook");
        sb.AppendLine($"    static partial void ValidateFeatureCustom({target.OptionsClassName} options, List<string> failures);");

        sb.AppendLine("}");

        ctx.AddSource($"{target.FeatureName}Feature.g.cs", sb.ToString());
    }
}
