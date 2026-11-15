using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Privacy.Internal;

/// <summary>
/// Lightweight regex-based implementation of IVKPrivacyFilter for detecting and masking PII.
/// </summary>
internal sealed partial class DefaultPrivacyFilter : IVKPrivacyFilter
{
    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b(?:\+\d{1,3}[- ]?)?\(?\d{3}\)?[- ]?\d{3}[- ]?\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\b(?:\d{4}[ -]?){3}\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex CreditCardRegex();

    public Task<VKResult<VKPrivacyMaskingResult>> MaskAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult(VKResult.Success(new VKPrivacyMaskingResult(text ?? string.Empty, [])));
        }

        var entities = new List<VKPIIEntity>();
        var maskedText = text;

        maskedText = ApplyMask(maskedText, EmailRegex(), "Email", "[EMAIL_MASKED]", entities);
        maskedText = ApplyMask(maskedText, PhoneRegex(), "Phone", "[PHONE_MASKED]", entities);
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

