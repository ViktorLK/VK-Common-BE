using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Provides unified, high-performance expression tree compilation and matching for <see cref="VKKnowledgeEntry"/> rules.
/// </summary>
public static class VKKnowledgeMatcher
{
    private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod(
        nameof(string.Contains),
        [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo RegexIsMatchMethod = typeof(Regex).GetMethod(
        nameof(Regex.IsMatch),
        [typeof(string), typeof(string), typeof(RegexOptions), typeof(TimeSpan)])!;

    private static readonly ConcurrentDictionary<string, Func<string, bool>> _compiledMatchers = new();

    /// <summary>
    /// Gets the matching delegate for the specified knowledge entry. Compiles and caches it if not present.
    /// </summary>
    public static Func<string, bool> GetMatcher(VKKnowledgeEntry entry)
    {
        VKGuard.NotNull(entry);
        return _compiledMatchers.GetOrAdd(entry.Id, _ => CompileMatcher(entry));
    }

    /// <summary>
    /// Forcibly clears the cached matcher for the specified entry ID (used during Upsert/Delete).
    /// </summary>
    public static void Invalidate(string entryId)
    {
        if (!string.IsNullOrWhiteSpace(entryId))
        {
            _compiledMatchers.TryRemove(entryId, out _);
        }
    }

    private static Func<string, bool> CompileMatcher(VKKnowledgeEntry entry)
    {
        var primaryKeys = entry.Keys.Where(k => !k.IsFilter).ToList();
        var filterKeys = entry.Keys.Where(k => k.IsFilter).ToList();

        if (primaryKeys.Count == 0)
        {
            return static _ => false;
        }

        try
        {
            ParameterExpression contextParam = Expression.Parameter(typeof(string), "context");

            // Build primary keys logic (Any match, so combined with OrElse)
            Expression primaryExpression = BuildOrChain(contextParam, primaryKeys);

            // Build filter keys logic (All filter logic groups must pass, so combined with AndAlso)
            if (filterKeys.Count > 0)
            {
                var combinedExpression = primaryExpression;

                foreach (var logicGroup in filterKeys.GroupBy(k => k.Logic))
                {
                    Expression groupExpression = BuildGroupExpression(contextParam, logicGroup.Key, logicGroup.ToList());
                    combinedExpression = Expression.AndAlso(combinedExpression, groupExpression);
                }

                primaryExpression = combinedExpression;
            }

            var lambda = Expression.Lambda<Func<string, bool>>(primaryExpression, contextParam);
            return lambda.Compile();
        }
        catch
        {
            // Safe fallback if compilation fails
            return static _ => false;
        }
    }

    private static Expression BuildOrChain(ParameterExpression contextParam, List<VKKnowledgeKey> keys)
    {
        Expression current = BuildSingleKeyExpression(contextParam, keys[0]);
        for (int i = 1; i < keys.Count; i++)
        {
            current = Expression.OrElse(current, BuildSingleKeyExpression(contextParam, keys[i]));
        }
        return current;
    }

    private static Expression BuildAndChain(ParameterExpression contextParam, List<VKKnowledgeKey> keys)
    {
        Expression current = BuildSingleKeyExpression(contextParam, keys[0]);
        for (int i = 1; i < keys.Count; i++)
        {
            current = Expression.AndAlso(current, BuildSingleKeyExpression(contextParam, keys[i]));
        }
        return current;
    }

    private static Expression BuildGroupExpression(
        ParameterExpression contextParam, 
        VKKnowledgeFilterLogic logic, 
        List<VKKnowledgeKey> keys)
    {
        if (keys.Count == 0)
        {
            return Expression.Constant(true);
        }

        return logic switch
        {
            VKKnowledgeFilterLogic.AndAny => BuildOrChain(contextParam, keys),
            VKKnowledgeFilterLogic.AndAll => BuildAndChain(contextParam, keys),
            VKKnowledgeFilterLogic.NotAny => Expression.Not(BuildOrChain(contextParam, keys)),
            VKKnowledgeFilterLogic.NotAll => Expression.Not(BuildAndChain(contextParam, keys)),
            _ => Expression.Constant(true)
        };
    }

    private static Expression BuildSingleKeyExpression(ParameterExpression contextParam, VKKnowledgeKey key)
    {
        if (string.IsNullOrWhiteSpace(key.Text))
        {
            return Expression.Constant(false);
        }

        if (key.IsRegex)
        {
            var pattern = key.Text;
            var options = RegexOptions.IgnoreCase;

            // SillyTavern style regex /pattern/flags
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

        return Expression.Call(
            contextParam,
            StringContainsMethod,
            Expression.Constant(key.Text),
            Expression.Constant(StringComparison.OrdinalIgnoreCase)
        );
    }
}
