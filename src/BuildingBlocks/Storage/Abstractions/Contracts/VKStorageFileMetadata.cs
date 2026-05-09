using System;
using System.Collections.Generic;
namespace VK.Blocks.Storage;

public sealed record VKStorageFileMetadata(
    string Name,
    long Size,
    string ContentType,
    DateTimeOffset CreatedAt,
    IDictionary<string, string>? Metadata = null);
