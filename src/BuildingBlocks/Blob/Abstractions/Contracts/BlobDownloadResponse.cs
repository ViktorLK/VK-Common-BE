namespace VK.Blocks.Blob.Abstractions.Contracts;

public sealed record BlobDownloadResponse(
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
