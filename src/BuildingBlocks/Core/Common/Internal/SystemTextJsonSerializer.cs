using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VK.Blocks.Core.Common.Internal;

/// <summary>
/// A default implementation of <see cref="IVKJsonSerializer"/> using <see cref="System.Text.Json"/>.
/// </summary>
internal sealed class SystemTextJsonSerializer : IVKJsonSerializer
{
    private static readonly JsonSerializerOptions _defaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <inheritdoc />
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _defaultOptions);

    /// <inheritdoc />
    public byte[] SerializeToUtf8Bytes<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, _defaultOptions);

    /// <inheritdoc />
    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _defaultOptions);

    /// <inheritdoc />
    public T? Deserialize<T>(ReadOnlySpan<byte> utf8Json) => JsonSerializer.Deserialize<T>(utf8Json, _defaultOptions);
}
