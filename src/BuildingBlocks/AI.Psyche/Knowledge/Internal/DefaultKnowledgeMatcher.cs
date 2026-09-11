using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// Provides unified, high-performance functional predicate composition and matching for <see cref="VKKnowledgeEntry"/> rules.
/// Avoids dynamic Expression compilation overhead and reflection in favor of Native AOT compatible closures.
/// Follows AP.01.
/// </summary>
internal static class DefaultKnowledgeMatcher
{
    private static readonly ConcurrentDictionary<VKKnowledgeId, (int StateHash, Func<string, bool> Matcher)> CompiledMatchers = new();

    /// <summary>
    /// Gets the matching delegate for the specified knowledge entry. Caches the compiled closure if not present,
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

        var matcher = BuildMatcher(entry);
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
            for (int i = 0; i < entry.Keys.Count; i++)
            {
                var key = entry.Keys[i];
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

    private static Func<string, bool> BuildMatcher(VKKnowledgeEntry entry)
    {
        if (entry.TriggerType == VKKnowledgeTriggerType.Constant)
        {
            return static _ => true;
        }

        if (entry.Keys is null || entry.Keys.Count == 0)
        {
            return static _ => false;
        }

        var predicates = new List<Func<string, bool>>(entry.Keys.Count);
        for (int i = 0; i < entry.Keys.Count; i++)
        {
            var key = entry.Keys[i];
            if (key is not null && !string.IsNullOrWhiteSpace(key.Text))
            {
                predicates.Add(CompileKeyPredicate(key));
            }
        }

        if (predicates.Count == 0)
        {
            return static _ => false;
        }

        var predicateArray = predicates.ToArray();

        return entry.FilterLogic switch
        {
            VKKnowledgeFilterLogic.AndAll => text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return false;
                }

                for (int i = 0; i < predicateArray.Length; i++)
                {
                    if (!predicateArray[i](text))
                    {
                        return false;
                    }
                }
                return true;
            },
            VKKnowledgeFilterLogic.NotAny => text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return false;
                }

                for (int i = 0; i < predicateArray.Length; i++)
                {
                    if (predicateArray[i](text))
                    {
                        return false;
                    }
                }
                return true;
            },
            VKKnowledgeFilterLogic.NotAll => text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return false;
                }

                for (int i = 0; i < predicateArray.Length; i++)
                {
                    if (!predicateArray[i](text))
                    {
                        return true;
                    }
                }
                return false;
            },
            VKKnowledgeFilterLogic.AndAny or _ => text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return false;
                }

                for (int i = 0; i < predicateArray.Length; i++)
                {
                    if (predicateArray[i](text))
                    {
                        return true;
                    }
                }
                return false;
            }
        };
    }

    private static Func<string, bool> CompileKeyPredicate(VKKnowledgeKey key)
    {
        if (string.IsNullOrWhiteSpace(key.Text))
        {
            return static _ => false;
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
                var regex = new Regex(pattern, options, TimeSpan.FromMilliseconds(100));
                return text => text is not null && regex.IsMatch(text);
            }
            catch
            {
                return static _ => false;
            }
        }

        // CJK characters do not have space-delimited word boundaries (\b).
        // Non-CJK WholeWord uses \b boundaries; CJK WholeWord falls through to substring Contains.
        if (key.MatchType == VKKnowledgeMatchType.WholeWord && !ContainsCjk(key.Text))
        {
            var pattern = $@"\b{Regex.Escape(key.Text)}\b";
            var options = key.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            try
            {
                var regex = new Regex(pattern, options, TimeSpan.FromMilliseconds(100));
                return text => text is not null && regex.IsMatch(text);
            }
            catch
            {
                return static _ => false;
            }
        }

        // Substring Contains (also serves as the fallback for CJK WholeWord)
        var comparison = key.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var keyText = key.Text;

        return text => text is not null && text.Contains(keyText, comparison);
    }

    private static bool ContainsCjk(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            if ((ch >= 0x4E00 && ch <= 0x9FFF) || // CJK Unified Ideographs (Common Chinese & Japanese Kanji)
                (ch >= 0x3400 && ch <= 0x4DBF) || // CJK Extension A
                (ch >= 0x3040 && ch <= 0x309F) || // Hiragana
                (ch >= 0x30A0 && ch <= 0x30FF) || // Katakana
                (ch >= 0x3000 && ch <= 0x303F) || // CJK Symbols and Punctuation
                (ch >= 0xF900 && ch <= 0xFAFF) || // CJK Compatibility Ideographs
                (ch >= 0xAC00 && ch <= 0xD7AF))   // Hangul Syllables
            {
                return true;
            }
        }
        return false;
    }
}
