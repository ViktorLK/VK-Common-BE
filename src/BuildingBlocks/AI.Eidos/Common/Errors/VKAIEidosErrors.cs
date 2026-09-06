using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Domain-specific error constants for AI.Eidos BuildingBlock.
/// Governed by [CS.01]: Result&lt;T&gt; only. Error constants on Errors class.
/// Format: AI.Eidos.{Category}.{Reason}
/// </summary>
public static class VKAIEidosErrors
{
    public static class Binding
    {
        public static readonly VKError NullResult = VKError.Validation(
            "AI.Eidos.Binding.NullResult",
            "Deserialization produced a null object.");

        public static VKError JsonError(string details) => VKError.Validation(
            "AI.Eidos.Binding.JsonError",
            $"JSON deserialization failed: {details}");

        public static VKError ToleranceModeNotSupported(VKMaterializationToleranceMode mode) => VKError.Validation(
            "AI.Eidos.Binding.ToleranceModeNotSupported",
            $"ToleranceMode '{mode}' is not supported yet. Only Strict mode is currently implemented.");
    }

    public static class Schema
    {
        public static VKError MalformedJsonSchema(string details) => VKError.Validation(
            "AI.Eidos.Schema.MalformedJsonSchema",
            $"Target schema RawJsonSchema is malformed JSON: {details}");

        public static readonly VKError SchemaNotFound = VKError.NotFound(
            "AI.Eidos.Schema.NotFound",
            "Specified Eidos schema contract could not be resolved.");
    }

    public static class Materialization
    {
        public static readonly VKError RepairFailed = VKError.Failure(
            "AI.Eidos.Materialization.RepairFailed",
            "Self-healing repair exhausted all retry attempts without producing a compliant payload.");

        public static readonly VKError CircuitBreakerOpen = VKError.Failure(
            "AI.Eidos.Materialization.CircuitBreakerOpen",
            "Materialization circuit breaker tripped due to consecutive schema violations.");
    }
}
