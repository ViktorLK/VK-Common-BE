namespace VK.Labs.PersonaWeavePulsar.Psyche.Session.Contracts;

public sealed record CreateSessionRequest
{
    public required string PersonaId { get; init; }
    public string? ModelId { get; init; }
    public string? ApiKey { get; init; }
    public string? Endpoint { get; init; }
    public string? ServiceType { get; init; }
}
