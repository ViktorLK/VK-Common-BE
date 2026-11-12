using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKTokenicsDiagnosticsConstants
{
    public const int QuotaExceededEventId = VKAIDiagnosticOffsets.Tokenics + 1;
    public const int TokenCalculationPerformedEventId = VKAIDiagnosticOffsets.Tokenics + 2;
    public const int TokenicsOperationFailedEventId = VKAIDiagnosticOffsets.Tokenics + 3;
}
