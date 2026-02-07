using VK.Lab.LayeredArchitecture.Models;

namespace VK.Lab.LayeredArchitecture.Repositories
{
    /// <summary>
    /// 製品リポジトリインターフェース
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// すべての製品を取征E
        /// </summary>
        Task<IEnumerable<Product>> GetAllAsync();

        /// <summary>
        /// IDで製品を取征E
        /// </summary>
        Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// 製品を追加
        /// </summary>
        Task<Product> AddAsync(Product product);

        /// <summary>
        /// 製品を更新
        /// </summary>
        Task UpdateAsync(Product product);

        /// <summary>
        /// 製品を削除
        /// </summary>
        Task DeleteAsync(Product product);

        /// <summary>
        /// 製品が存在するか確誁E
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// 変更を保孁E
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
