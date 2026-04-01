using VK.Blocks.Mapping.Abstractions;

namespace VK.Blocks.Mapping.AutoMapper;

/// <summary>
/// Adapter implementation of IMapper using AutoMapper.
/// </summary>
public sealed class AutoMapperAdapter : IMapper
{
    private readonly global::AutoMapper.IMapper _mapper;

    public AutoMapperAdapter(global::AutoMapper.IMapper mapper)
    {
        _mapper = mapper;
    }

    public TDestination Map<TDestination>(object source)
    {
        return _mapper.Map<TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        return _mapper.Map(source, destination);
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        return _mapper.ProjectTo<TDestination>(source);
    }
}
