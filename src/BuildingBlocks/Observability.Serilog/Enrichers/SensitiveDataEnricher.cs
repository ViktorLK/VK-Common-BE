using Serilog.Core;
using Serilog.Events;

namespace VK.Blocks.Observability.Serilog.Enrichers;

/// <summary>
/// A log event enricher that masks sensitive data based on a list of keywords.
/// </summary>
public sealed class SensitiveDataEnricher : ILogEventEnricher
{
    private readonly string[] _sensitiveKeywords;
    private const string Mask = "***MASKED***";

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveDataEnricher"/> class.
    /// </summary>
    /// <param name="sensitiveKeywords">The list of keywords to mask. Masking checks use case-insensitive partial matching.</param>
    public SensitiveDataEnricher(IEnumerable<string> sensitiveKeywords)
    {
        _sensitiveKeywords = sensitiveKeywords.ToArray();
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        List<string>? keysToMask = null;

        foreach (var key in logEvent.Properties.Keys)
        {
            if (IsSensitiveProperty(key))
            {
                keysToMask ??= [];
                keysToMask.Add(key);
            }
        }

        if (keysToMask != null)
        {
            foreach (var key in keysToMask)
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(key, Mask));
            }
        }
    }

    private bool IsSensitiveProperty(string propertyName)
    {
        // Use IndexOf with StringComparison.OrdinalIgnoreCase for faster partial matching without allocations
        foreach (var keyword in _sensitiveKeywords)
        {
            if (propertyName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
}
