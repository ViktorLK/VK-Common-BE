using System;
using System.Collections.Generic;
namespace VK.Blocks.Storage;

public sealed record VKStorageEntry(
    string Name,
    string Path,
    bool IsDirectory,
    long? Size = null,
    string? ContentType = null,
    DateTimeOffset? CreatedOn = null,
    IDictionary<string, string>? Metadata = null);
