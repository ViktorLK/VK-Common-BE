
namespace VK.Blocks.Web.CorrelationId;

public class CorrelationIdOptions
{
    public string Header { get; set; } = "X-Correlation-ID";
    public bool IncludeInResponse { get; set; } = true;
    public bool UseTraceIdIfAvailable { get; set; } = true;
    public string LogContextPropertyName { get; set; } = "CorrelationId";
}
