using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace VK.Lab.CleanArchitecture.Repositories
{
    /// <summary>
    /// Azure Blob Storage リポジトリ実装
    /// </summary>
    public class AzureBlobStorageRepository : IAzureBlobStorageRepository
    {
        private readonly BlobServiceClient blobServiceClient;

        public AzureBlobStorageRepository(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }

        #region Private Helper Methods

        /// <summary>
        /// BlobClientを取得し、存在確認を行う
        /// </summary>
        private async Task<BlobClient> GetAndValidateBlobClientAsync(string containerName, string blobName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'");
            }

            return blobClient;
        }

        /// <summary>
        /// BlobContainerClientを取得し、存在確認を行う
        /// </summary>
        private async Task<BlobContainerClient> GetAndValidateContainerClientAsync(string containerName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            if (!await containerClient.ExistsAsync())
            {
                throw new DirectoryNotFoundException($"Container '{containerName}' not found");
            }

            return containerClient;
        }

        /// <summary>
        /// SAS Builderからトークン文字列を生成する
        /// </summary>
        private async Task<string> GenerateSasTokenAsync(BlobSasBuilder sasBuilder)
        {
            var userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
                sasBuilder.StartsOn,
                sasBuilder.ExpiresOn);

            return sasBuilder.ToSasQueryParameters(userDelegationKey.Value, blobServiceClient.AccountName).ToString();
        }

        #endregion

        /// <summary>
        /// 指定されたBlobのSAS URLを生成する
        /// </summary>
        public async Task<string> GenerateBlobSasUrlAsync(string containerName, string blobName, int expiresInMinutes = 60)
        {
            var blobClient = await GetAndValidateBlobClientAsync(containerName, blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = await GenerateSasTokenAsync(sasBuilder);
            return $"{blobClient.Uri}?{sasToken}";
        }

        /// <summary>
        /// 指定されたコンテナのSAS URLを生成する
        /// </summary>
        public async Task<string> GenerateContainerSasUrlAsync(string containerName, int expiresInMinutes = 60)
        {
            var containerClient = await GetAndValidateContainerClientAsync(containerName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "c",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List);

            var sasToken = await GenerateSasTokenAsync(sasBuilder);
            return $"{containerClient.Uri}?{sasToken}";
        }

        /// <summary>
        /// 書き込み権限を持つBlobのSAS URLを生成する
        /// </summary>
        public async Task<string> GenerateWriteBlobSasUrlAsync(string containerName, string blobName, int expiresInMinutes = 60)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create | BlobSasPermissions.Read);

            var sasToken = await GenerateSasTokenAsync(sasBuilder);
            return $"{blobClient.Uri}?{sasToken}";
        }

        /// <summary>
        /// Blobをソフトデリートする
        /// </summary>
        public async Task SoftDeleteBlobAsync(string containerName, string blobName)
        {
            var blobClient = await GetAndValidateBlobClientAsync(containerName, blobName);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        /// <summary>
        /// Blobにリースを取得する
        /// </summary>
        /// <returns>リースID</returns>
        public async Task<string> AcquireBlobLeaseAsync(string containerName, string blobName, int leaseDurationSeconds = 60)
        {
            var blobClient = await GetAndValidateBlobClientAsync(containerName, blobName);
            var leaseClient = blobClient.GetBlobLeaseClient();

            var lease = await leaseClient.AcquireAsync(TimeSpan.FromSeconds(leaseDurationSeconds));
            return lease.Value.LeaseId;
        }

        /// <summary>
        /// Blobのリースを解放する
        /// </summary>
        public async Task ReleaseBlobLeaseAsync(string containerName, string blobName, string leaseId)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            var leaseClient = blobClient.GetBlobLeaseClient(leaseId);

            // リースを解放
            await leaseClient.ReleaseAsync();
        }

        /// <summary>
        /// Blobのインデックスタグを読み取る
        /// </summary>
        public async Task<IDictionary<string, string>> GetBlobIndexTagsAsync(string containerName, string blobName)
        {
            var blobClient = await GetAndValidateBlobClientAsync(containerName, blobName);
            var tags = await blobClient.GetTagsAsync();
            return tags.Value.Tags;
        }

        /// <summary>
        /// Blobにインデックスタグを書き込む
        /// </summary>
        public async Task SetBlobIndexTagsAsync(string containerName, string blobName, IDictionary<string, string> tags)
        {
            var blobClient = await GetAndValidateBlobClientAsync(containerName, blobName);
            await blobClient.SetTagsAsync(tags);
        }

        /// <summary>
        /// 指定されたプレフィックスの下にあるすべてのBlobを非同期ストリームとして取得する
        /// </summary>
        public async IAsyncEnumerable<BlobItem> GetBlobsByPrefixAsync(string containerName, string prefix)
        {
            var containerClient = await GetAndValidateContainerClientAsync(containerName);

            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                yield return blobItem;
            }
        }

        /// <summary>
        /// Blobのバージョン履歴を取得する
        /// </summary>
        public async IAsyncEnumerable<BlobItem> GetBlobVersionHistoryAsync(string containerName, string blobName)
        {
            var containerClient = await GetAndValidateContainerClientAsync(containerName);

            await foreach (var blobItem in containerClient.GetBlobsAsync(
                prefix: blobName,
                states: BlobStates.Version))
            {
                if (blobItem.Name == blobName)
                {
                    yield return blobItem;
                }
            }
        }

        /// <summary>
        /// 特定のバージョンのBlobに対するSAS URLを生成する
        /// </summary>
        public async Task<string> GenerateVersionedBlobSasUrlAsync(
            string containerName,
            string blobName,
            string versionId,
            int expiresInMinutes = 60)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            var versionedBlobClient = blobClient.WithVersion(versionId);

            if (!await versionedBlobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Blob version '{versionId}' not found for blob '{blobName}' in container '{containerName}'");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                BlobVersionId = versionId,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = await GenerateSasTokenAsync(sasBuilder);
            return $"{versionedBlobClient.Uri}?{sasToken}";
        }
    }
}
