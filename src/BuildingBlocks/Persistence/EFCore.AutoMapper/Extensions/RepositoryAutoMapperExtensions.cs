using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Linq.Expressions;
using VK.Blocks.Persistence.EFCore.Extensions;

namespace VK.Blocks.Persistence.EFCore.AutoMapper.Extensions;

/// <summary>
/// Repository projection extensions using AutoMapper
/// </summary>
public static class RepositoryAutoMapperExtensions
{
    public static Task<IReadOnlyList<TDto>> GetListProjectedAsync<TEntity, TDto>(
            this IBaseRepository<TEntity> repository,
            Expression<Func<TEntity, bool>>? predicate,
            IConfigurationProvider configuration,
            CancellationToken cancellationToken = default)
            where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(configuration);

        return repository.ExecuteAsync(query =>
        {
            return query.WhereIf(predicate is not null, predicate!).ProjectTo<TDto>(configuration);
        }, cancellationToken);
    }

}
