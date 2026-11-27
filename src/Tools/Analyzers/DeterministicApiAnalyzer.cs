using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace VK.Tools.Analyzers;

/// <summary>
/// Enforces CS.06 determinism rules by prohibiting direct access to system clocks, Guid.NewGuid(), and JsonSerializer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeterministicApiAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            VKAnalyzerDescriptors.NonDeterministicClockAccess,
            VKAnalyzerDescriptors.NonDeterministicGuidGeneration,
            VKAnalyzerDescriptors.DirectJsonSerializerUsage);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context)
    {
        var operation = (IPropertyReferenceOperation)context.Operation;
        var property = operation.Property;

        if (property.Name is not ("Now" or "UtcNow"))
        {
            return;
        }

        var containingType = property.ContainingType;
        if (containingType is null)
        {
            return;
        }

        var typeName = containingType.ToDisplayString();
        if (typeName is "System.DateTime" or "System.DateTimeOffset")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKAnalyzerDescriptors.NonDeterministicClockAccess,
                operation.Syntax.GetLocation(),
                $"{containingType.Name}.{property.Name}"));
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var operation = (IInvocationOperation)context.Operation;
        var method = operation.TargetMethod;
        var containingTypeName = method.ContainingType?.ToDisplayString();

        if (containingTypeName == "System.Guid" && method.Name == "NewGuid")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKAnalyzerDescriptors.NonDeterministicGuidGeneration,
                operation.Syntax.GetLocation()));
            return;
        }

        if (containingTypeName == "System.Text.Json.JsonSerializer" &&
            method.Name is "Serialize" or "Deserialize" or "SerializeToUtf8Bytes" or "SerializeAsync" or "DeserializeAsync")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKAnalyzerDescriptors.DirectJsonSerializerUsage,
                operation.Syntax.GetLocation(),
                method.Name));
        }
    }
}
