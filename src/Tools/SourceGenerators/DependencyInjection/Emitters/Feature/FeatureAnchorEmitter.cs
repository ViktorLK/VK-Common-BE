using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Feature;

internal static class FeatureAnchorEmitter
{
    public static void Emit(SourceProductionContext ctx, FeatureTarget target)
    {
        var parentBlockName = target.Parent.BlockTypeFullName.Split('.').Last();
        if (parentBlockName.EndsWith("Block"))
            parentBlockName = parentBlockName.Substring(0, parentBlockName.Length - 5);
        if (parentBlockName.StartsWith("VK"))
            parentBlockName = parentBlockName.Substring(2);

        var blockOptionsName = $"VK{parentBlockName}Options";
        var isBlockOptions = target.Options.ClassName == blockOptionsName;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Diagnostics.Metrics;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {target.Options.FullNamespace};");
        sb.AppendLine();
        var lastDotIndex = target.Parent.BlockTypeFullName.LastIndexOf('.');
        var rootNamespace = lastDotIndex > 0 ? target.Parent.BlockTypeFullName.Substring(0, lastDotIndex) : target.Identity.Namespace;
        if (rootNamespace.StartsWith("global::"))
        {
            rootNamespace = rootNamespace.Substring(8);
        }
        
        sb.AppendLine($"namespace {rootNamespace};");
        sb.AppendLine();
        var featureClassName = isBlockOptions ? $"{target.Identity.FeatureName}Block" : $"{target.Identity.FeatureName}Feature";

        sb.AppendLine($"[VKFeatureMarker(\"{target.Identity.FeatureName}\", typeof({target.Parent.BlockTypeFullName}))]");

        var interfaceList = new List<string> { "IVKFeatureMarker", $"IVKBlockMarkerProvider<{featureClassName}>" };

        sb.AppendLine($"internal sealed partial class {featureClassName} : {string.Join(", ", interfaceList)}");
        sb.AppendLine("{");
        sb.AppendLine($"    public const string FeatureName = \"{target.Identity.FeatureName}\";");
        sb.AppendLine($"    public static string FeatureIdentifier => {target.Parent.BlockTypeFullName}.BlockIdentifier + \".\" + FeatureName;");
        sb.AppendLine("    public static string BlockIdentifier => FeatureIdentifier;");
        sb.AppendLine();
        sb.AppendLine($"    public static {featureClassName} Instance {{ get; }} = new();");
        sb.AppendLine($"    static IVKBlockMarker IVKBlockMarkerProvider<{featureClassName}>.Instance => Instance;");
        sb.AppendLine();
        sb.AppendLine("    [global::System.ThreadStatic]");
        sb.AppendLine("    private static bool _isRegistering;");
        sb.AppendLine();

        sb.AppendLine("    public string Name => FeatureName;");
        sb.AppendLine("    public string Identifier => FeatureIdentifier;");
        sb.AppendLine("    public string Version => \"1.0.0\";");
        sb.AppendLine();
        sb.AppendLine($"    public string ParentBlockIdentifier => {target.Parent.BlockTypeFullName}.BlockIdentifier;");
        sb.AppendLine("    public bool IsOptional => true;");
        sb.AppendLine();
        sb.AppendLine($"    public IReadOnlyList<IVKBlockMarker> Dependencies => (IVKBlockMarker[])[{target.Parent.BlockTypeFullName}.Instance];");
        sb.AppendLine();
        sb.AppendLine("    public string ActivitySourceName => FeatureIdentifier;");
        sb.AppendLine("    public string MeterName => FeatureIdentifier;");
        sb.AppendLine();
        sb.AppendLine($"    public static readonly ActivitySource Source = new(FeatureIdentifier);");
        sb.AppendLine($"    public static readonly Meter Meter = new(FeatureIdentifier);");
        sb.AppendLine();
        sb.AppendLine("    // --- Registration Logic ---");
        sb.AppendLine();
        sb.AppendLine($"    public static {target.Identity.BuilderTypeFullName} Register(");
        sb.AppendLine($"        {target.Identity.BuilderTypeFullName} builder,");
        sb.AppendLine($"        Func<{target.Options.ClassName}, {target.Options.ClassName}>? transform = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_isRegistering)");
        sb.AppendLine("        {");
        sb.AppendLine($"            throw new global::System.InvalidOperationException($\"Recursive feature registration detected for {featureClassName}! Do not call Register(builder) inside RegisterFeatureCustom(services, options).\");");
        sb.AppendLine("        }");
        sb.AppendLine("        _isRegistering = true;");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            var services = builder.Services;");
        sb.AppendLine();

        if (target.Parent.BlockTypeFullName.EndsWith("Feature"))
        {
            sb.AppendLine("            // Ensure parent feature is registered (Implicit Pull-up)");
            sb.AppendLine($"            _ = {target.Parent.BlockTypeFullName}.Register(builder);");
            sb.AppendLine();
        }

        if (!isBlockOptions && target.Parent.Toggleable && target.Parent.OptionsTypeFullName is not null)
        {
            sb.AppendLine($"            var parentOptions = services.GetVKBlockOptions<{target.Parent.OptionsTypeFullName}>();");
            sb.AppendLine("            if (parentOptions is not null && !parentOptions.Enabled)");
            sb.AppendLine("            {");
            sb.AppendLine("                return builder;");
            sb.AppendLine("            }");
            sb.AppendLine();
        }

        sb.AppendLine($"            if (services.IsVKBlockRegistered<{featureClassName}>())");
        sb.AppendLine("            {");
        sb.AppendLine("                if (transform is not null)");
        sb.AppendLine("                {");
        sb.AppendLine($"                    _ = services.AddVKBlockOptions<{target.Options.ClassName}>(builder.Configuration!, transform);");
        sb.AppendLine("                }");
        sb.AppendLine("                return builder;");
        sb.AppendLine("            }");
        sb.AppendLine();

        sb.AppendLine($"            var options = services.AddVKBlockOptions<{target.Options.ClassName}>(builder.Configuration!, transform);");
        sb.AppendLine($"            services.AddVKBlockMarker<{featureClassName}>();");
        sb.AppendLine();

        sb.AppendLine($"            services.TryAddEnumerableSingleton<IValidateOptions<{target.Options.ClassName}>, {featureClassName}.FeatureOptionsValidator>();");

        if (target.ArgsGenerationMode != 0)
        {
            var optionsClassName = target.Options.ClassName;
            var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;
            var cleanBaseClassName = baseClassName;
            if (cleanBaseClassName.StartsWith("VK"))
            {
                cleanBaseClassName = cleanBaseClassName.Substring(2);
            }
            sb.AppendLine($"            services.TryAddSingleton<IVK{cleanBaseClassName}OptionsProvider, Default{cleanBaseClassName}OptionsProvider>();");
        }

        if (target.Options.IsToggleable)
        {
            sb.AppendLine();
            sb.AppendLine("            if (!options.Enabled) return builder;");
        }

        sb.AppendLine();
        sb.AppendLine("            RegisterFeatureCustom(services, options);");
        sb.AppendLine();
        sb.AppendLine("            return builder;");
        sb.AppendLine("        }");
        sb.AppendLine("        finally");
        sb.AppendLine("        {");
        sb.AppendLine("            _isRegistering = false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    // [SG Hook] Optional registration hook");
        sb.AppendLine($"    static partial void RegisterFeatureCustom(IServiceCollection services, {target.Options.ClassName} options);");
        sb.AppendLine();

        sb.AppendLine("    // --- Validation Logic ---");
        sb.AppendLine();
        sb.AppendLine($"    internal sealed class FeatureOptionsValidator : IValidateOptions<{target.Options.ClassName}>");
        sb.AppendLine("    {");
        if (!isBlockOptions && target.Parent.Toggleable && target.Parent.OptionsTypeFullName is not null)
        {
            sb.AppendLine($"        private readonly {target.Parent.OptionsTypeFullName}? _parentOptions;");
            sb.AppendLine();
            sb.AppendLine($"        public FeatureOptionsValidator() : this(null) {{ }}");
            sb.AppendLine();
            sb.AppendLine("        [global::Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]");
            sb.AppendLine($"        public FeatureOptionsValidator({target.Parent.OptionsTypeFullName}? parentOptions)");
            sb.AppendLine("        {");
            sb.AppendLine("            _parentOptions = parentOptions;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
        sb.AppendLine($"        public ValidateOptionsResult Validate(string? name, {target.Options.ClassName} options)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (options is null) return ValidateOptionsResult.Fail(\"Options cannot be null.\");");
        sb.AppendLine();
        if (!isBlockOptions && target.Parent.Toggleable && target.Parent.OptionsTypeFullName is not null)
        {
            sb.AppendLine("            if (_parentOptions is not null && !_parentOptions.Enabled) return ValidateOptionsResult.Success;");
            sb.AppendLine();
        }
        if (target.Options.IsToggleable)
        {
            sb.AppendLine("            if (!options.Enabled) return ValidateOptionsResult.Success;");
            sb.AppendLine();
        }
        sb.AppendLine("            var failures = new List<string>();");
        sb.AppendLine($"            {featureClassName}.ValidateFeatureCustom(options, failures);");
        sb.AppendLine("            if (failures.Count > 0) return ValidateOptionsResult.Fail(string.Join(\", \", failures));");
        sb.AppendLine("            return ValidateOptionsResult.Success;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    // [SG Hook] Optional validation hook");
        sb.AppendLine($"    static partial void ValidateFeatureCustom({target.Options.ClassName} options, List<string> failures);");

        sb.AppendLine("}");

        ctx.AddSource($"{featureClassName}.g.cs", sb.ToString());
    }
}
