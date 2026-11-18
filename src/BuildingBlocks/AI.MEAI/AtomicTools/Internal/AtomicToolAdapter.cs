using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.AtomicTools.Internal;

/// <summary>
/// Adapts pure IVKAtomicTool definitions to MEAI AIFunction instances while preserving full JSON schemas
/// and executing IVKAtomicToolFilter onion pipelines.
/// Adheres to AP.01, AP.03, ARCH-07, ARCH-09, and ARCH-11.
/// </summary>
public static class AtomicToolAdapter
{
    private static readonly ConcurrentDictionary<string, JsonElement> SchemaCache = new(StringComparer.Ordinal);
    private static readonly JsonElement DefaultObjectSchema;

    static AtomicToolAdapter()
    {
        using var doc = JsonDocument.Parse("{\"type\":\"object\"}");
        DefaultObjectSchema = doc.RootElement.Clone();
    }

    public static AIFunction ToAIFunction(
        IVKAtomicTool tool,
        VKAtomicToolContext? context = null,
        IEnumerable<IVKAtomicToolFilter>? filters = null,
        IVKAtomicToolApprovalHandler? approvalHandler = null,
        int? maxResultLength = null)
    {
        VKGuard.NotNull(tool);
        return new AtomicAIFunction(tool, context, filters, approvalHandler, maxResultLength);
    }

    /// <summary>
    /// Adapts and filters atomic tools for both direct model calls and upstream agent runtimes (A1).
    /// </summary>
    public static IList<AITool> AdaptTools(
        IEnumerable<IVKAtomicTool>? explicitTools,
        IEnumerable<IVKAtomicTool>? registeredTools = null,
        IReadOnlyList<string>? enabledToolNames = null,
        VKAtomicToolContext? context = null,
        IEnumerable<IVKAtomicToolFilter>? filters = null,
        IVKAtomicToolApprovalHandler? approvalHandler = null,
        int? maxResultLength = null)
    {
        var selectedTools = VKAtomicToolPipeline.SelectTools(explicitTools, registeredTools, enabledToolNames);
        if (selectedTools.Count == 0)
        {
            return [];
        }

        var filterList = filters as IReadOnlyCollection<IVKAtomicToolFilter> ?? filters?.ToList();
        var adapted = new List<AITool>(selectedTools.Count);

        foreach (var tool in selectedTools)
        {
            adapted.Add(ToAIFunction(tool, context, filterList, approvalHandler, maxResultLength));
        }

        return adapted;
    }

    private sealed class AtomicAIFunction : AIFunction
    {
        private readonly IVKAtomicTool _tool;
        private readonly VKAtomicToolContext? _context;
        private readonly IReadOnlyList<IVKAtomicToolFilter> _filters;
        private readonly IVKAtomicToolApprovalHandler? _approvalHandler;
        private readonly int? _maxResultLength;
        private readonly string _name;
        private readonly string _description;
        private readonly JsonElement _jsonSchema;

        public override string Name => _name;
        public override string Description => _description;
        public override JsonElement JsonSchema => _jsonSchema;

        public AtomicAIFunction(
            IVKAtomicTool tool,
            VKAtomicToolContext? context,
            IEnumerable<IVKAtomicToolFilter>? filters,
            IVKAtomicToolApprovalHandler? approvalHandler,
            int? maxResultLength)
        {
            _tool = VKGuard.NotNull(tool);
            _context = context;
            _filters = filters?.OrderBy(static f => f.Order).ToList() ?? [];
            _approvalHandler = approvalHandler;
            _maxResultLength = maxResultLength;
            _name = _tool.Manifest.Metadata.Name;
            _description = _tool.Manifest.Metadata.Description;

            var paramSchema = _tool.Manifest.ParameterSchema;
            if (!string.IsNullOrWhiteSpace(paramSchema))
            {
                _jsonSchema = SchemaCache.GetOrAdd(paramSchema, static s =>
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(s);
                        return doc.RootElement.Clone();
                    }
                    catch
                    {
                        return DefaultObjectSchema;
                    }
                });
            }
            else
            {
                _jsonSchema = DefaultObjectSchema;
            }
        }

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments,
            CancellationToken cancellationToken)
        {
            var dict = new Dictionary<string, object>();

            if (arguments is not null)
            {
                foreach (var kvp in arguments)
                {
                    dict[kvp.Key] = kvp.Value ?? string.Empty;
                }
            }

            // [ARCH-01] Delegate tool execution, confirmation, filtering, and history tracking to shared pure-VK core.
            var result = await VKAtomicToolPipeline.ExecuteWithFiltersAsync(
                _tool,
                dict,
                _context,
                _filters,
                _approvalHandler,
                _maxResultLength,
                cancellationToken).ConfigureAwait(false);

            return VKAtomicToolPipeline.FormatToolResult(result);
        }
    }
}
