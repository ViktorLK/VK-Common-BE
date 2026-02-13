
namespace VK.Blocks.Persistence.Abstractions.Options;

public class PersistenceOptions
{
    public bool EnableAuditing { get; set; } = true;

    public bool EnableSoftDelete { get; set; } = true;
}
