using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheDirectiveRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryDirectiveRepository : VKInMemoryAggregateRepository<VKDirectiveCharter, VKDirectiveId>, IVKPsycheDirectiveRepository
{
    private readonly ILogger<InMemoryDirectiveRepository>? _logger;

    public InMemoryDirectiveRepository()
    {
    }

    public InMemoryDirectiveRepository(ILogger<InMemoryDirectiveRepository> logger)
    {
        _logger = logger;
        _logger?.DirectiveInitialized();
    }

    protected override VKError GetNotFoundError(VKDirectiveId id) => VKDirectiveErrors.NotFound;

    protected override VKError GetAlreadyExistsError(VKDirectiveId id) => VKDirectiveErrors.AlreadyExists;
}
