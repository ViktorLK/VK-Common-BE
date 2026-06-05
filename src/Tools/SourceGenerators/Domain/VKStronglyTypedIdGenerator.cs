using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Utilities;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Domain;

[Generator]
public sealed class VKStronglyTypedIdGenerator : IIncrementalGenerator
{
    private const string AttributeName = "VKStronglyTypedIdAttribute";
    private const string AttributeFullName = $"VK.Blocks.Core.{AttributeName}";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyNameProvider = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        // Find target types
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is RecordDeclarationSyntax { ClassOrStructKeyword.ValueText: "struct" },
                transform: GetTargetRecordStruct)
            .WhereNotNull();

        // 3. Generate
        context.RegisterSourceOutput(
            provider.Combine(assemblyNameProvider),
            (ctx, pair) => EmitSource(ctx, pair.Left, pair.Right, this.GetType()));
    }

    private static TargetRecordStruct? GetTargetRecordStruct(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var recordDeclaration = (RecordDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(recordDeclaration, ct) as INamedTypeSymbol;

        if (symbol is null)
            return null;

        var hasAttribute = symbol.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == AttributeFullName ||
            a.AttributeClass?.Name == AttributeName);

        if (!hasAttribute)
            return null;

        var isPartial = recordDeclaration.Modifiers.Any(m => m.Text == "partial");

        var hasEfCore = context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter`2") != null;

        return new TargetRecordStruct(
            Namespace: symbol.ContainingNamespace.ToDisplayString(),
            Name: symbol.Name,
            IsPartial: isPartial,
            HasEfCore: hasEfCore
        );
    }

    private static void EmitSource(SourceProductionContext ctx, TargetRecordStruct target, string? assemblyName, Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        if (!target.IsPartial)
        {
            // Error diagnostic should be emitted in a full implementation.
            return;
        }

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine("using System.Globalization;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        if (target.HasEfCore)
        {
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Storage.ValueConversion;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.ChangeTracking;");
        }
        sb.AppendLine("using VK.Blocks.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace};");
        sb.AppendLine();

        sb.AppendLine($"[JsonConverter(typeof({target.Name}JsonConverter))]");
        sb.AppendLine($"[TypeConverter(typeof({target.Name}TypeConverter))]");
        sb.AppendLine($"public partial record struct {target.Name}(Guid Value) : IComparable<{target.Name}>, IParsable<{target.Name}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public static {target.Name} Empty => new(Guid.Empty);");
        sb.AppendLine("    public bool IsEmpty => Value == Guid.Empty;");
        sb.AppendLine();
        sb.AppendLine($"    public int CompareTo({target.Name} other) => Value.CompareTo(other.Value);");
        sb.AppendLine();
        sb.AppendLine("    public override string ToString() => Value.ToString();");
        sb.AppendLine();
        sb.AppendLine($"    public static implicit operator {target.Name}(Guid value) => new(value);");
        sb.AppendLine($"    public static {target.Name} New(IVKGuidGenerator generator) => new(generator.Create());");
        sb.AppendLine();
        sb.AppendLine($"    public static {target.Name} Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));");
        sb.AppendLine($"    public static bool TryParse(string? s, IFormatProvider? provider, out {target.Name} result)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (Guid.TryParse(s, provider, out var guid))");
        sb.AppendLine("        {");
        sb.AppendLine($"            result = new {target.Name}(guid);");
        sb.AppendLine("            return true;");
        sb.AppendLine("        }");
        sb.AppendLine("        result = Empty;");
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // JSON Converter
        sb.AppendLine($"public class {target.Name}JsonConverter : JsonConverter<{target.Name}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public override {target.Name} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetGuid());");
        sb.AppendLine($"    public override void Write(Utf8JsonWriter writer, {target.Name} value, JsonSerializerOptions options) => writer.WriteStringValue(value.Value);");
        sb.AppendLine("}");
        sb.AppendLine();

        if (target.HasEfCore)
        {
            // EF Core Converter
            sb.AppendLine($"public class {target.Name}EfCoreConverter : ValueConverter<{target.Name}, Guid>");
            sb.AppendLine("{");
            sb.AppendLine($"    public {target.Name}EfCoreConverter() : base(id => id.Value, value => new {target.Name}(value)) {{ }}");
            sb.AppendLine("}");
            sb.AppendLine();

            // EF Core Value Comparer
            sb.AppendLine($"public class {target.Name}ValueComparer : ValueComparer<{target.Name}>");
            sb.AppendLine("{");
            sb.AppendLine($"    public {target.Name}ValueComparer() : base((l, r) => l.Value == r.Value, v => v.Value.GetHashCode(), v => new {target.Name}(v.Value)) {{ }}");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Type Converter
        sb.AppendLine($"public class {target.Name}TypeConverter : TypeConverter");
        sb.AppendLine("{");
        sb.AppendLine("    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);");
        sb.AppendLine("    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (value is string stringValue && Guid.TryParse(stringValue, out var guid))");
        sb.AppendLine("        {");
        sb.AppendLine($"            return new {target.Name}(guid);");
        sb.AppendLine("        }");
        sb.AppendLine("        return base.ConvertFrom(context, culture, value);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{target.Name}.g.cs", sb.ToString());
    }

    private sealed record TargetRecordStruct(string Namespace, string Name, bool IsPartial, bool HasEfCore);
}
