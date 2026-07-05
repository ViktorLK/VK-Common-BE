using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Feature.Models;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.Feature.Internal;

internal static class BuilderExtensionsEmitter
{
    public static void Emit(
        SourceProductionContext ctx,
        ImmutableArray<FeatureTarget> targets,
        Compilation compilation,
        Type generatorType)
    {
        var assemblyName = compilation.AssemblyName;
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
            return;

        if (targets.IsDefaultOrEmpty)
            return;

        var targetInfos = targets.Select(t =>
        {
            var ns = t.BuilderTypeFullName.Replace("global::", "");
            var lastDot = ns.LastIndexOf('.');
            var blockNamespace = lastDot > -1 ? ns.Substring(0, lastDot) : ns;

            var interfaceName = ns.Split('.').Last();
            var baseBuilderName = interfaceName.StartsWith("IVK") ? interfaceName.Substring(3) : (interfaceName.StartsWith("I") ? interfaceName.Substring(1) : interfaceName);
            if (baseBuilderName.EndsWith("Builder"))
            {
                baseBuilderName = baseBuilderName.Substring(0, baseBuilderName.Length - 7);
            }
            var extensionsClassName = $"VK{baseBuilderName}BuilderExtensions";

            return new { Target = t, BlockNamespace = blockNamespace, ExtensionsClassName = extensionsClassName, BaseBuilderName = baseBuilderName };
        });

        var groups = targetInfos.GroupBy(x => x.ExtensionsClassName);

        foreach (var group in groups)
        {
            var extensionsClassName = group.Key;
            var first = group.First();
            var blockNamespace = first.BlockNamespace;
            var baseBuilderName = first.BaseBuilderName;
            var builderTypeFullName = first.Target.BuilderTypeFullName;

            // Check if the extension class already exists in the compilation and is NOT partial
            var typeSymbol = compilation.GetTypeByMetadataName($"{blockNamespace}.{extensionsClassName}");
            if (typeSymbol is not null)
            {
                var isPartial = typeSymbol.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<TypeDeclarationSyntax>()
                    .Any(t => t.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

                if (!isPartial)
                {
                    continue; // Skip generation to avoid compilation errors on non-partial manual classes
                }
            }

            var sb = SourceCodeBuilder.CreateWithHeader();
            sb.AppendLine("using System;");
            sb.AppendLine("using VK.Blocks.Core;");
            sb.AppendLine();
            sb.AppendLine($"namespace {blockNamespace};");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// Automatically generated builder extension methods for {baseBuilderName}.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public static partial class {extensionsClassName}");
            sb.AppendLine("{");

            var sortedFeatures = group.Select(x => x.Target).OrderBy(t => t.FeatureName).ToList();

            foreach (var target in sortedFeatures)
            {
                // Check if the specific AddVK{FeatureName} method is already manually defined in the class
                if (typeSymbol is not null)
                {
                    var methodExists = typeSymbol.GetMembers($"AddVK{target.FeatureName}")
                        .OfType<IMethodSymbol>()
                        .Any(m => m.Parameters.Length > 0 && m.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == target.BuilderTypeFullName);

                    if (methodExists)
                    {
                        continue; // Skip generating this specific method
                    }
                }

                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// Adds the {target.FeatureName} feature.");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public static {target.BuilderTypeFullName} AddVK{target.FeatureName}(");
                sb.AppendLine($"        this {target.BuilderTypeFullName} builder,");
                sb.AppendLine($"        Func<{target.OptionsFullNamespace}.{target.OptionsClassName}, {target.OptionsFullNamespace}.{target.OptionsClassName}>? transform = null)");
                sb.AppendLine("    {");
                sb.AppendLine("        VKGuard.NotNull(builder);");
                sb.AppendLine($"        {target.Namespace}.Internal.{target.FeatureName}Feature.Register(builder, transform);");
                sb.AppendLine("        return builder;");
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            // Check if AddVK{BaseBuilderName}DefaultFeatures is already manually defined
            var defaultFeaturesMethodName = $"AddVK{baseBuilderName}DefaultFeatures";
            bool hasManualDefaultFeatures = false;
            if (typeSymbol is not null)
            {
                hasManualDefaultFeatures = typeSymbol.GetMembers(defaultFeaturesMethodName)
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Parameters.Length > 0 && m.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == builderTypeFullName);
            }

            if (!hasManualDefaultFeatures)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// Automatically enables all standard features for {baseBuilderName}.");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public static {builderTypeFullName} {defaultFeaturesMethodName}(this {builderTypeFullName} builder)");
                sb.AppendLine("    {");
                sb.AppendLine("        VKGuard.NotNull(builder);");
                sb.AppendLine("        return builder");

                var defaultFeatures = sortedFeatures.Where(t => t.IsDefault).ToList();
                if (defaultFeatures.Count > 0)
                {
                    for (int i = 0; i < defaultFeatures.Count; i++)
                    {
                        var target = defaultFeatures[i];
                        sb.AppendLine($"            .AddVK{target.FeatureName}()" + (i == defaultFeatures.Count - 1 ? ";" : ""));
                    }
                }
                else
                {
                    sb.AppendLine("            ;");
                }
                sb.AppendLine("    }");
            }

            sb.AppendLine("}");

            ctx.AddSource($"{extensionsClassName}.g.cs", sb.ToString());
        }
    }
}
