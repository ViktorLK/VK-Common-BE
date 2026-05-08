using System;
using System.IO;
using System.Threading.Tasks;
namespace VK.Blocks.Storage;

public sealed record VKStorageDownloadResponse(
    Stream Content,
    string ContentType,
    string FileName) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        if (Content != null)
        {
            await Content.DisposeAsync();
        }
    }
}
