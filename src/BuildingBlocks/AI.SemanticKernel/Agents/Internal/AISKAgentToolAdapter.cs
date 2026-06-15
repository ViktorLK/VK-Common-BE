using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Agents.Internal;

/// <summary>
/// Adapter to convert <see cref="IVKAtomicTool"/> to Semantic Kernel <see cref="KernelFunction"/>.
/// </summary>
internal sealed class AISKAgentToolAdapter
{
    public static KernelFunction ToKernelFunction(IVKAtomicTool tool)
    {
        VKGuard.NotNull(tool);

        var metadata = tool.Manifest.Metadata;
        var parameters = ParseParameters(tool.Manifest.ParameterSchema);

        // Wrapper to execute the underlying tool
        async Task<string> WrapperAsync(KernelArguments args, CancellationToken ct)
        {
            var dictionary = args as IDictionary<string, object?>
                 ?? args.ToDictionary(k => k.Key, v => v.Value);

            var context = new VKAgentExecutionContext();
            var result = await tool.ExecuteAsync(dictionary, context, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return result.Value?.Content ?? string.Empty;
            }

            // Return error message so LLM can understand the failure
            return $"Tool Execution Failed: {result.Errors.FirstOrDefault()?.Description ?? "Unknown error"}";
        }

        var returnParameter = new KernelReturnParameterMetadata { Description = "The result of the tool execution.", ParameterType = typeof(string) };
        if (!string.IsNullOrWhiteSpace(tool.Manifest.ReturnSchema))
        {
            try
            {
                returnParameter.Schema = KernelJsonSchema.Parse(tool.Manifest.ReturnSchema);
            }
            catch (JsonException)
            {
                // Ignore malformed schema
            }
        }

        var functionOptions = new KernelFunctionFromMethodOptions
        {
            FunctionName = metadata.Name,
            Description = metadata.Description,
            Parameters = parameters,
            ReturnParameter = returnParameter
        };

        // Return a KernelFunction constructed from the wrapper
        return KernelFunctionFactory.CreateFromMethod(WrapperAsync, functionOptions);
    }

    private static List<KernelParameterMetadata> ParseParameters(string? jsonSchema)
    {
        var parameters = new List<KernelParameterMetadata>();
        if (string.IsNullOrWhiteSpace(jsonSchema))
        {
            return parameters;
        }

        try
        {
            using var document = JsonDocument.Parse(jsonSchema);
            var root = document.RootElement;

            if (root.TryGetProperty("properties", out var propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
            {
                var requiredList = new HashSet<string>();
                if (root.TryGetProperty("required", out var requiredElement) && requiredElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var req in requiredElement.EnumerateArray())
                    {
                        var reqStr = req.GetString();
                        if (!string.IsNullOrEmpty(reqStr))
                        {
                            requiredList.Add(reqStr);
                        }
                    }
                }

                foreach (var prop in propertiesElement.EnumerateObject())
                {
                    var propName = prop.Name;
                    var propTypeStr = prop.Value.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : "string";
                    var propDesc = prop.Value.TryGetProperty("description", out var descEl) ? descEl.GetString() : string.Empty;

                    var type = propTypeStr?.ToLowerInvariant() switch
                    {
                        "integer" => typeof(int),
                        "number" => typeof(double),
                        "boolean" => typeof(bool),
                        _ => typeof(string)
                    };

                    parameters.Add(new KernelParameterMetadata(propName)
                    {
                        Description = propDesc,
                        ParameterType = type,
                        IsRequired = requiredList.Contains(propName)
                    });
                }
            }
        }
        catch (JsonException)
        {
            // Ignore malformed schema for now
        }

        return parameters;
    }
}
internal sealed class VKAgentToolInvocationLogger : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"[TOOL PULSE TRIGGERED] Executing: {context.Function.Name}");
        await next(context).ConfigureAwait(false);
    }
}
