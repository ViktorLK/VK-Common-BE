using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Persistence.EFCore.Tests;

public class TestProduct : IAuditable, ISoftDelete
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // // IConcurrency
    // [ConcurrencyCheck]
    // public byte[] RowVersion { get; set; } = [];
}
