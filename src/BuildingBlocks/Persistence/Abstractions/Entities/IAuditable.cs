
namespace VK.Blocks.Persistence.Abstractions.Entities;

/// <summary>
/// Interface for auditable entities.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
