using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.DependencyInjection;

/// <summary>
/// Source generator that automates feature boilerplate (Constants, Marker, Registration).
/// Triggered by [VKFeature] attribute on Options records/classes.
/// </summary>
[Generator]
public sealed class VKFeatureGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKFeatureAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var targets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is RecordDeclarationSyntax or ClassDeclarationSyntax,
                transform: GetTarget)
            .WhereNotNull();

        context.RegisterSourceOutput(
            targets.Combine(assemblyName),
            (ctx, pair) => EmitSource(ctx, pair.Left, pair.Right, this.GetType()));
    }

    private static FeatureTarget? GetTarget(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, ct) as INamedTypeSymbol;

        if (symbol is null) return null;

        var attribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeFullName);

        if (attribute is null) return null;

        // Extract attribute data
        var args = attribute.ConstructorArguments;
        if (args.Length < 1) return null;

        var parentTypeSymbol = args[0].Value as INamedTypeSymbol;
        if (parentTypeSymbol is null) return null;

        // 1. Name Inference
        var explicitName = args.Length > 1 ? args[1].Value?.ToString() : null;
        var featureName = explicitName ?? InferName(symbol.Name);

        // 2. Namespace Inheritance
        var namespaceOverride = attribute.NamedArguments.FirstOrDefault(n => n.Key == "Namespace").Value.Value?.ToString();
        var parentNs = parentTypeSymbol.ContainingNamespace.ToDisplayString();
        if (parentNs.EndsWith(".Internal")) parentNs = parentNs.Substring(0, parentNs.Length - 9);
        var targetNamespace = namespaceOverride ?? $"{parentNs}.{featureName}";

        // 3. Flags
        var generateArgs = attribute.NamedArguments.FirstOrDefault(n => n.Key == "GenerateArgs").Value.Value as bool? ?? false;
        var generateValidator = attribute.NamedArguments.FirstOrDefault(n => n.Key == "GenerateValidator").Value.Value as bool? ?? true;
        var sectionNameOverride = attribute.NamedArguments.FirstOrDefault(n => n.Key == "SectionName").Value.Value?.ToString();

        // 4. Structural Info
        var isToggleable = symbol.AllInterfaces.Any(i => i.Name == "IVKToggleableBlockOptions");
        var isPartial = typeDeclaration.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword));
        var isAISettings = symbol.AllInterfaces.Any(i => i.Name == "IVKAIProviderSettings");
        var isGovernanceSettings = symbol.AllInterfaces.Any(i => i.Name == "IVKAIGovernanceSettings");

        // --- Mode B: Strict Interface Mapping ---
        var overridableProperties = new HashSet<string>(StringComparer.Ordinal);
        var implementedOverrides = new List<string>();

        foreach (var @interface in symbol.AllInterfaces)
        {
            var interfaceName = @interface.Name;
            string? overridesName = null;

            if (interfaceName.EndsWith("Overrides"))
            {
                overridesName = interfaceName;
            }
            else if (interfaceName.EndsWith("Settings"))
            {
                // Mapping: IVK...Settings -> IVK...Overrides
                overridesName = interfaceName.Replace("Settings", "Overrides");
            }

            if (overridesName != null)
            {
                // Try to find the interface in the compilation
                var overridesSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName($"VK.Blocks.AI.{overridesName}") 
                                      ?? context.SemanticModel.Compilation.GetTypeByMetadataName($"VK.Blocks.AI.Chat.{overridesName}")
                                      ?? @interface.ContainingNamespace.GetTypeMembers(overridesName).FirstOrDefault();

                if (overridesSymbol != null)
                {
                    implementedOverrides.Add(overridesName);
                    foreach (var member in overridesSymbol.GetMembers().OfType<IPropertySymbol>())
                    {
                        overridableProperties.Add(member.Name);
                    }
                }
            }
        }

        var properties = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public 
                        && !p.IsStatic 
                        && !p.IsReadOnly 
                        && overridableProperties.Contains(p.Name)) // STRICT MODE: Must be in Overrides interface
            .Select(p => new PropertyTarget(
                Name: p.Name,
                Type: p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier)),
                IsAlreadyNullable: p.Type.NullableAnnotation == NullableAnnotation.Annotated || p.Type.ToDisplayString().EndsWith("?")
            ))
            .ToImmutableArray();

        var isTimeoutPresent = symbol.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == "Timeout");

        return new FeatureTarget(
            Namespace: targetNamespace,
            OptionsClassName: symbol.Name,
            OptionsFullNamespace: symbol.ContainingNamespace.ToDisplayString(),
            FeatureName: featureName,
            ParentBlockTypeFullName: parentTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            GenerateArgs: generateArgs,
            GenerateValidator: generateValidator,
            SectionNameOverride: sectionNameOverride,
            IsToggleable: isToggleable,
            IsPartial: isPartial,
            IsAISettings: isAISettings,
            IsGovernanceSettings: isGovernanceSettings,
            IsTimeoutPresent: isTimeoutPresent,
            ImplementedOverrides: implementedOverrides.ToImmutableArray(),
            Properties: properties
        );
    }

    private static string InferName(string className)
    {
        var name = className;
        if (name.StartsWith("VK")) name = name.Substring(2);
        if (name.EndsWith("Options")) name = name.Substring(0, name.Length - 7);
        return name;
    }

    private static void EmitSource(SourceProductionContext ctx, FeatureTarget target, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName)) return;

        // 1. Generate Consolidated Feature Anchor (Marker + Registration + Validator)
        EmitFeatureAnchor(ctx, target);

        // 5. Generate Behavioral Boilerplate (Args & Provider)
        if (target.GenerateArgs)
        {
            EmitArgs(ctx, target);
            EmitProvider(ctx, target);
        }

        // 6. Generate Options Partial (if applicable)
        if (target.IsPartial)
        {
            EmitOptionsPartial(ctx, target);
        }
    }

    private static void EmitArgs(SourceProductionContext ctx, FeatureTarget target)
    {
        var optionsClassName = target.OptionsClassName;
        var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;
        var argsClassName = $"{baseClassName}Args";
        var extensionsClassName = $"{argsClassName}Extensions";

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using VK.Blocks.Core;");
        if (target.OptionsFullNamespace.Contains(".AI"))
        {
            sb.AppendLine("using VK.Blocks.AI;");
            sb.AppendLine("using VK.Blocks.AI.Chat;");
            sb.AppendLine("using VK.Blocks.AI.Agents;");
            sb.AppendLine("using VK.Blocks.AI.Text;");
            sb.AppendLine("using VK.Blocks.AI.Audio;");
            sb.AppendLine("using VK.Blocks.AI.Vectorics;");
            sb.AppendLine("using VK.Blocks.AI.Tokenics;");
            sb.AppendLine("using VK.Blocks.AI.Guardrails;");
        }
        sb.AppendLine();
        sb.AppendLine($"namespace {target.OptionsFullNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Automatically generated request-scoped arguments for <see cref=\"{target.OptionsClassName}\"/>.");
        sb.AppendLine("/// </summary>");
        
        var isAI = target.OptionsFullNamespace.Contains(".AI");
        var interfaceList = new List<string> { "IVKAIArgs", $"IVKArgs<{argsClassName}>" };
        
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
            if (propType.EndsWith("Options")) propType = propType.Substring(0, propType.Length - 7) + "Args";
            else if (propType.EndsWith("Options?")) propType = propType.Substring(0, propType.Length - 8) + "Args?";

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

            if (prop.Type.EndsWith("Options") || prop.Type.EndsWith("Options?"))
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

    private static void EmitProvider(SourceProductionContext ctx, FeatureTarget target)
    {
        var optionsClassName = target.OptionsClassName;
        var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;
        
        var interfaceName = $"I{baseClassName}Provider";
        var implementationName = $"{baseClassName}DefaultProvider";
        var argsName = $"{baseClassName}Args";

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
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

    private static void EmitFeatureAnchor(SourceProductionContext ctx, FeatureTarget target)
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
        
        var interfaceList = new List<string> { "IVKFeatureMarker", $"IVKBlockMarkerProvider<{target.FeatureName}Feature>" };
        if (target.GenerateValidator) interfaceList.Add($"IValidateOptions<{target.OptionsClassName}>");
        
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
        sb.AppendLine("    public static IVKAIBuilder Register(");
        sb.AppendLine("        IVKAIBuilder builder,");
        sb.AppendLine($"        Func<{target.OptionsClassName}, {target.OptionsClassName}>? transform = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        var services = builder.Services;");
        sb.AppendLine();
        sb.AppendLine($"        if (services.IsVKBlockRegistered<{target.FeatureName}Feature>()) return builder;");
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
        
        if (target.GenerateValidator)
        {
            sb.AppendLine($"        services.TryAddEnumerableSingleton<IValidateOptions<{target.OptionsClassName}>, {target.FeatureName}Feature>();");
        }
        
        if (target.GenerateArgs)
        {
            var optionsClassName = target.OptionsClassName;
            var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;
            sb.AppendLine($"        services.TryAddSingleton<I{baseClassName}Provider, {baseClassName}DefaultProvider>();");
        }
        
        if (target.IsToggleable)
        {
            sb.AppendLine();
            sb.AppendLine("        if (!options.Enabled) return builder;");
        }
        
        sb.AppendLine();
        sb.AppendLine("        RegisterCustom(services, options);");
        sb.AppendLine();
        sb.AppendLine("        return builder;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    // [SG Hook] Optional registration hook");
        sb.AppendLine($"    static partial void RegisterCustom(IServiceCollection services, {target.OptionsClassName} options);");
        sb.AppendLine();

        if (target.GenerateValidator)
        {
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
            sb.AppendLine("        ValidateCustom(options, failures);");
            sb.AppendLine("        if (failures.Count > 0) return ValidateOptionsResult.Fail(string.Join(\", \", failures));");
            sb.AppendLine("        return ValidateOptionsResult.Success;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    // [SG Hook] Optional validation hook");
            sb.AppendLine($"    static partial void ValidateCustom({target.OptionsClassName} options, List<string> failures);");
        }

        sb.AppendLine("}");
        
        ctx.AddSource($"{target.FeatureName}Feature.g.cs", sb.ToString());
    }

    private static void EmitOptionsPartial(SourceProductionContext ctx, FeatureTarget target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.OptionsFullNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public partial record {target.OptionsClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>The configuration section name, automatically generated by VK.Blocks.</summary>");
        
        // At runtime, transform "VK.Blocks.AI.Tokenics" to "AI:Tokenics"
        var parentIdExpr = $"{target.ParentBlockTypeFullName}.BlockIdentifier.Replace(\"VK.Blocks.\", \"\").Replace(\".\", \":\")";
        
        sb.AppendLine($"    public static string SectionName => {parentIdExpr} + \":\" + \"{target.FeatureName}\";");
        sb.AppendLine("}");
        
        ctx.AddSource($"{target.OptionsClassName}.Feature.g.cs", sb.ToString());
    }

    private sealed record FeatureTarget(
        string Namespace, 
        string OptionsClassName, 
        string OptionsFullNamespace,
        string FeatureName, 
        string ParentBlockTypeFullName,
        bool GenerateArgs,
        bool GenerateValidator,
        string? SectionNameOverride,
        bool IsToggleable,
        bool IsPartial,
        bool IsAISettings,
        bool IsGovernanceSettings,
        bool IsTimeoutPresent,
        ImmutableArray<string> ImplementedOverrides,
        ImmutableArray<PropertyTarget> Properties);

    private sealed record PropertyTarget(string Name, string Type, bool IsAlreadyNullable);
}
