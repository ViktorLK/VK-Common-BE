using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Feature.Models;
using VK.Tools.SourceGenerators.Feature.Internal;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.Feature;

/// <summary>
/// Source generator that automates feature boilerplate (Constants, Marker, Registration).
/// Triggered by [VKFeature] attribute on Options records/classes.
/// </summary>
[Generator]
public sealed class VKFeatureGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKFeatureAttribute";
    private const string DefaultsAttributeFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKDefaultsAttribute";

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

        var collected = targets.Collect();
        context.RegisterSourceOutput(
            collected.Combine(context.CompilationProvider),
            (ctx, pair) => BuilderExtensionsEmitter.Emit(ctx, pair.Left, pair.Right, this.GetType()));
    }

    private static FeatureTarget? GetTarget(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, ct) as INamedTypeSymbol;

        if (symbol is null)
            return null;

        var attribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeFullName);

        var isDefaultsAttribute = false;
        if (attribute is null)
        {
            attribute = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == DefaultsAttributeFullName);
            if (attribute is null)
                return null;
            isDefaultsAttribute = true;
        }

        // Extract attribute data
        var args = attribute.ConstructorArguments;
        if (args.Length < 1)
            return null;

        var parentTypeSymbol = args[0].Value as INamedTypeSymbol;
        if (parentTypeSymbol is null)
            return null;

        var parentSegment = ResolveSegment(parentTypeSymbol);

        // 1. Name Inference
        string featureName;
        if (isDefaultsAttribute)
        {
            featureName = $"{parentSegment}Defaults";
        }
        else
        {
            var explicitName = args.Length > 1 ? args[1].Value?.ToString() : null;
            featureName = explicitName ?? InferName(symbol.Name);
        }

        // 2. Namespace Inheritance
        var namespaceOverride = attribute.NamedArguments.FirstOrDefault(n => n.Key == "Namespace").Value.Value?.ToString();
        var parentNs = parentTypeSymbol.ContainingNamespace.ToDisplayString();
        if (parentNs.EndsWith(".Internal"))
            parentNs = parentNs.Substring(0, parentNs.Length - 9);

        string targetNamespace;
        if (isDefaultsAttribute)
        {
            targetNamespace = namespaceOverride ?? $"{parentNs}.Common.DependencyInjection";
        }
        else
        {
            targetNamespace = namespaceOverride ?? $"{parentNs}.{featureName}";
        }

        // 3. Flags
        var generateArgs = !isDefaultsAttribute && (attribute.NamedArguments.FirstOrDefault(n => n.Key == "GenerateArgs").Value.Value as bool? ?? false);
        var isDefault = isDefaultsAttribute || (attribute.NamedArguments.FirstOrDefault(n => n.Key == "IsDefault").Value.Value as bool? ?? true);
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

            if (overridesName is not null)
            {
                // Try to find the interface in the compilation
                var overridesSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName($"{@interface.ContainingNamespace.ToDisplayString()}.{overridesName}")
                                      ?? @interface.ContainingNamespace.GetTypeMembers(overridesName).FirstOrDefault();

                if (overridesSymbol is not null)
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
                IsAlreadyNullable: p.Type.NullableAnnotation is NullableAnnotation.Annotated || p.Type.ToDisplayString().EndsWith("?"),
                ExistsInOptions: optionsProperties.Contains(p.Name)
            ))
            .ToImmutableArray();

        var isTimeoutPresent = symbol.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == "Timeout");

        var builderTypeFullName = GetBuilderTypeFullName(parentTypeSymbol);

        var configFeatureName = featureName;
        if (configFeatureName.EndsWith("Defaults") && configFeatureName.StartsWith(parentSegment))
        {
            configFeatureName = "Defaults";
        }
        var computedSectionName = sectionNameOverride ?? $"{parentSegment}:{configFeatureName}";

        return new FeatureTarget(
            Namespace: targetNamespace,
            OptionsClassName: symbol.Name,
            OptionsFullNamespace: symbol.ContainingNamespace.ToDisplayString(),
            FeatureName: featureName,
            ParentBlockTypeFullName: parentTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            BuilderTypeFullName: builderTypeFullName,
            GenerateArgs: generateArgs,
            IsDefault: isDefault,
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

        if (featureMarkerAttr is not null && featureMarkerAttr.ConstructorArguments.Length > 1)
        {
            var grandParentType = featureMarkerAttr.ConstructorArguments[1].Value as INamedTypeSymbol;
            if (grandParentType is not null)
            {
                return GetBuilderTypeFullName(grandParentType);
            }
        }

        // 3. Generic Assembly-Based Block Resolution
        var assemblyName = parentTypeSymbol.ContainingAssembly?.Name;
        if (assemblyName is not null && assemblyName.StartsWith("VK.Blocks"))
        {
            var moduleName = assemblyName.Substring(9).TrimStart('.');
            var blockName = moduleName.Replace(".", "");
            return $"global::{assemblyName}.IVK{blockName}Builder";
        }

        // 4. Generic Namespace Segment Fallback
        var fullNs = parentTypeSymbol.ContainingNamespace.ToDisplayString();
        var segments = fullNs.Split('.');
        var segmentCount = segments.Length;

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
        var fullNs = symbol.ContainingNamespace.ToDisplayString();
        var segments = fullNs.Split('.');
        var segmentCount = segments.Length;

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
                if (customBlockId is not null)
                    break;
                foreach (var subNs in ns.GetNamespaceMembers())
                {
                    queue.Enqueue(subNs);
                }
            }
        }

        var assemblyName = symbol.ContainingAssembly?.Name ?? "";
        var assemblySegments = assemblyName.Split('.').ToList();
        if (assemblySegments.Count > 0 && assemblySegments[0] == "VK")
            assemblySegments.RemoveAt(0);
        if (assemblySegments.Count > 0 && assemblySegments[0] == "Blocks")
            assemblySegments.RemoveAt(0);

        var blockSegmentsCount = Math.Min(cleanedSegments.Count, assemblySegments.Count);
        var blockPart = customBlockId ?? string.Join("", cleanedSegments.Take(blockSegmentsCount));
        var featureSegments = cleanedSegments.Skip(blockSegmentsCount).ToList();

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

        FeatureAnchorEmitter.Emit(ctx, target);

        if (target.GenerateArgs)
        {
            ArgsEmitter.Emit(ctx, target, assemblyName);
            OptionsProviderEmitter.Emit(ctx, target, assemblyName);
        }

        if (target.IsPartial)
        {
            OptionsPartialEmitter.Emit(ctx, target);
        }
    }
}
