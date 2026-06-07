using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Filters.Internal;

/// <summary>
/// A lightweight regex-based implementation of <see cref="IVKPrivacyFilter"/>
/// for detecting and masking PII without external dependencies.
/// </summary>
internal sealed partial class RegexPrivacyFilter : IVKPrivacyFilter
{
    // C# 11+ source generated regexes for better performance
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b(?:\+\d{1,3}[- ]?)?\(?\d{3}\)?[- ]?\d{3}[- ]?\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\b(?:\d{4}[ -]?){3}\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex CreditCardRegex();
#else
    private static Regex EmailRegex() => new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
    private static Regex PhoneRegex() => new(@"\b(?:\+\d{1,3}[- ]?)?\(?\d{3}\)?[- ]?\d{3}[- ]?\d{4}\b", RegexOptions.Compiled);
    private static Regex CreditCardRegex() => new(@"\b(?:\d{4}[ -]?){3}\d{4}\b", RegexOptions.Compiled);
#endif

    /// <inheritdoc />
    public Task<VKResult<VKPrivacyMaskingResult>> MaskAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult(VKResult.Success(new VKPrivacyMaskingResult(text ?? string.Empty, [])));
        }

        var entities = new List<VKPIIEntity>();
        var maskedText = text;

        // Apply Email Masking
        maskedText = ApplyMask(maskedText, EmailRegex(), "Email", "[EMAIL_MASKED]", entities);

        // Apply Phone Masking
        maskedText = ApplyMask(maskedText, PhoneRegex(), "Phone", "[PHONE_MASKED]", entities);

        // Apply Credit Card Masking
        maskedText = ApplyMask(maskedText, CreditCardRegex(), "CreditCard", "[CREDITCARD_MASKED]", entities);

        return Task.FromResult(VKResult.Success(new VKPrivacyMaskingResult(maskedText, entities)));
    }

    private static string ApplyMask(string input, Regex regex, string entityType, string maskToken, List<VKPIIEntity> entities)
    {
        return regex.Replace(input, match =>
        {
            entities.Add(new VKPIIEntity(entityType, match.Value, match.Index, match.Index + match.Length));
            return maskToken;
        });
    }
}
