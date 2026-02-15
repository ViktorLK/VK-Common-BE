using Microsoft.Azure.Cosmos;
using VK.Lab.CleanArchitecture.Models;

namespace VK.Lab.CleanArchitecture.Repositories
{
    /// <summary>
    /// CosmosDBリポジトリ実装
    /// </summary>
    public class CosmosRepository : ICosmosRepository
    {
        private readonly Container _container;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CosmosRepository(CosmosClient cosmosClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"]
                ?? throw new InvalidOperationException("CosmosDb:DatabaseName configuration is required");
            var containerName = configuration["CosmosDb:ContainerName"]
                ?? throw new InvalidOperationException("CosmosDb:ContainerName configuration is required");

            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        /// <summary>
        /// IDとパーティションキーでドキュメントを取得
        /// </summary>
        public async Task<CosmosDocument?> GetByIdAsync(string id, string partitionKey)
        {
            try
            {
                var response = await _container.ReadItemAsync<CosmosDocument>(
                    id,
                    new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <summary>
        /// パーティション内のすべてのドキュメントを取得
        /// </summary>
        public async Task<IEnumerable<CosmosDocument>> GetAllAsync(string partitionKey)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @partitionKey")
                .WithParameter("@partitionKey", partitionKey);

            var iterator = _container.GetItemQueryIterator<CosmosDocument>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });

            var results = new List<CosmosDocument>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        /// <summary>
        /// 新しいドキュメントを作成
        /// </summary>
        public async Task<CosmosDocument> CreateAsync(CosmosDocument document)
        {
            document.CreatedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;

            var response = await _container.CreateItemAsync(
                document,
                new PartitionKey(document.PartitionKey));

            return response.Resource;
        }

        /// <summary>
        /// 既存のドキュメントを更新
        /// </summary>
        public async Task<CosmosDocument> UpdateAsync(CosmosDocument document)
        {
            document.UpdatedAt = DateTime.UtcNow;

            var response = await _container.ReplaceItemAsync(
                document,
                document.Id,
                new PartitionKey(document.PartitionKey));

            return response.Resource;
        }

        /// <summary>
        /// ドキュメントを削除
        /// </summary>
        public async Task DeleteAsync(string id, string partitionKey)
        {
            await _container.DeleteItemAsync<CosmosDocument>(
                id,
                new PartitionKey(partitionKey));
        }

        /// <summary>
        /// ドキュメントが存在するか確認
        /// </summary>
        public async Task<bool> ExistsAsync(string id, string partitionKey)
        {
            var document = await GetByIdAsync(id, partitionKey);
            return document is not null;
        }
    }
}
