using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultToolProjection : IVKToolProjection
{
    public IVKAtomicTool ProjectToTool(VKAIEidosResponseContract contract, bool injectNarrativeField = false, bool allowSegmentation = true)
    {
        VKGuard.NotNull(contract);
        var schema = injectNarrativeField
            ? DefaultSchemaProjection.InjectNarrativeFieldToSchema(contract.Schema.RawJsonSchema, allowSegmentation)
            : contract.Schema.RawJsonSchema;
        return new EidosDynamicContractTool(contract, schema);
    }

    private sealed class EidosDynamicContractTool(VKAIEidosResponseContract contract, string schema) : IVKAtomicTool
    {
        public VKAtomicToolManifest Manifest { get; } = new VKAtomicToolManifest
        {
            Metadata = new VKAtomicToolMetadata
            {
                Name = contract.Schema.SchemaName,
                Description = contract.Description,
                Category = "EidosContract"
            },
            ParameterSchema = schema
        };

        public Task<VKResult<VKAtomicToolResult>> ExecuteAsync(
            IDictionary<string, object> arguments,
            VKAgentExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(arguments);
            return Task.FromResult(VKResult.Success(new VKAtomicToolResult
            {
                Content = json
            }));
        }
    }
}
