using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.DependencyInjection.Emitters.Feature;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.DependencyInjection;

/// <summary>
/// Source generator that automates feature boilerplate (Constants, Marker, Registration).
/// Triggered by [VKFeature] attribute on Feature classes (or backward-compatible Options records).
/// </summary>
[Generator]
public sealed class VKFeatureGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "VK.Blocks.Core.VKFeatureAttribute";

    private static class ArgsMode
    {
        public const int None = 0;
        public const int Explicit = 1;
        public const int Implicit = 2;
    }

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
            (ctx, pair) => FeatureExtensionsEmitter.Emit(ctx, pair.Left, pair.Right, this.GetType()));
    }

    private static FeatureTarget? GetTarget(GeneratorSyntaxContext context, CancellationToken ct)
    {
        try
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, ct) as INamedTypeSymbol;

            if (symbol is null || symbol.IsAbstract)
                return null;

            var attribute = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeFullName);

            if (attribute is null)
                return null;

            INamedTypeSymbol? parentTypeSymbol = null;
            string? explicitName = null;
            string? sectionNameOverride = null;
            string? namespaceOverride = null;
            INamedTypeSymbol? optionsTypeSymbol = null;

            var args = attribute.ConstructorArguments;
            if (args.Length >= 1)
            {
                parentTypeSymbol = args[0].Value as INamedTypeSymbol;
            }
            explicitName = args.Length > 1 ? args[1].Value?.ToString() : null;

            var registerByDefault = true;
            var registerByDefaultArg = attribute.NamedArguments.FirstOrDefault(n => n.Key == "RegisterByDefault");
            if (registerByDefaultArg.Value.Value is bool regVal)
            {
                registerByDefault = regVal;
            }
            var argsGenerationMode = ArgsMode.None;
            INamedTypeSymbol? argsBaseTypeSymbol = attribute.NamedArguments.FirstOrDefault(n => n.Key == "ArgsBaseType").Value.Value as INamedTypeSymbol;

            var argsModeArg = attribute.NamedArguments.FirstOrDefault(n => n.Key == "ArgsGenerationMode");
            if (argsModeArg.Value.Value is int modeVal)
            {
                argsGenerationMode = modeVal;
            }
            else if (argsModeArg.Value.Value is byte byteModeVal)
            {
                argsGenerationMode = byteModeVal;
            }

            // Check if optionsTypeSymbol has [VKOptions] attribute overriding settings
            if (optionsTypeSymbol is not null)
            {
                var vkOptionsAttr = optionsTypeSymbol.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKOptionsAttribute" ||
                    a.AttributeClass?.Name == "VKOptionsAttribute" ||
                    a.AttributeClass?.Name == "VKOptions");

                if (vkOptionsAttr is not null)
                {
                    var optArgsMode = vkOptionsAttr.NamedArguments.FirstOrDefault(n => n.Key == "ArgsMode");
                    if (optArgsMode.Value.Value is int optModeVal)
                    {
                        argsGenerationMode = optModeVal;
                    }
                    else if (optArgsMode.Value.Value is byte optByteModeVal)
                    {
                        argsGenerationMode = optByteModeVal;
                    }
                    var optArgsBase = vkOptionsAttr.NamedArguments.FirstOrDefault(n => n.Key == "ArgsBaseType").Value.Value as INamedTypeSymbol;
                    if (optArgsBase is not null)
                    {
                        argsBaseTypeSymbol = optArgsBase;
                    }
                }
            }

            sectionNameOverride = attribute.NamedArguments.FirstOrDefault(n => n.Key == "SectionName").Value.Value?.ToString();
            namespaceOverride = attribute.NamedArguments.FirstOrDefault(n => n.Key == "Namespace").Value.Value?.ToString();
            optionsTypeSymbol = attribute.NamedArguments.FirstOrDefault(n => n.Key == "OptionsType").Value.Value as INamedTypeSymbol;

            if (parentTypeSymbol is null)
            {
                return null;
            }

            var featureName = explicitName ?? InferName(symbol.Name);

            // Options name & namespace mapping (resolving from OptionsType symbol or naming fallback)
            var optionsClassName = optionsTypeSymbol?.Name ?? $"VK{featureName}Options";
            var optionsFullNamespace = optionsTypeSymbol?.ContainingNamespace.ToDisplayString() ?? symbol.ContainingNamespace.ToDisplayString().Replace(".Internal", "");

            var targetNamespace = namespaceOverride ?? symbol.ContainingNamespace.ToDisplayString();
            if (targetNamespace.EndsWith(".Internal"))
            {
                targetNamespace = targetNamespace.Substring(0, targetNamespace.Length - 9);
            }

            // Structural Info
            var isToggleable = optionsTypeSymbol?.AllInterfaces.Any(i => i.Name == "IVKToggleableBlockOptions") ?? false;
            var isOptionsPartial = optionsTypeSymbol?.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .Any(t => t.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))) ?? false;

            // Extract public properties based on ArgsGenerationMode
            var propertiesList = new List<PropertyTarget>();
            if (optionsTypeSymbol is not null && argsGenerationMode != ArgsMode.None)
            {

                foreach (var member in optionsTypeSymbol.GetMembers().OfType<IPropertySymbol>())
                {
                    if (member.DeclaredAccessibility == Accessibility.Public && !member.IsStatic && !member.IsReadOnly)
                    {
                        var hasOverrideAttr = member.GetAttributes().Any(a =>
                            a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKRequestOverrideAttribute" ||
                            a.AttributeClass?.Name == "VKRequestOverrideAttribute" ||
                            a.AttributeClass?.Name == "VKRequestOverride");

                        var hasNoOverrideAttr = member.GetAttributes().Any(a =>
                            a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKNoRequestOverrideAttribute" ||
                            a.AttributeClass?.Name == "VKNoRequestOverrideAttribute" ||
                            a.AttributeClass?.Name == "VKNoRequestOverride");

                        bool shouldInclude = false;
                        if (argsGenerationMode == ArgsMode.Explicit)
                        {
                            shouldInclude = hasOverrideAttr;
                        }
                        else if (argsGenerationMode == ArgsMode.Implicit)
                        {
                            shouldInclude = !hasNoOverrideAttr;
                        }

                        if (shouldInclude)
                        {
                            propertiesList.Add(new PropertyTarget(
                                Name: member.Name,
                                Type: member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier)),
                                IsAlreadyNullable: member.Type.NullableAnnotation is NullableAnnotation.Annotated || member.Type.ToDisplayString().EndsWith("?"),
                                ExistsInOptions: true
                            ));
                        }
                    }
                }
            }

            var builderTypeFullName = GetBuilderTypeFullName(parentTypeSymbol);
            var parentBlockName = parentTypeSymbol.Name;
            if (parentBlockName.EndsWith("Block"))
                parentBlockName = parentBlockName.Substring(0, parentBlockName.Length - 5);
            if (parentBlockName.StartsWith("VK"))
                parentBlockName = parentBlockName.Substring(2);

            // Walk up the parent hierarchy to construct the full configuration path (e.g. VKBlocks:AI:Tokenics:Counting)
            var pathParts = new System.Collections.Generic.List<string> { featureName };
            var curr = parentTypeSymbol;
            while (curr is not null)
            {
                var name = curr.Name;
                if (name.EndsWith("Block"))
                {
                    name = name.Substring(0, name.Length - 5);
                    if (name.StartsWith("VK")) name = name.Substring(2);
                    pathParts.Insert(0, name);
                    break;
                }
                if (name.EndsWith("Feature"))
                {
                    name = name.Substring(0, name.Length - 7);
                    if (name.StartsWith("VK")) name = name.Substring(2);
                    pathParts.Insert(0, name);
                }

                var featureAttr = curr.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKFeatureAttribute" || a.AttributeClass?.Name == "VKFeatureAttribute");
                if (featureAttr is not null && featureAttr.ConstructorArguments.Length > 0)
                {
                    var parentType = featureAttr.ConstructorArguments[0].Value as INamedTypeSymbol;
                    if (parentType is not null)
                    {
                        curr = parentType;
                        continue;
                    }
                }
                break;
            }

            var path = string.Join(":", pathParts);
            var computedSectionName = sectionNameOverride ?? $"{VKBlocksConstants.VKBlocksConfigPrefix}:{path}";

            var rootBlock = FindRootBlockSymbol(parentTypeSymbol);
            var rootNamespace = rootBlock.ContainingNamespace.ToDisplayString();
            var internalIndex = rootNamespace.IndexOf(".Common.DependencyInjection");
            if (internalIndex > 0)
            {
                rootNamespace = rootNamespace.Substring(0, internalIndex);
            }

            // Parent block options type name & toggleable
            string? parentOptionsTypeFullName = null;
            var parentToggleable = true;
            
            if (parentTypeSymbol.Name.EndsWith("Block"))
            {
                parentOptionsTypeFullName = $"global::{rootNamespace}.VK{parentBlockName}Options";
            }
            else
            {
                var parentCleanName = parentTypeSymbol.Name;
                if (parentCleanName.EndsWith("Feature"))
                    parentCleanName = parentCleanName.Substring(0, parentCleanName.Length - 7);
                if (parentCleanName.StartsWith("VK"))
                    parentCleanName = parentCleanName.Substring(2);
                parentOptionsTypeFullName = $"global::{rootNamespace}.VK{parentCleanName}Options";
            }

            var blockMarkerAttr = parentTypeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKBlockMarkerAttribute");
            if (blockMarkerAttr is not null)
            {
                var toggleableArg = blockMarkerAttr.NamedArguments.FirstOrDefault(n => n.Key == "Toggleable");
                if (toggleableArg.Value.Value is bool b)
                {
                    parentToggleable = b;
                }
            }

            ArgsBaseInfo? argsBaseInfo = null;

            if (argsBaseTypeSymbol is not null)
            {
                var baseProps = new List<PropertyTarget>();
                foreach (var member in argsBaseTypeSymbol.GetMembers().OfType<IPropertySymbol>())
                {
                    if (member.DeclaredAccessibility == Accessibility.Public && !member.IsStatic && !member.IsReadOnly)
                    {
                        var propType = member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
                        var existsInOptions = optionsTypeSymbol?.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == member.Name) ?? false;
                        
                        baseProps.Add(new PropertyTarget(
                            Name: member.Name,
                            Type: propType,
                            IsAlreadyNullable: member.Type.NullableAnnotation is NullableAnnotation.Annotated || member.Type.ToDisplayString().EndsWith("?"),
                            ExistsInOptions: existsInOptions
                        ));
                    }
                }
                
                argsBaseInfo = new ArgsBaseInfo(
                    TypeName: argsBaseTypeSymbol.Name,
                    FullNamespace: argsBaseTypeSymbol.ContainingNamespace.ToDisplayString(),
                    Properties: baseProps.ToImmutableArray()
                );
            }

            var isTimeoutPresent = optionsTypeSymbol?.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == "Timeout") ?? false;

            return new FeatureTarget(
                Identity: new FeatureIdentity(
                    Namespace: targetNamespace,
                    FeatureName: featureName,
                    BuilderTypeFullName: builderTypeFullName
                ),
                Options: new FeatureOptionsInfo(
                    ClassName: optionsClassName,
                    FullNamespace: optionsFullNamespace,
                    ComputedSectionName: computedSectionName,
                    IsToggleable: isToggleable,
                    IsPartial: isOptionsPartial,
                    IsTimeoutPresent: isTimeoutPresent
                ),
                Parent: new FeatureParentInfo(
                    BlockTypeFullName: parentTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    OptionsTypeFullName: parentOptionsTypeFullName,
                    Toggleable: parentToggleable
                ),
                ArgsGenerationMode: argsGenerationMode,
                RegisterByDefault: registerByDefault,
                Properties: propertiesList.ToImmutableArray(),
                ArgsBase: argsBaseInfo
            );
        }
        catch
        {
            return null;
        }
    }

    private static string GetBuilderTypeFullName(INamedTypeSymbol parentTypeSymbol)
    {
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

        var assemblyName = parentTypeSymbol.ContainingAssembly?.Name;
        if (assemblyName is not null && assemblyName.StartsWith("VK.Blocks"))
        {
            var moduleName = assemblyName.Substring(9).TrimStart('.');
            var blockName = moduleName.Replace(".", "");
            return $"global::{assemblyName}.IVK{blockName}Builder";
        }

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
        if (name.EndsWith("Feature"))
            name = name.Substring(0, name.Length - 7);
        if (name.EndsWith("Options"))
            name = name.Substring(0, name.Length - 7);
        return name;
    }

    private static void EmitSource(SourceProductionContext ctx, FeatureTarget target, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
            return;

        FeatureAnchorEmitter.Emit(ctx, target);

        if (target.ArgsGenerationMode != ArgsMode.None)
        {
            FeatureArgsEmitter.Emit(ctx, target, assemblyName);
            FeatureOptionsProviderEmitter.Emit(ctx, target, assemblyName);
        }

        var parentBlockName = target.Parent.BlockTypeFullName.Split('.').Last();
        if (parentBlockName.EndsWith("Block"))
            parentBlockName = parentBlockName.Substring(0, parentBlockName.Length - 5);
        if (parentBlockName.StartsWith("VK"))
            parentBlockName = parentBlockName.Substring(2);

        var blockOptionsName = $"VK{parentBlockName}Options";
        var isBlockOptions = target.Options.ClassName == blockOptionsName;

        if (target.Options.IsPartial && !isBlockOptions)
        {
            FeatureOptionsEmitter.Emit(ctx, target);
        }
        else if (!target.Options.IsPartial && !isBlockOptions)
        {
            var diagnostic = VKDiagnostics.CreateTypeMustBePartial(
                "options class",
                target.Options.ClassName);
            ctx.ReportDiagnostic(diagnostic);
        }
    }

    private static INamedTypeSymbol FindRootBlockSymbol(INamedTypeSymbol symbol)
    {
        var current = symbol;
        while (current is not null)
        {
            if (current.Name.EndsWith("Block"))
            {
                return current;
            }
            var featureAttr = current.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "VK.Blocks.Core.VKFeatureAttribute" || a.AttributeClass?.Name == "VKFeatureAttribute");
            if (featureAttr is not null && featureAttr.ConstructorArguments.Length > 0)
            {
                var parentType = featureAttr.ConstructorArguments[0].Value as INamedTypeSymbol;
                if (parentType is not null)
                {
                    current = parentType;
                    continue;
                }
            }
            break;
        }
        return symbol;
    }
}
