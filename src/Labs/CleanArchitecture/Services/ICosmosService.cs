using VK.Lab.CleanArchitecture.Models;

namespace VK.Lab.CleanArchitecture.Services
{
    /// <summary>
    /// CosmosDBサービスインターフェース
    /// </summary>
    public interface ICosmosService
    {
        /// <summary>
        /// IDとパーティションキーでドキュメントを取得
        /// </summary>
        Task<CosmosDocument?> GetByIdAsync(string id, string partitionKey);

        /// <summary>
        /// パーティション内のすべてのドキュメントを取得
        /// </summary>
        Task<IEnumerable<CosmosDocument>> GetAllAsync(string partitionKey);

        /// <summary>
        /// 新しいドキュメントを作成
        /// </summary>
        Task<CosmosDocument> CreateAsync(CosmosDocument document);

        /// <summary>
        /// 既存のドキュメントを更新
        /// </summary>
        Task<CosmosDocument> UpdateAsync(CosmosDocument document);

        /// <summary>
        /// ドキュメントを削除
        /// </summary>
        Task DeleteAsync(string id, string partitionKey);

        /// <summary>
        /// ドキュメントが存在するか確認
        /// </summary>
        Task<bool> ExistsAsync(string id, string partitionKey);
    }
}
