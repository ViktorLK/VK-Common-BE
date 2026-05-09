using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Observability.Internal;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.Observability;

/// <summary>
/// Source generator that automatically adds ActivitySource and Meter to classes decorated with [VKBlockDiagnostics].
/// </summary>
[Generator]
public sealed class VKBlockDiagnosticsGenerator : IIncrementalGenerator
{
    private const string GenericAttributeFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKBlockDiagnosticsAttribute`1";
    private const string AppAttributeFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKAppDiagnosticsAttribute";
    private const string MarkerAttributeFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKBlockMarkerAttribute";
    private const string FeatureAttributeFullName = $"{VKBlocksConstants.VKBlocksPrefix}.Core.VKFeatureMarkerAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);
        var options = context.AnalyzerConfigOptionsProvider;
        var assemblyVersion = context.CompilationProvider.Select(static (c, _) => GetAssemblyVersion(c.Assembly));

        var genericClassTargets = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                  GenericAttributeFullName,
                  predicate: IsPartialClass,
                  transform: TransformToTarget)
                .WhereNotNull();

        var appClassTargets = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                  AppAttributeFullName,
                  predicate: IsPartialClass,
                  transform: TransformToTarget)
                .WhereNotNull();

        var markerClassTargets = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                  MarkerAttributeFullName,
                  predicate: IsPartialClass,
                  transform: TransformToTarget)
                .WhereNotNull();

        var featureClassTargets = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                  FeatureAttributeFullName,
                  predicate: IsPartialClass,
                  transform: TransformToTarget)
                .WhereNotNull();

        // Combine all target sources with options, assembly name, and assembly version
        var allWithContext = assemblyName.Combine(options).Combine(assemblyVersion);

        context.RegisterSourceOutput(
            genericClassTargets.Combine(allWithContext),
            (ctx, pair) => EmitAttributeSource(ctx, pair.Left, pair.Right.Left.Left, pair.Right.Left.Right, pair.Right.Right, this.GetType()));

        context.RegisterSourceOutput(
            appClassTargets.Combine(allWithContext),
            (ctx, pair) => EmitAttributeSource(ctx, pair.Left, pair.Right.Left.Left, pair.Right.Left.Right, pair.Right.Right, this.GetType()));

        context.RegisterSourceOutput(
            markerClassTargets.Combine(allWithContext),
            (ctx, pair) => EmitAttributeSource(ctx, pair.Left, pair.Right.Left.Left, pair.Right.Left.Right, pair.Right.Right, this.GetType()));

        context.RegisterSourceOutput(
            featureClassTargets.Combine(allWithContext),
            (ctx, pair) => EmitAttributeSource(ctx, pair.Left, pair.Right.Left.Left, pair.Right.Left.Right, pair.Right.Right, this.GetType()));
    }

    private static bool IsPartialClass(SyntaxNode node, CancellationToken _)
        => node is ClassDeclarationSyntax cls &&
           cls.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword));

    private static DiagnosticsTargetInfo? TransformToTarget(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken _)
    {
        var attr = ctx.Attributes[0];

        // Case A: [VKBlockMarker("id")]
        if (attr.AttributeClass?.ToDisplayString() == MarkerAttributeFullName)
        {
            var classSymbolInner = (INamedTypeSymbol)ctx.TargetSymbol;

            // 1. Identifier: Explicit > Assembly Name
            var id = attr.ConstructorArguments.FirstOrDefault().Value?.ToString();
            if (string.IsNullOrWhiteSpace(id) || id == "Unknown")
            {
                id = classSymbolInner.ContainingAssembly.Name ?? "Unknown";
            }

            // 2. Version: Explicit > null (deferred to Emit phase)
            var ver = attr.NamedArguments.FirstOrDefault(x => x.Key == "Version").Value.Value?.ToString();

            // Extract Dependencies safely with Symbol validation
            var depsArg = attr.NamedArguments.FirstOrDefault(x => x.Key == "Dependencies").Value;
            var dependencies = (depsArg.Kind == TypedConstantKind.Array && !depsArg.Values.IsDefaultOrEmpty)
                ? depsArg.Values
                    .Select(v => v.Value as INamedTypeSymbol)
                    .Where(s => s is not null && IsMarkerType(s))
                    .Select(s => s!.ToDisplayString())
                    .ToArray()
                : Array.Empty<string>();

            var bName = ExtractBlockName(classSymbolInner.Name);

            var finalId = id!.StartsWith(VKBlocksConstants.VKBlocksPrefix)
                ? id
                : $"{VKBlocksConstants.VKBlocksPrefix}.{id}";

            return new BlockMarkerInfo(
                Namespace: classSymbolInner.ContainingNamespace.ToDisplayString(),
                ClassName: classSymbolInner.Name,
                Identifier: $"\"{finalId}\"",
                BlockName: $"\"{bName}\"",
                Version: ver is not null ? $"\"{ver}\"" : null,
                Modifiers: GetModifiers(classSymbolInner),
                DependencyTypes: dependencies.Length > 0 ? dependencies : null
            );
        }

        // Case B: [VKFeatureMarker("id", typeof(ParentBlock))]
        if (attr.AttributeClass?.ToDisplayString() == FeatureAttributeFullName)
        {
            var id = attr.ConstructorArguments.ElementAtOrDefault(0).Value?.ToString() ?? "Unknown";
            var parentSymbol = attr.ConstructorArguments.ElementAtOrDefault(1).Value as INamedTypeSymbol;

            if (parentSymbol is null)
                return null;

            var ver = attr.NamedArguments.FirstOrDefault(x => x.Key == "Version").Value.Value?.ToString();
            var optional = (bool)(attr.NamedArguments.FirstOrDefault(x => x.Key == "IsOptional").Value.Value ?? true);

            var parentTypeName = parentSymbol.ToDisplayString();
            var classSymbolInner = (INamedTypeSymbol)ctx.TargetSymbol;

            var fName = ExtractBlockName(classSymbolInner.Name);

            return new FeatureMarkerInfo(
                Namespace: classSymbolInner.ContainingNamespace.ToDisplayString(),
                ClassName: classSymbolInner.Name,
                Identifier: $"$\"{{{parentTypeName}.BlockIdentifier}}.{id}\"",
                BlockName: $"\"{fName}\"",
                Version: ver is not null ? $"\"{ver}\"" : null,
                Modifiers: GetModifiers(classSymbolInner),
                ParentIdentifier: $"{parentTypeName}.BlockIdentifier",
                IsOptional: optional
            );
        }

        string blockExpression;
        string? versionExpression;
        string? description = null;

        // Case C: [VKBlockDiagnostics<TBlock>]
        if (attr.AttributeClass is { IsGenericType: true, TypeArguments.Length: 1 })
        {
            var typeArg = attr.AttributeClass.TypeArguments[0];
            var typeName = typeArg.ToDisplayString();

            blockExpression = $"{typeName}.BlockIdentifier";
            versionExpression = $"{typeName}.BlockVersion";
        }
        else
        {
            // Case D: [VKAppDiagnostics("name", Version = "1.0.0")]
            var appName = attr.ConstructorArguments.FirstOrDefault().Value?.ToString();
            var appVersion = attr.NamedArguments
                                .FirstOrDefault(x => x.Key == "Version").Value.Value?.ToString();
            description = attr.NamedArguments
                                .FirstOrDefault(x => x.Key == "Description").Value.Value?.ToString();

            if (appName is null)
                return null;

            blockExpression = $"\"{appName}\"";
            versionExpression = appVersion is not null ? $"\"{appVersion}\"" : null;
        }

        var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var gName = ExtractBlockName(classSymbol.Name);

        return new GenericDiagnosticsInfo(
            Namespace: classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: classSymbol.Name,
            Identifier: blockExpression,
            BlockName: $"\"GENERIC_{gName}\"", // Obvious marker
            Version: versionExpression,
            Modifiers: GetModifiers(classSymbol),
            Description: description
        );
    }

    private static bool IsMarkerType(INamedTypeSymbol symbol)
    {
        // 1. Check if it implements IVKBlockMarker (Directly or via IVKFeatureMarker)
        var isMarker = symbol.AllInterfaces.Any(i => i.Name == "IVKBlockMarker");

        // 2. Check if it has a static 'Instance' property or field
        var hasInstance = symbol.GetMembers("Instance")
            .Any(m => m is { IsStatic: true });

        return isMarker && hasInstance;
    }

    private static string GetModifiers(INamedTypeSymbol classSymbol)
    {
        // Reconstruct access + static modifier
        var accessibility = classSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "internal"
        };
        var isStatic = classSymbol.IsStatic ? " static" : string.Empty;
        var isSealed = classSymbol.IsSealed ? " sealed" : string.Empty;
        return $"{accessibility}{isStatic}{isSealed}";
    }

    private static void EmitAttributeSource(SourceProductionContext ctx, DiagnosticsTargetInfo info, string? assemblyName, AnalyzerConfigOptionsProvider optionsProvider, string assemblyFallbackVersion, Type generatorType)
    {
        // Guard against execution in non-VK.Blocks assemblies (Rule: Global Guard)
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        var sb = SourceCodeBuilder.CreateWithHeader();

        // Final Version Resolution: 
        // 1. Explicitly set in attribute (info.Version)
        // 2. MSBuild property (Version)
        // 3. Assembly Metadata fallback
        var finalVersion = info.Version;
        if (string.IsNullOrEmpty(finalVersion))
        {
            if (optionsProvider.GlobalOptions.TryGetValue("build_property.Version", out var msbuildVersion) && !string.IsNullOrWhiteSpace(msbuildVersion))
            {
                finalVersion = $"\"{msbuildVersion}\"";
            }
            else
            {
                finalVersion = $"\"{assemblyFallbackVersion}\"";
            }
        }
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Diagnostics.Metrics;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {info.Namespace};");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(info.Description))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {info.Description}");
            sb.AppendLine("/// </summary>");
        }

        // Inheritance part
        var baseInterfaces = info switch
        {
            BlockMarkerInfo => $": IVKBlockMarker, IVKBlockMarkerProvider<{info.ClassName}>",
            FeatureMarkerInfo => $": IVKFeatureMarker, IVKBlockMarkerProvider<{info.ClassName}>",
            _ => string.Empty
        };

        sb.AppendLine($"{info.Modifiers} partial class {info.ClassName} {baseInterfaces}");
        sb.AppendLine("{");

        if (info is BlockMarkerInfo or FeatureMarkerInfo)
        {
            // Constants for compile-time safety
            sb.AppendLine("    /// <summary>The canonical short name of this building block.</summary>");
            sb.AppendLine($"    public const string BlockName = {info.BlockName};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>The machine-readable identifier of this block.</summary>");
            sb.AppendLine($"    public const string BlockIdentifier = {info.Identifier};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>The current version of this block.</summary>");
            sb.AppendLine($"    public const string BlockVersion = {finalVersion};");
            sb.AppendLine();

            // IVKBlockMarkerProvider Implementation
            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine($"    static IVKBlockMarker IVKBlockMarkerProvider<{info.ClassName}>.Instance => Instance;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>Gets the singleton instance of this marker.</summary>");
            sb.AppendLine($"    public static {info.ClassName} Instance {{ get; }} = new();");
            sb.AppendLine();

            // IVKBlockMarker Properties (Referencing Constants)
            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public string Name => BlockName;");
            sb.AppendLine();
            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public string Identifier => BlockIdentifier;");
            sb.AppendLine();
            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public string Version => BlockVersion;");
            sb.AppendLine();

            if (info is BlockMarkerInfo block)
            {
                var deps = block.DependencyTypes is not null
                    ? string.Join(", ", block.DependencyTypes.Select(t => $"{t}.Instance"))
                    : string.Empty;
                sb.AppendLine("    /// <inheritdoc />");
                sb.AppendLine($"    public IReadOnlyList<IVKBlockMarker> Dependencies => (IVKBlockMarker[])[{deps}];");
                sb.AppendLine();
            }
            else if (info is FeatureMarkerInfo feature)
            {
                // Architectural Semantic: A feature inherently depends on its parent block.
                // We extract the parent type name from ParentIdentifier (which is ParentBlock.BlockIdentifier)
                // Wait, ParentIdentifier in BlockInfo is the EXPRESSION "ParentBlock.BlockIdentifier".
                // We need the ACTUAL type name. Let's fix this in the emitter by passing the parent type name too.
                // Actually, the simplest way is to use the info.ParentIdentifier and replace ".BlockIdentifier" with ".Instance".
                var parentInstance = feature.ParentIdentifier.Replace(".BlockIdentifier", ".Instance");
                sb.AppendLine("    /// <inheritdoc />");
                sb.AppendLine($"    public IReadOnlyList<IVKBlockMarker> Dependencies => (IVKBlockMarker[])[{parentInstance}];");
                sb.AppendLine();
            }

            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public string ActivitySourceName => BlockIdentifier;");
            sb.AppendLine();
            sb.AppendLine("    /// <inheritdoc />");
            sb.AppendLine("    public string MeterName => BlockIdentifier;");
            sb.AppendLine();

            if (info is FeatureMarkerInfo fFeature)
            {
                // IVKFeatureMarker Properties
                sb.AppendLine("    /// <inheritdoc />");
                sb.AppendLine($"    public string ParentBlockIdentifier => {fFeature.ParentIdentifier};");
                sb.AppendLine();
                sb.AppendLine("    /// <inheritdoc />");
                sb.AppendLine($"    public bool IsOptional => {(fFeature.IsOptional ? "true" : "false")};");
            }
        }

        // Diagnostics Fields
        if (info is FeatureMarkerInfo f)
        {
            // PROXY MODE: Features use their parent's source to avoid noise
            sb.AppendLine("    /// <summary>Gets the ActivitySource (proxied to parent block).</summary>");
            sb.AppendLine($"    public static readonly ActivitySource Source = new({f.ParentIdentifier}, {finalVersion});");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>Gets the Meter (proxied to parent block).</summary>");
            sb.AppendLine($"    public static readonly Meter Meter = new({f.ParentIdentifier}, {finalVersion});");
        }
        else
        {
            // STANDALONE MODE: Root blocks or apps have their own telemetry sources
            var idSource = info is BlockMarkerInfo ? "BlockIdentifier" : info.Identifier;
            var verSource = info is BlockMarkerInfo ? "BlockVersion" : finalVersion;

            sb.AppendLine("    /// <summary>Gets the ActivitySource for this block.</summary>");
            sb.AppendLine($"    public static readonly ActivitySource Source = new({idSource}, {verSource});");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>Gets the Meter for this block.</summary>");
            sb.AppendLine($"    public static readonly Meter Meter = new({idSource}, {verSource});");
        }

        sb.AppendLine("}");

        ctx.AddSource($"{info.ClassName}.g.cs", sb.ToString());
    }

    private static string GetAssemblyVersion(IAssemblySymbol assembly)
    {
        // 1. Try AssemblyInformationalVersionAttribute (supports semantic versioning)
        var infoVersionAttr = assembly.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.Reflection.AssemblyInformationalVersionAttribute");

        if (infoVersionAttr is { ConstructorArguments.Length: > 0 })
        {
            var value = infoVersionAttr.ConstructorArguments[0].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
                return value!;
        }

        // 2. Fallback to AssemblyVersion (AssemblyIdentity)
        return assembly.Identity.Version?.ToString() ?? "1.0.0";
    }

    private static string ExtractBlockName(string className)
    {
        var name = className;
        if (name.StartsWith("VK"))
            name = name.Substring(2);
        if (name.EndsWith("Block"))
            name = name.Substring(0, name.Length - 5);
        if (name.EndsWith("Marker"))
            name = name.Substring(0, name.Length - 6);
        if (name.EndsWith("Feature"))
            name = name.Substring(0, name.Length - 7);
        return name;
    }
}
