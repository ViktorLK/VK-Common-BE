using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.DependencyInjection.Emitters.Block;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.DependencyInjection;

/// <summary>
/// Source generator that automatically generates:
/// 1. IVK{BlockName}Builder interface
/// 2. {BlockName}BlockBuilder implementation
/// 3. VK{BlockName}BlockExtensions public entry point
/// 4. {BlockName}BlockRegistration core DI boilerplate
/// for any class decorated with [VKBlockMarker].
/// </summary>
[Generator]
public sealed class VKBlockGenerator : IIncrementalGenerator
{
    private const string MarkerAttributeFullName = "VK.Blocks.Core.VKBlockMarkerAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var blockTargets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MarkerAttributeFullName,
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: GetTargetBlock)
            .WhereNotNull();

        context.RegisterSourceOutput(
            blockTargets.Combine(assemblyName),
            (ctx, pair) => EmitSource(ctx, pair.Left, pair.Right, this.GetType()));
    }

    private static BlockTargetInfo? GetTargetBlock(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        var symbol = context.TargetSymbol as INamedTypeSymbol;
        if (symbol is null || symbol.IsAbstract)
            return null;

        var blockName = symbol.Name;
        if (blockName.StartsWith("VK"))
            blockName = blockName.Substring(2);
        if (blockName.EndsWith("Block"))
            blockName = blockName.Substring(0, blockName.Length - 5);

        if (blockName == "Core")
            return null;

        // Extract Toggleable property from [VKBlockMarker] attribute
        var attribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == MarkerAttributeFullName);
        var toggleable = true;
        if (attribute is not null)
        {
            var toggleArg = attribute.NamedArguments.FirstOrDefault(x => x.Key == "Toggleable");
            if (toggleArg.Value.Value is bool b)
            {
                toggleable = b;
            }
        }

        // Verify if VK{BlockName}Options exists in the compilation
        var optionsTypeName = $"{symbol.ContainingNamespace.ToDisplayString()}.VK{blockName}Options";
        var optionsTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(optionsTypeName);
        var hasEnabled = optionsTypeSymbol?.GetMembers("Enabled").Any() ?? false;
        var generateToggleableMembers = toggleable && !hasEnabled;

        // Detect if a generated feature class for this block exists in the expected namespace.
        var featureTypeFqn = $"{symbol.ContainingNamespace.ToDisplayString()}.Common.DependencyInjection.Internal.{blockName}Block";
        var featureTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(featureTypeFqn);
        var hasGeneratedFeature = featureTypeSymbol is not null;

        return new BlockTargetInfo(
            Namespace: symbol.ContainingNamespace.ToDisplayString(),
            ClassName: symbol.Name,
            BlockName: blockName,
            GenerateToggleableMembers: generateToggleableMembers,
            Toggleable: toggleable,
            HasGeneratedFeature: hasGeneratedFeature
        );
    }

    private static void EmitSource(SourceProductionContext ctx, BlockTargetInfo target, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
            return;

        // 1. Generate VK{BlockName}Options record or its SectionName partial
        BlockOptionsEmitter.Emit(ctx, target);

        // 2. Generate {BlockName}Block registration and validation hub class
        BlockAnchorEmitter.Emit(ctx, target);

        // 3. Generate IVK{BlockName}OptionsProvider and VK{BlockName}OptionsDefaultProvider
        BlockOptionsProviderEmitter.Emit(ctx, target);

        // 4. Generate IVK{BlockName}Builder interface and implementations
        BlockBuilderEmitter.Emit(ctx, target);

        // 5. Generate VK{BlockName}BlockExtensions
        BlockExtensionsEmitter.Emit(ctx, target);

        // 6. Generate {BlockName}BlockRegistration
        BlockRegistrationEmitter.Emit(ctx, target);
    }
}
