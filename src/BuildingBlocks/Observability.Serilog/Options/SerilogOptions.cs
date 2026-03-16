namespace VK.Blocks.Observability.Serilog.Options;

/// <summary>
/// Configuration options for the Serilog observability block.
/// </summary>
public sealed record SerilogOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "VK:Observability:Serilog";

    /// <summary>
    /// Gets or sets a value indicating whether Serilog integration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the TraceContext enricher.
    /// </summary>
    public bool EnableTraceContext { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the UserContext enricher.
    /// </summary>
    public bool EnableUserContext { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the Application enricher.
    /// </summary>
    public bool EnableApplicationEnricher { get; init; } = true;

    /// <summary>
    /// Gets or sets the list of keywords to mask in logs.
    /// </summary>
    public List<string> SensitiveKeywords { get; init; } =
    [
        "Password", "Token", "Secret", "ApiKey", "CreditCard", "SSN", "Bearer"
    ];

    /// <summary>
    /// Gets or sets the console sink configuration.
    /// </summary>
    public ConsoleOptions Console { get; init; } = new();

    /// <summary>
    /// Gets or sets the file sink configuration.
    /// </summary>
    public FileOptions File { get; init; } = new();

    /// <summary>
    /// Nested options for console output.
    /// </summary>
    public sealed record ConsoleOptions
    {
        public bool Enabled { get; init; } = true;
        public string OutputTemplate { get; init; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
    }

    /// <summary>
    /// Nested options for file output.
    /// </summary>
    public sealed record FileOptions
    {
        public bool Enabled { get; init; } = false;
        public string Path { get; init; } = "logs/log-.txt";
        public string OutputTemplate { get; init; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
        public int RetainedFileCountLimit { get; init; } = 31;
    }
}
