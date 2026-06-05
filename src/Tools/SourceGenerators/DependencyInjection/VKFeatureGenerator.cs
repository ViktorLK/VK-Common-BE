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

        if (symbol is null)
            return null;

        var attribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeFullName);

        if (attribute is null)
            return null;

        // Extract attribute data
        var args = attribute.ConstructorArguments;
        if (args.Length < 1)
            return null;

        var parentTypeSymbol = args[0].Value as INamedTypeSymbol;
        if (parentTypeSymbol is null)
            return null;

        // 1. Name Inference
        var explicitName = args.Length > 1 ? args[1].Value?.ToString() : null;
        var featureName = explicitName ?? InferName(symbol.Name);

        // 2. Namespace Inheritance
        var namespaceOverride = attribute.NamedArguments.FirstOrDefault(n => n.Key == "Namespace").Value.Value?.ToString();
        var parentNs = parentTypeSymbol.ContainingNamespace.ToDisplayString();
        if (parentNs.EndsWith(".Internal"))
            parentNs = parentNs.Substring(0, parentNs.Length - 9);
        var targetNamespace = namespaceOverride ?? $"{parentNs}.{featureName}";

        // 3. Flags
        var generateArgs = attribute.NamedArguments.FirstOrDefault(n => n.Key == "GenerateArgs").Value.Value as bool? ?? false;
        var generateValidator = attribute.NamedArguments.FirstOrDefault(n => n.Key == "GenerateValidator").Value.Value as bool? ?? true;
        var sectionNameOverride = attribute.NamedArguments.FirstOrDefault(n => n.Key == "SectionName").Value.Value?.ToString();

        // 4. Structural Info
        var isToggleable = symbol.AllInterfaces.Any(i => i.Name == "IVKToggleableBlockOptions");
        var isPartial = typeDeclaration.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword));
        var isAISettings = false;
        var isGovernanceSettings = false;

        var overridableProperties = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
        var implementedOverrides = new List<string>();

        foreach (var @interface in symbol.AllInterfaces)
        {
            var interfaceName = @interface.Name;
            string? overridesName = null;

            if (interfaceName.EndsWith("Overrides"))
            {
                overridesName = interfaceName;
            }
            else if (interfaceName.EndsWith("Options") && interfaceName.StartsWith("IVK") && interfaceName != "IVKBlockOptions" && interfaceName != "IVKToggleableBlockOptions")
            {
                // Mapping: IVK...Options -> IVK...Overrides
                overridesName = interfaceName.Replace("Options", "Overrides");
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

                    // Add direct members
                    foreach (var member in overridesSymbol.GetMembers().OfType<IPropertySymbol>())
                    {
                        overridableProperties[member.Name] = member;
                    }

                    // Add inherited members recursively
                    foreach (var inheritedInterface in overridesSymbol.AllInterfaces)
                    {
                        foreach (var member in inheritedInterface.GetMembers().OfType<IPropertySymbol>())
                        {
                            overridableProperties[member.Name] = member;
                        }
                    }
                }
            }
        }

        // Get options properties for existence check
        var optionsProperties = new System.Collections.Generic.HashSet<string>(
            symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && !p.IsReadOnly)
                .Select(p => p.Name),
            StringComparer.Ordinal
        );

        var properties = overridableProperties.Values
            .Select(p => new PropertyTarget(
                Name: p.Name,
                Type: p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier)),
                IsAlreadyNullable: p.Type.NullableAnnotation == NullableAnnotation.Annotated || p.Type.ToDisplayString().EndsWith("?"),
                ExistsInOptions: optionsProperties.Contains(p.Name)
            ))
            .ToImmutableArray();

        var isTimeoutPresent = symbol.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == "Timeout");

        var builderTypeFullName = GetBuilderTypeFullName(parentTypeSymbol);

        var parentSegment = ResolveSegment(parentTypeSymbol);
        var computedSectionName = sectionNameOverride ?? $"{parentSegment}:{featureName}";

        return new FeatureTarget(
            Namespace: targetNamespace,
            OptionsClassName: symbol.Name,
            OptionsFullNamespace: symbol.ContainingNamespace.ToDisplayString(),
            FeatureName: featureName,
            ParentBlockTypeFullName: parentTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            BuilderTypeFullName: builderTypeFullName,
            GenerateArgs: generateArgs,
            GenerateValidator: generateValidator,
            SectionNameOverride: sectionNameOverride,
            IsToggleable: isToggleable,
            IsPartial: isPartial,
            IsAISettings: isAISettings,
            IsGovernanceSettings: isGovernanceSettings,
            IsTimeoutPresent: isTimeoutPresent,
            ImplementedOverrides: implementedOverrides.ToImmutableArray(),
            Properties: properties,
            ComputedSectionName: computedSectionName
        );
    }

    private static string GetBuilderTypeFullName(INamedTypeSymbol parentTypeSymbol)
    {
        // 1. If parent ends with "Block", resolve its builder name directly.
        if (parentTypeSymbol.Name.EndsWith("Block"))
        {
            var parentTypeName = parentTypeSymbol.Name;
            var blockName = parentTypeName;
            if (blockName.StartsWith("VK"))
                blockName = blockName.Substring(2);
            if (blockName.EndsWith("Block"))
                blockName = blockName.Substring(0, blockName.Length - 5);

            var parentNs = parentTypeSymbol.ContainingNamespace.ToDisplayString();
            return $"global::{parentNs}.IVK{blockName}Builder";
        }

        // 2. Otherwise, check if it's a feature marker that has [VKFeatureMarker(..., typeof(Parent))]
        var featureMarkerAttr = parentTypeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "VKFeatureMarkerAttribute" || a.AttributeClass?.ToDisplayString().EndsWith("VKFeatureMarkerAttribute") == true);

        if (featureMarkerAttr != null && featureMarkerAttr.ConstructorArguments.Length > 1)
        {
            var grandParentType = featureMarkerAttr.ConstructorArguments[1].Value as INamedTypeSymbol;
            if (grandParentType != null)
            {
                return GetBuilderTypeFullName(grandParentType);
            }
        }

        // 3. Generic Assembly-Based Block Resolution (100% generic, zero hardcoding)
        var assemblyName = parentTypeSymbol.ContainingAssembly?.Name;
        if (assemblyName != null && assemblyName.StartsWith("VK.Blocks"))
        {
            var moduleName = assemblyName.Substring(9).TrimStart('.'); // e.g. "AI", "AI.Cognitive", "Authorization"
            var blockName = moduleName.Replace(".", ""); // e.g. "AI", "AICognitive", "Authorization"
            return $"global::{assemblyName}.IVK{blockName}Builder";
        }

        // 4. Generic Namespace Segment Fallback (100% generic, zero hardcoding)
        var fullNs = parentTypeSymbol.ContainingNamespace.ToDisplayString();
        var segments = fullNs.Split('.');
        var segmentCount = segments.Length;

        // Trim common trailing technical namespace suffixes
        while (segmentCount > 0 && (
            segments[segmentCount - 1] == "Internal" ||
            segments[segmentCount - 1] == "DependencyInjection" ||
            segments[segmentCount - 1] == "Common" ||
            segments[segmentCount - 1] == "Shared" ||
            segments[segmentCount - 1] == "Contracts" ||
            segments[segmentCount - 1] == "Protocols"))
        {
            segmentCount--;
        }

        if (segmentCount >= 3 && segments[0] == "VK" && segments[1] == "Blocks")
        {
            // The last segment of a cleaned feature namespace is always the feature name itself.
            // Everything before it is the block's root namespace.
            // If the cleaned segments count is 3 (e.g. VK.Blocks.Core), it is the block itself.
            var blockSegmentCount = segmentCount > 3 ? segmentCount - 1 : segmentCount;
            var blockNamespace = string.Join(".", segments.Take(blockSegmentCount));
            var blockName = string.Join("", segments.Skip(2).Take(blockSegmentCount - 2));
            return $"global::{blockNamespace}.IVK{blockName}Builder";
        }

        // 5. Absolute Fallback
        var fallbackTypeName = parentTypeSymbol.Name;
        var fallbackBlockName = fallbackTypeName;
        if (fallbackBlockName.StartsWith("VK"))
            fallbackBlockName = fallbackBlockName.Substring(2);
        if (fallbackBlockName.EndsWith("Block"))
            fallbackBlockName = fallbackBlockName.Substring(0, fallbackBlockName.Length - 5);
        if (fallbackBlockName.EndsWith("Feature"))
            fallbackBlockName = fallbackBlockName.Substring(0, fallbackBlockName.Length - 7);

        var fallbackNs = parentTypeSymbol.ContainingNamespace.ToDisplayString();
        if (fallbackNs.EndsWith(".Internal"))
            fallbackNs = fallbackNs.Substring(0, fallbackNs.Length - 9);

        return $"global::{fallbackNs}.IVK{fallbackBlockName}Builder";
    }

    private static string InferName(string className)
    {
        var name = className;
        if (name.StartsWith("VK"))
            name = name.Substring(2);
        if (name.EndsWith("Options"))
            name = name.Substring(0, name.Length - 7);
        return name;
    }

    private static string ResolveSegment(INamedTypeSymbol symbol)
    {
        // 1. Get the cleaned namespace of the symbol
        var fullNs = symbol.ContainingNamespace.ToDisplayString();
        var segments = fullNs.Split('.');
        var segmentCount = segments.Length;

        // Trim common trailing technical namespaces
        while (segmentCount > 0 && (
            segments[segmentCount - 1] == "Internal" ||
            segments[segmentCount - 1] == "DependencyInjection" ||
            segments[segmentCount - 1] == "Common" ||
            segments[segmentCount - 1] == "Shared" ||
            segments[segmentCount - 1] == "Contracts" ||
            segments[segmentCount - 1] == "Protocols"))
        {
            segmentCount--;
        }

        var cleanedSegments = segments.Take(segmentCount).ToList();
        if (cleanedSegments.Count > 0 && cleanedSegments[0] == "VK")
            cleanedSegments.RemoveAt(0);
        if (cleanedSegments.Count > 0 && cleanedSegments[0] == "Blocks")
            cleanedSegments.RemoveAt(0);

        // 2. Scan the containing assembly for a custom VKBlockMarkerAttribute override!
        var assembly = symbol.ContainingAssembly;
        string? customBlockId = null;
        if (assembly is not null)
        {
            var queue = new Queue<INamespaceSymbol>();
            queue.Enqueue(assembly.GlobalNamespace);
            while (queue.Count > 0)
            {
                var ns = queue.Dequeue();
                foreach (var type in ns.GetTypeMembers())
                {
                    var blockAttr = type.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKBlockMarkerAttribute");
                    if (blockAttr is not null)
                    {
                        var explicitId = blockAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
                        if (!string.IsNullOrWhiteSpace(explicitId) && explicitId != "Unknown")
                        {
                            customBlockId = explicitId!.Replace("VK.Blocks.", "").Replace(".", "");
                            break;
                        }
                    }
                }
                if (customBlockId is not null) break;
                foreach (var subNs in ns.GetNamespaceMembers())
                {
                    queue.Enqueue(subNs);
                }
            }
        }

        // 3. Determine the Block segment vs Feature segments based on assembly name overlap
        var assemblyName = symbol.ContainingAssembly?.Name ?? "";
        var assemblySegments = assemblyName.Split('.').ToList();
        if (assemblySegments.Count > 0 && assemblySegments[0] == "VK")
            assemblySegments.RemoveAt(0);
        if (assemblySegments.Count > 0 && assemblySegments[0] == "Blocks")
            assemblySegments.RemoveAt(0);

        var blockSegmentsCount = Math.Min(cleanedSegments.Count, assemblySegments.Count);
        
        // Use customBlockId if found, otherwise use the overlap of assembly segments
        var blockPart = customBlockId ?? string.Join("", cleanedSegments.Take(blockSegmentsCount));
        
        var featureSegments = cleanedSegments.Skip(blockSegmentsCount).ToList();
        
        // Also if the options class name itself contains additional nesting beyond namespace features
        var optionsFeatureName = InferName(symbol.Name);
        if (featureSegments.Count == 0 || featureSegments[featureSegments.Count - 1] != optionsFeatureName)
        {
            // If the last segment is not the feature itself (like budgeting options in tokenics namespace)
            // But wait, in VKBudgetingOptions: namespace is Tokenics.Budgeting, so featureSegments contains "Tokenics" and "Budgeting".
            // So "Budgeting" is already there. No need to add.
        }

        if (featureSegments.Count > 0)
        {
            return blockPart + ":" + string.Join(":", featureSegments);
        }
        
        return blockPart;
    }

    private static void EmitSource(SourceProductionContext ctx, FeatureTarget target, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
            return;

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

    private static void EmitProvider(SourceProductionContext ctx, FeatureTarget target)
    {
        var optionsClassName = target.OptionsClassName;
        var baseClassName = optionsClassName.EndsWith("Options") ? optionsClassName.Substring(0, optionsClassName.Length - 7) : optionsClassName;

        var interfaceName = $"I{baseClassName}OptionsProvider";
        var implementationName = $"{baseClassName}OptionsDefaultProvider";
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
        if (target.GenerateValidator)
            interfaceList.Add($"IValidateOptions<{target.OptionsClassName}>");

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
        sb.AppendLine("            if (transform != null)");
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

        if (target.GenerateValidator)
        {
            sb.AppendLine($"        services.TryAddEnumerableSingleton<IValidateOptions<{target.OptionsClassName}>, {target.FeatureName}Feature>();");
        }

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
        sb.AppendLine($"    public static string SectionName => \"{target.ComputedSectionName}\";");
        sb.AppendLine("}");

        ctx.AddSource($"{target.OptionsClassName}.Feature.g.cs", sb.ToString());
    }

    private sealed record FeatureTarget(
        string Namespace,
        string OptionsClassName,
        string OptionsFullNamespace,
        string FeatureName,
        string ParentBlockTypeFullName,
        string BuilderTypeFullName,
        bool GenerateArgs,
        bool GenerateValidator,
        string? SectionNameOverride,
        bool IsToggleable,
        bool IsPartial,
        bool IsAISettings,
        bool IsGovernanceSettings,
        bool IsTimeoutPresent,
        ImmutableArray<string> ImplementedOverrides,
        ImmutableArray<PropertyTarget> Properties,
        string ComputedSectionName);

    private sealed record PropertyTarget(string Name, string Type, bool IsAlreadyNullable, bool ExistsInOptions);
}
