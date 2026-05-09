using System.Collections.Generic;
using System.IO;

namespace VK.Blocks.Storage;

public sealed record VKStorageUploadRequest(
    [StorageFile] string FileName,
    string ContentType,
    Stream Content,
    IDictionary<string, string>? Metadata = null,
    IDictionary<string, string>? IndexTags = null);
