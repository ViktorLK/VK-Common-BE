using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// Provides unified, high-performance expression tree compilation and matching for <see cref="VKKnowledgeEntry"/> rules.
/// </summary>
internal static class DefaultKnowledgeMatcher
{
    private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod(
        nameof(string.Contains),
        [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo RegexIsMatchMethod = typeof(Regex).GetMethod(
        nameof(Regex.IsMatch),
        [typeof(string), typeof(string), typeof(RegexOptions), typeof(TimeSpan)])!;

    // TODO: Transition to Scheme A (Event-driven cache invalidation) once a unified Application/Domain Event bus is introduced.
    // This will allow subscribing to KnowledgeUpdated/Deleted events to invalidate matchers instead of state hash check.
    private static readonly ConcurrentDictionary<VKKnowledgeId, (int StateHash, Func<string, bool> Matcher)> CompiledMatchers = new();

    /// <summary>
    /// Gets the matching delegate for the specified knowledge entry. Compiles and caches it if not present,
    /// or if the entry's configuration has changed.
    /// </summary>
    public static Func<string, bool> GetMatcher(VKKnowledgeEntry entry)
    {
        VKGuard.NotNull(entry);

        var currentHash = GetEntryStateHash(entry);

        if (CompiledMatchers.TryGetValue(entry.Id, out var cached) && cached.StateHash == currentHash)
        {
            return cached.Matcher;
        }

        var matcher = CompileMatcher(entry);
        CompiledMatchers[entry.Id] = (currentHash, matcher);
        return matcher;
    }

    /// <summary>
    /// Forcibly clears the cached matcher for the specified entry ID (used during Upsert/Delete).
    /// </summary>
    public static void Invalidate(VKKnowledgeId entryId)
    {
        if (!entryId.IsEmpty)
        {
            CompiledMatchers.TryRemove(entryId, out _);
        }
    }

    private static int GetEntryStateHash(VKKnowledgeEntry entry)
    {
        var hashCode = new HashCode();
        hashCode.Add(entry.TriggerType);
        hashCode.Add(entry.FilterLogic);
        if (entry.Keys is not null)
        {
            foreach (var key in entry.Keys)
            {
                if (key is not null)
                {
                    hashCode.Add(key.Text);
                    hashCode.Add(key.MatchType);
                    hashCode.Add(key.CaseSensitive);
                }
            }
        }
        return hashCode.ToHashCode();
    }

    private static Func<string, bool> CompileMatcher(VKKnowledgeEntry entry)
    {
        if (entry.TriggerType == VKKnowledgeTriggerType.Constant)
        {
            // Constant entries are always active and ignore keys
            return static _ => true;
        }

        var keys = entry.Keys.ToList();

        if (keys.Count == 0)
        {
            return static _ => false;
        }

        try
        {
            ParameterExpression contextParam = Expression.Parameter(typeof(string), "context");

            // Build keys logic combined based on FilterLogic
            Expression primaryExpression = BuildCombinationExpression(contextParam, keys, entry.FilterLogic);

            var lambda = Expression.Lambda<Func<string, bool>>(primaryExpression, contextParam);
            return lambda.Compile();
        }
        catch
        {
            // Safe fallback if compilation fails
            return static _ => false;
        }
    }

    private static Expression BuildCombinationExpression(
        ParameterExpression contextParam,
        List<VKKnowledgeKey> keys,
        VKKnowledgeFilterLogic logic)
    {
        Expression BuildAndChain()
        {
            Expression current = BuildSingleKeyExpression(contextParam, keys[0]);
            for (int i = 1; i < keys.Count; i++)
            {
                current = Expression.AndAlso(current, BuildSingleKeyExpression(contextParam, keys[i]));
            }
            return current;
        }

        Expression BuildOrChain()
        {
            Expression current = BuildSingleKeyExpression(contextParam, keys[0]);
            for (int i = 1; i < keys.Count; i++)
            {
                current = Expression.OrElse(current, BuildSingleKeyExpression(contextParam, keys[i]));
            }
            return current;
        }

        return logic switch
        {
            VKKnowledgeFilterLogic.AndAll => BuildAndChain(),
            VKKnowledgeFilterLogic.NotAny => Expression.Not(BuildOrChain()),
            VKKnowledgeFilterLogic.NotAll => Expression.Not(BuildAndChain()),
            VKKnowledgeFilterLogic.AndAny or _ => BuildOrChain()
        };
    }

    private static Expression BuildSingleKeyExpression(ParameterExpression contextParam, VKKnowledgeKey key)
    {
        if (string.IsNullOrWhiteSpace(key.Text))
        {
            return Expression.Constant(false);
        }

        if (key.MatchType == VKKnowledgeMatchType.Regex)
        {
            var pattern = key.Text;
            var options = key.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (pattern.StartsWith('/') && pattern.LastIndexOf('/') > 0)
            {
                var lastSlash = pattern.LastIndexOf('/');
                var flags = pattern.Substring(lastSlash + 1);
                pattern = pattern.Substring(1, lastSlash - 1);

                options = RegexOptions.None;
                if (flags.Contains('i'))
                {
                    options |= RegexOptions.IgnoreCase;
                }
                if (flags.Contains('m'))
                {
                    options |= RegexOptions.Multiline;
                }
                if (flags.Contains('s'))
                {
                    options |= RegexOptions.Singleline;
                }
            }

            try
            {
                // Validate regex pattern compatibility during compilation time
                _ = new Regex(pattern, options, TimeSpan.FromMilliseconds(100));
            }
            catch
            {
                // If regex pattern is malformed, bypass it by compiling as constant false
                return Expression.Constant(false);
            }

            return Expression.Call(
                RegexIsMatchMethod,
                contextParam,
                Expression.Constant(pattern),
                Expression.Constant(options),
                Expression.Constant(TimeSpan.FromMilliseconds(100))
            );
        }

        if (key.MatchType == VKKnowledgeMatchType.WholeWord)
        {
            var pattern = $@"\b{Regex.Escape(key.Text)}\b";
            var options = key.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            try
            {
                _ = new Regex(pattern, options, TimeSpan.FromMilliseconds(100));
            }
            catch
            {
                return Expression.Constant(false);
            }

            return Expression.Call(
                RegexIsMatchMethod,
                contextParam,
                Expression.Constant(pattern),
                Expression.Constant(options),
                Expression.Constant(TimeSpan.FromMilliseconds(100))
            );
        }

        var comparison = key.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        return Expression.Call(
            contextParam,
            StringContainsMethod,
            Expression.Constant(key.Text),
            Expression.Constant(comparison)
        );
    }
}
