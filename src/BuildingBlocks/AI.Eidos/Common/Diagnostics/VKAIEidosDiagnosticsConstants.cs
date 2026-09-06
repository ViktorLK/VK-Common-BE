namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Diagnostic constants for VK.Blocks.AI.Eidos.
/// Governed by [BB.04] Diagnostics Blueprint.
/// </summary>
public static class VKAIEidosDiagnosticsConstants
{
    public const string SystemName = "VK.Blocks.AI.Eidos";

    // Activity Tracing Names
    public const string NegotiationActivity = "Eidos.Negotiation";
    public const string ValidationActivity = "Eidos.Validation";
    public const string RepairActivity = "Eidos.Repair";
    public const string FallbackActivity = "Eidos.Fallback";
    public const string BindingActivity = "Eidos.Binding";
    public const string ProjectionActivity = "Eidos.Projection";

    // Metric Instrument Names
    public static class Metrics
    {
        public const string ValidationErrors = "ai.eidos.contract.validation_errors_total";
        public const string RepairsTotal = "ai.eidos.contract.repairs_total";
        public const string NegotiationMode = "ai.eidos.contract.negotiation_mode_total";
        public const string FallbacksTotal = "ai.eidos.contract.fallbacks_total";
        public const string ValidationDuration = "ai.eidos.contract.validation_duration_ms";
    }

    // Semantic Attribute / Tag Keys
    public static class Tags
    {
        public const string ContractId = "eidos.contract.id";
        public const string Fingerprint = "eidos.contract.fingerprint";
        public const string ContractName = "eidos.contract_name";
        public const string Mode = "eidos.negotiation.mode";
        public const string ErrorCategory = "eidos.validation.error_category";
        public const string PropertyPath = "eidos.validation.property_path";
        public const string IsValid = "eidos.validation.is_valid";
        public const string RepairAttempt = "eidos.repair.attempt";
        public const string FallbackFrom = "eidos.fallback.from";
        public const string FallbackTo = "eidos.fallback.to";
    }
}
