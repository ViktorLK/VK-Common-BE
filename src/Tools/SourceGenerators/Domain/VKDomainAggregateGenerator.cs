using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;

namespace VK.Tools.SourceGenerators.Domain;

/// <summary>
/// Incremental Source Generator that produces domain aggregate root boilerplate:
/// 1. Snapshot property encapsulation.
/// 2. Snapshot reconstruction constructor with base constructor linking.
/// 3. Read-only forwarding properties from Snapshot.
/// 4. Auditing interface mutable property forwarders.
/// Follows AP.01, AP.03, CS.01.
/// </summary>
[Generator]
public sealed class VKDomainAggregateGenerator : IIncrementalGenerator
{
    private const string AttributeName = "VKDomainAggregateAttribute";
    private const string AttributeFullName = $"VK.Blocks.Core.{AttributeName}";
    private const string DomainIgnoreAttributeFullName = "VK.Blocks.Core.VKDomainIgnoreAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformTarget(ctx))
            .Where(static t => t is not null);

        var assemblyNameProvider = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        context.RegisterSourceOutput(
            targets.Combine(assemblyNameProvider),
            (ctx, pair) => EmitSource(ctx, pair.Left!, pair.Right, typeof(VKDomainAggregateGenerator)));
    }

    private static AggregateTargetInfo? TransformTarget(GeneratorAttributeSyntaxContext ctx)
    {
        var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
        var targetSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attribute = ctx.Attributes[0];

        if (attribute.ConstructorArguments.Length < 1)
            return null;

        var snapshotType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
        if (snapshotType is null)
            return null;

        var isPartial = classDeclaration.Modifiers.Any(m => m.Text == "partial");
        var accessibility = targetSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            _ => "public"
        };

        var snapshotProps = snapshotType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsIndexer && p.DeclaredAccessibility == Accessibility.Public && p.GetMethod is not null && !string.Equals(p.Name, "EqualityContract", StringComparison.Ordinal))
            .ToList();

        var idProp = snapshotProps.FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
        var hasBaseIdConstructor = false;

        if (idProp is not null && targetSymbol.BaseType is not null)
        {
            foreach (var ctor in targetSymbol.BaseType.Constructors)
            {
                if (ctor.Parameters.Length == 1 &&
                    SymbolEqualityComparer.Default.Equals(ctor.Parameters[0].Type, idProp.Type))
                {
                    hasBaseIdConstructor = true;
                    break;
                }
            }
        }

        var isFullAuditable = targetSymbol.ImplementsInterface("IVKFullAuditable");
        var isAuditable = isFullAuditable || targetSymbol.ImplementsInterface("IVKAuditable");
        var isCreationAudited = isAuditable || targetSymbol.ImplementsInterface("IVKCreationAudited");
        var isModificationAudited = isAuditable || targetSymbol.ImplementsInterface("IVKModificationAudited");
        var isSoftDeletable = isFullAuditable || targetSymbol.ImplementsInterface("IVKSoftDeletable") || targetSymbol.ImplementsInterface("IVKSoftDeleteAudited");
        var isDeletionAudited = isFullAuditable || targetSymbol.ImplementsInterface("IVKDeletionAudited") || targetSymbol.ImplementsInterface("IVKSoftDeleteAudited");

        var properties = new List<AggregatePropertyInfo>();

        foreach (var prop in snapshotProps)
        {
            // 1. Skip if property is already declared on target class or any base class (e.g. Id)
            if (HasPropertyInHierarchy(targetSymbol, prop.Name))
            {
                continue;
            }

            // 2. Skip if marked with [VKDomainIgnore]
            if (prop.HasAttribute(DomainIgnoreAttributeFullName))
            {
                continue;
            }

            // 4. Check if it's an audit property requiring getter and setter
            var isAuditMutable = false;
            if (prop.Name == "IsDeleted" && isSoftDeletable)
            {
                isAuditMutable = true;
            }
            else if ((prop.Name == "CreatedAt" || prop.Name == "CreatedBy") && isCreationAudited)
            {
                isAuditMutable = true;
            }
            else if ((prop.Name == "UpdatedAt" || prop.Name == "UpdatedBy") && isModificationAudited)
            {
                isAuditMutable = true;
            }
            else if ((prop.Name == "DeletedAt" || prop.Name == "DeletedBy") && isDeletionAudited)
            {
                isAuditMutable = true;
            }

            properties.Add(new AggregatePropertyInfo(
                Name: prop.Name,
                TypeFullName: prop.Type.ToDisplayString(),
                IsAuditMutable: isAuditMutable
            ));
        }

        return new AggregateTargetInfo(
            Namespace: targetSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: targetSymbol.Name,
            Accessibility: accessibility,
            IsPartial: isPartial,
            Location: classDeclaration.Identifier.GetLocation(),
            SnapshotTypeName: snapshotType.Name,
            SnapshotTypeFullName: snapshotType.ToDisplayString(),
            HasBaseIdConstructor: hasBaseIdConstructor,
            Properties: properties
        );
    }

    private static bool HasPropertyInHierarchy(INamedTypeSymbol? symbol, string propertyName)
    {
        var current = symbol;
        while (current is not null)
        {
            if (current.GetMembers(propertyName).OfType<IPropertySymbol>().Any())
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static void EmitSource(
        SourceProductionContext ctx,
        AggregateTargetInfo target,
        string? assemblyName,
        Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        if (!target.IsPartial)
        {
            var diagnostic = VKDiagnostics.CreateTypeMustBePartial(
                "domain aggregate/entity",
                target.ClassName,
                target.Location);
            ctx.ReportDiagnostic(diagnostic);
            return;
        }

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"{target.Accessibility} partial class {target.ClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets the encapsulated immutable state snapshot.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public {target.SnapshotTypeFullName} Snapshot {{ get; private set; }}");
        sb.AppendLine();

        var baseCall = target.HasBaseIdConstructor ? " : base(snapshot.Id)" : string.Empty;

        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Constructs an instance of <see cref=\"{target.ClassName}\"/> from an existing state snapshot.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public {target.ClassName}({target.SnapshotTypeFullName} snapshot){baseCall}");
        sb.AppendLine("    {");
        sb.AppendLine("        Snapshot = VKGuard.NotNull(snapshot);");
        sb.AppendLine("        OnInitialized(snapshot);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    partial void OnInitialized({target.SnapshotTypeFullName} snapshot);");
        sb.AppendLine();

        foreach (var prop in target.Properties)
        {
            if (prop.IsAuditMutable)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// Gets or sets the {prop.Name} state.");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public {prop.TypeFullName} {prop.Name}");
                sb.AppendLine("    {");
                sb.AppendLine($"        get => Snapshot.{prop.Name};");
                sb.AppendLine($"        set => Snapshot = Snapshot with {{ {prop.Name} = value }};");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// Gets the {prop.Name} state.");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public {prop.TypeFullName} {prop.Name} => Snapshot.{prop.Name};");
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        ctx.AddSource($"{target.ClassName}.g.cs", sb.ToString());
    }

    private sealed record AggregateTargetInfo(
        string Namespace,
        string ClassName,
        string Accessibility,
        bool IsPartial,
        Location Location,
        string SnapshotTypeName,
        string SnapshotTypeFullName,
        bool HasBaseIdConstructor,
        List<AggregatePropertyInfo> Properties
    );

    private sealed record AggregatePropertyInfo(
        string Name,
        string TypeFullName,
        bool IsAuditMutable
    );
}
