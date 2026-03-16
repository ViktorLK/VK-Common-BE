using System.Text.RegularExpressions;
using VK.Blocks.Observability.AspNetCore.Options;

namespace VK.Blocks.Observability.AspNetCore.Logging;

/// <summary>
/// JSON ボディや文字列内の機密フィールドをマスキング (<c>&lt;redacted&gt;</c>) に置換する。
/// <para>
/// JSON のキー/値ペアに対する正規表現置換を使用するため、
/// 完全な JSON パーサーを必要とせずに機密データを脱敏できる。
/// </para>
/// </summary>
public sealed class SensitiveDataRedactor
{
    private const string RedactedValue = "<redacted>";
    private readonly IReadOnlyList<string> _sensitiveFields;
    private readonly Regex _redactPattern;

    /// <summary>
    /// <see cref="RequestLoggingOptions.SensitiveFields"/> を元に脱敏パターンを構築する。
    /// </summary>
    public SensitiveDataRedactor(RequestLoggingOptions options)
        : this(options.SensitiveFields) { }

    /// <summary>
    /// 脱敏対象フィールド名リストを明示的に指定して初期化する。
    /// </summary>
    public SensitiveDataRedactor(IReadOnlyList<string> sensitiveFields)
    {
        _sensitiveFields = sensitiveFields
            ?? throw new ArgumentNullException(nameof(sensitiveFields));

        _redactPattern = BuildPattern(sensitiveFields);
    }

    /// <summary>
    /// 入力文字列内の機密フィールドの値を <c>&lt;redacted&gt;</c> に置換する。
    /// </summary>
    /// <param name="input">置換対象の文字列 (JSON ボディなど)。</param>
    /// <returns>
    /// 機密フィールドがマスクされた文字列。
    /// <paramref name="input"/> が <see langword="null"/> または空の場合はそのまま返す。
    /// </returns>
    public string Redact(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        if (_sensitiveFields.Count == 0)
            return input;

        return _redactPattern.Replace(input, $"$1\"{RedactedValue}\"");
    }

    // -------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------

    private static Regex BuildPattern(IReadOnlyList<string> fields)
    {
        if (fields.Count == 0)
            return new Regex(string.Empty);

        // JSON キー/値パターン: "fieldName"\s*:\s*value
        // 文字列 ("..."), 数値 (-123.45), boolean (true/false), null に対応
        var escapedFields = fields.Select(Regex.Escape);
        var joined = string.Join("|", escapedFields);

        // Capture group 1: key portion up to the colon
        // Match string, numbers (including negative/decimal), true, false, or null
        var pattern =
            $@"(""(?:{joined})""\s*:\s*)(?:""[^""]*""|-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?|true|false|null)";

        return new Regex(
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));
    }
}
