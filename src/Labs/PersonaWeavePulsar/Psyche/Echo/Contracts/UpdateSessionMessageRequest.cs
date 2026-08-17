namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Contracts;

public sealed record UpdateSessionMessageRequest
{
    public required string Content { get; init; }
}
