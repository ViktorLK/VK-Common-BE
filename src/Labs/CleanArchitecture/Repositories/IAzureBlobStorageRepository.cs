using Azure.Storage.Blobs.Models;

namespace VK.Lab.CleanArchitecture.Repositories
{
    /// <summary>
    /// Azure Blob Storage リポジトリインターフェース
    /// </summary>
    public interface IAzureBlobStorageRepository
    {
        /// <summary>
        /// 指定されたBlobのSAS URLを生成する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <param name="expiresInMinutes">有効期限（分）</param>
        /// <returns>SAS URL</returns>
        Task<string> GenerateBlobSasUrlAsync(string containerName, string blobName, int expiresInMinutes = 60);

        /// <summary>
        /// 指定されたコンテナのSAS URLを生成する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="expiresInMinutes">有効期限（分）</param>
        /// <returns>SAS URL</returns>
        Task<string> GenerateContainerSasUrlAsync(string containerName, int expiresInMinutes = 60);

        /// <summary>
        /// 書き込み権限を持つBlobのSAS URLを生成する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <param name="expiresInMinutes">有効期限（分）</param>
        /// <returns>SAS URL</returns>
        Task<string> GenerateWriteBlobSasUrlAsync(string containerName, string blobName, int expiresInMinutes = 60);

        /// <summary>
        /// Blobをソフトデリートする
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        Task SoftDeleteBlobAsync(string containerName, string blobName);

        /// <summary>
        /// Blobにリースを取得する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <param name="leaseDurationSeconds">リース期間（秒）</param>
        /// <returns>リースID</returns>
        Task<string> AcquireBlobLeaseAsync(string containerName, string blobName, int leaseDurationSeconds = 60);

        /// <summary>
        /// Blobのリースを解放する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <param name="leaseId">リースID</param>
        Task ReleaseBlobLeaseAsync(string containerName, string blobName, string leaseId);

        /// <summary>
        /// Blobのインデックスタグを読み取る
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <returns>タグのディクショナリ</returns>
        Task<IDictionary<string, string>> GetBlobIndexTagsAsync(string containerName, string blobName);

        /// <summary>
        /// Blobにインデックスタグを書き込む
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <param name="tags">設定するタグ</param>
        Task SetBlobIndexTagsAsync(string containerName, string blobName, IDictionary<string, string> tags);

        /// <summary>
        /// 指定されたプレフィックスの下にあるすべてのBlobを非同期ストリームとして取得する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="prefix">プレフィックス</param>
        /// <returns>Blobアイテムの非同期列挙</returns>
        IAsyncEnumerable<BlobItem> GetBlobsByPrefixAsync(string containerName, string prefix);

        /// <summary>
        /// Blobのバージョン履歴を取得する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <returns>Blobバージョンの非同期列挙</returns>
        IAsyncEnumerable<BlobItem> GetBlobVersionHistoryAsync(string containerName, string blobName);

        /// <summary>
        /// 特定のバージョンのBlobに対するSAS URLを生成する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <param name="versionId">バージョンID</param>
        /// <param name="expiresInMinutes">有効期限（分）</param>
        /// <returns>SAS URL</returns>
        Task<string> GenerateVersionedBlobSasUrlAsync(
            string containerName,
            string blobName,
            string versionId,
            int expiresInMinutes = 60);
    }
}
