using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
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

        // Verify if VK{BlockName}Options exists in the compilation
        var optionsTypeName = $"{symbol.ContainingNamespace.ToDisplayString()}.VK{blockName}Options";
        var optionsTypeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(optionsTypeName);
        var generateOptions = optionsTypeSymbol is null;

        return new BlockTargetInfo(
            Namespace: symbol.ContainingNamespace.ToDisplayString(),
            ClassName: symbol.Name,
            BlockName: blockName,
            GenerateOptions: generateOptions
        );
    }

    private static void EmitSource(SourceProductionContext ctx, BlockTargetInfo target, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
            return;

        // 0. Generate VK{BlockName}Options record if not present
        if (target.GenerateOptions)
        {
            EmitOptionsClass(ctx, target);
        }

        // 1. Generate IVK{BlockName}Builder interface
        EmitBuilderInterface(ctx, target);

        // 2. Generate {BlockName}BlockBuilder class
        EmitBuilderClass(ctx, target);

        // 3. Generate VK{BlockName}BlockExtensions
        EmitExtensionsClass(ctx, target);

        // 4. Generate {BlockName}BlockRegistration
        EmitRegistrationClass(ctx, target);
    }



    private static void EmitOptionsClass(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Configuration options for the {target.BlockName} building block.");
        sb.AppendLine("/// Automatically generated via VKBlockMarker.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed partial record VK{target.BlockName}Options : IVKToggleableBlockOptions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// The configuration section name.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static string SectionName => $\"{{VKBlocksConstants.VKBlocksConfigPrefix}}:{target.BlockName}\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets or sets a value indicating whether the block is enabled.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public bool Enabled { get; init; } = true;");
        sb.AppendLine("}");

        ctx.AddSource($"VK{target.BlockName}Options.g.cs", sb.ToString());
    }

    private static void EmitBuilderInterface(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Builder contract for the {target.BlockName} building block.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public partial interface IVK{target.BlockName}Builder : IVKBlockBuilder<{target.ClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("}");

        ctx.AddSource($"IVK{target.BlockName}Builder.g.cs", sb.ToString());
    }

    private static void EmitBuilderClass(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace}.Common.DependencyInjection.Internal;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Default implementation of the {target.BlockName} builder.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed partial class {target.BlockName}BlockBuilder(");
        sb.AppendLine("    IServiceCollection services,");
        sb.AppendLine("    IConfiguration configuration)");
        sb.AppendLine($"    : VKBlockBuilder<{target.ClassName}>(services, configuration), IVK{target.BlockName}Builder");
        sb.AppendLine("{");
        sb.AppendLine("}");

        ctx.AddSource($"{target.BlockName}BlockBuilder.g.cs", sb.ToString());
    }

    private static void EmitExtensionsClass(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine($"using {target.Namespace}.Common.DependencyInjection.Internal;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Service collection extensions for the {target.BlockName} building block.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static partial class VK{target.BlockName}BlockExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Adds the {target.BlockName} building block services using configuration.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static IVK{target.BlockName}Builder AddVK{target.BlockName}Block(");
        sb.AppendLine("        this IServiceCollection services,");
        sb.AppendLine("        IConfiguration configuration,");
        sb.AppendLine($"        Func<VK{target.BlockName}Options, VK{target.BlockName}Options>? transform = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        VKGuard.NotNull(services);");
        sb.AppendLine("        VKGuard.NotNull(configuration);");
        sb.AppendLine($"        return {target.BlockName}BlockRegistration.Register(services, configuration, transform);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"VK{target.BlockName}BlockExtensions.g.cs", sb.ToString());
    }

    private static void EmitRegistrationClass(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace}.Common.DependencyInjection.Internal;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Handles the core DI registration for the {target.BlockName} block.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal static partial class {target.BlockName}BlockRegistration");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Custom registration hook for block-specific components.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    static partial void RegisterBlockCustom(IVK{target.BlockName}Builder builder);");
        sb.AppendLine();
        sb.AppendLine($"    internal static IVK{target.BlockName}Builder Register(");
        sb.AppendLine("        IServiceCollection services,");
        sb.AppendLine("        IConfiguration configuration,");
        sb.AppendLine($"        Func<VK{target.BlockName}Options, VK{target.BlockName}Options>? transform = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        // 1. Check-Self");
        sb.AppendLine($"        if (services.IsVKBlockRegistered<{target.ClassName}>())");
        sb.AppendLine("        {");
        sb.AppendLine($"            return new {target.BlockName}BlockBuilder(services, configuration);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 2. Options Registration");
        sb.AppendLine($"        VK{target.BlockName}Options options = services.AddVKBlockOptions<VK{target.BlockName}Options>(configuration, transform);");
        sb.AppendLine();
        sb.AppendLine("        // 3. Mark-Self");
        sb.AppendLine($"        services.AddVKBlockMarker<{target.ClassName}>();");
        sb.AppendLine();
        sb.AppendLine("        // 4. Validate Options");
        if (!target.GenerateOptions)
        {
            sb.AppendLine($"        services.TryAddEnumerableSingleton<IValidateOptions<VK{target.BlockName}Options>, {target.BlockName}OptionsValidator>();");
        }
        sb.AppendLine();
        sb.AppendLine($"        var builder = new {target.BlockName}BlockBuilder(services, configuration);");
        sb.AppendLine();
        sb.AppendLine("        // 5. Feature Toggle");
        sb.AppendLine("        if (!options.Enabled)");
        sb.AppendLine("        {");
        sb.AppendLine("            return builder;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 6. Custom Hook");
        sb.AppendLine($"        RegisterBlockCustom(builder);");
        sb.AppendLine();
        sb.AppendLine("        return builder;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{target.BlockName}BlockRegistration.g.cs", sb.ToString());
    }

    private sealed record BlockTargetInfo(string Namespace, string ClassName, string BlockName, bool GenerateOptions);
}
