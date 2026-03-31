using AutoMapper;
using VK.Blocks.Mapping.Core;

namespace VK.Blocks.Mapping.AutoMapper;

/// <summary>
/// Bridge profile that adapts VK.Blocks.Mapping.Core.MappingProfile to AutoMapper's Profile.
/// </summary>
public abstract class AutoMapperProfile : Profile
{
    // Implementation to bridge the two.
    // For now, it will be used as a base for AutoMapper specific profiles 
    // or as a registry logic.
}
