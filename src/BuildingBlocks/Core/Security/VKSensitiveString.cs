using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VK.Blocks.Core;

/// <summary>
/// A wrapper for sensitive string data (e.g., API keys, secrets) that prevents 
/// accidental leakage in logs and telemetry by overriding <see cref="ToString"/>.
/// [OR.02] PII masked in logs.
/// </summary>
[JsonConverter(typeof(VKSensitiveStringJsonConverter))]
[TypeConverter(typeof(VKSensitiveStringTypeConverter))]
public readonly record struct VKSensitiveString
{
    private readonly string _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKSensitiveString"/> struct.
    /// </summary>
    /// <param name="value">The sensitive value.</param>
    public VKSensitiveString(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets a value indicating whether the string is null or empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    /// <summary>
    /// Returns a masked representation of the sensitive value.
    /// [OR.02]
    /// </summary>
    /// <returns>A string of asterisks.</returns>
    public override string ToString() => "********";

    /// <summary>
    /// Reveals the underlying sensitive value. 
    /// Use this only at the final boundary where the value is required (e.g., SDK call).
    /// </summary>
    /// <returns>The raw sensitive string.</returns>
    public string Reveal() => _value ?? string.Empty;

    /// <summary>
    /// Performs an implicit conversion from <see cref="string"/> to <see cref="VKSensitiveString"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    public static implicit operator VKSensitiveString(string value) => new(value);

    /// <summary>
    /// Creates a <see cref="VKSensitiveString"/> from a string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="VKSensitiveString"/>.</returns>
    public static VKSensitiveString From(string value) => new(value);

    private sealed class VKSensitiveStringJsonConverter : JsonConverter<VKSensitiveString>
    {
        public override VKSensitiveString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new VKSensitiveString(reader.GetString() ?? string.Empty);
        }

        public override void Write(Utf8JsonWriter writer, VKSensitiveString value, JsonSerializerOptions options)
        {
            // [OR.02] Always write mask to JSON
            writer.WriteStringValue(value.ToString());
        }
    }

    private sealed class VKSensitiveStringTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {
                return new VKSensitiveString(s);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
