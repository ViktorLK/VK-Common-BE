using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using VK.Tools.SourceGenerators.Diagnostics;

namespace VK.Tools.SourceGenerators.Observability;

/// <summary>
/// Analyzer that ensures EventIds in [LoggerMessage] attributes are unique across the compilation (OR.01 / BB.04).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventIdConflictAnalyzer : DiagnosticAnalyzer
{
    private const string LoggerMessageAttributeFullName = "Microsoft.Extensions.Logging.LoggerMessageAttribute";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [VKDiagnosticDescriptors.DuplicateEventId];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var eventIdMap = new ConcurrentDictionary<int, (string MethodName, Location Location)>();

            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var methodSymbol = (IMethodSymbol)symbolContext.Symbol;

                foreach (var attr in methodSymbol.GetAttributes())
                {
                    if (attr.AttributeClass?.ToDisplayString() != LoggerMessageAttributeFullName)
                        continue;

                    int? eventId = null;

                    // 1. Check constructor arguments
                    if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int ctorId)
                    {
                        eventId = ctorId;
                    }
                    else
                    {
                        // 2. Check named arguments
                        var namedArg = attr.NamedArguments.FirstOrDefault(x => x.Key == "EventId");
                        if (namedArg.Value.Value is int nId)
                        {
                            eventId = nId;
                        }
                    }

                    if (eventId.HasValue && eventId.Value != 0)
                    {
                        var qualifiedMethodName = $"{methodSymbol.ContainingType?.Name}.{methodSymbol.Name}";
                        var location = methodSymbol.Locations.FirstOrDefault() ?? Location.None;

                        if (!eventIdMap.TryAdd(eventId.Value, (qualifiedMethodName, location)))
                        {
                            var existing = eventIdMap[eventId.Value];
                            if (existing.MethodName != qualifiedMethodName)
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    VKDiagnosticDescriptors.DuplicateEventId,
                                    location,
                                    eventId.Value,
                                    qualifiedMethodName,
                                    existing.MethodName));
                            }
                        }
                    }
                }
            }, SymbolKind.Method);
        });
    }
}
