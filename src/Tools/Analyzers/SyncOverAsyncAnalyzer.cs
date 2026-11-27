using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace VK.Tools.Analyzers;

/// <summary>
/// Enforces CS.03 async discipline by prohibiting synchronous blocking (.Result, .Wait(), .GetAwaiter().GetResult()).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SyncOverAsyncAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(VKAnalyzerDescriptors.SyncOverAsyncBlocking);

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

        if (property.Name != "Result")
        {
            return;
        }

        var originalTypeName = property.ContainingType?.OriginalDefinition?.ToDisplayString();
        if (originalTypeName is "System.Threading.Tasks.Task<TResult>" or "System.Threading.Tasks.ValueTask<TResult>")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKAnalyzerDescriptors.SyncOverAsyncBlocking,
                operation.Syntax.GetLocation(),
                $"{property.ContainingType!.Name}.Result"));
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var operation = (IInvocationOperation)context.Operation;
        var method = operation.TargetMethod;
        var originalTypeName = method.ContainingType?.OriginalDefinition?.ToDisplayString();

        if (method.Name == "Wait" &&
            originalTypeName is "System.Threading.Tasks.Task" or "System.Threading.Tasks.Task<TResult>")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKAnalyzerDescriptors.SyncOverAsyncBlocking,
                operation.Syntax.GetLocation(),
                "Task.Wait()"));
            return;
        }

        if (method.Name == "GetResult" &&
            originalTypeName is "System.Runtime.CompilerServices.TaskAwaiter"
                or "System.Runtime.CompilerServices.TaskAwaiter<TResult>"
                or "System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter"
                or "System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter"
                or "System.Runtime.CompilerServices.ValueTaskAwaiter"
                or "System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>"
                or "System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter"
                or "System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VKAnalyzerDescriptors.SyncOverAsyncBlocking,
                operation.Syntax.GetLocation(),
                "GetAwaiter().GetResult()"));
        }
    }
}
