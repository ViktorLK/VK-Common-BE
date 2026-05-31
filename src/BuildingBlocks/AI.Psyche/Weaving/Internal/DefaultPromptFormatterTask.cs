using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultPromptFormatterTask : IVKWeavingTask
{
    private readonly IEnumerable<IVKPromptFormatter> _formatters;

    public DefaultPromptFormatterTask(IEnumerable<IVKPromptFormatter> formatters)
    {
        _formatters = VKGuard.NotNull(formatters);
    }

    public int TaskOrder => 300;
    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var formattedFragments = new List<VKPromptFragment>();

        foreach (var fragment in context.Fragments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 1. Check if we have an IVKPromptFormatter for this fragment's type
            var matchedFormatter = _formatters.FirstOrDefault(f => f.CanFormat(fragment));
            if (matchedFormatter is null)
            {
                if (!string.IsNullOrEmpty(fragment.Content))
                {
                    formattedFragments.Add(fragment);
                    continue;
                }
                return Task.FromResult(VKResult.Failure(VKWeavingErrors.FormatterNotFound));
            }

            var formatResult = matchedFormatter.Format(fragment, context);
            if (formatResult.IsFailure)
            {
                return Task.FromResult(VKResult.Failure(formatResult.FirstError));
            }

            // [AP.01] Modern C# record state mutation via 'with' expression
            formattedFragments.Add(fragment with
            {
                Content = formatResult.Value
            });
        }

        context.SetFragments(formattedFragments);
        return Task.FromResult(VKResult.Success());
    }
}
