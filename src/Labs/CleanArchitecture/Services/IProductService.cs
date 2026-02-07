using VK.Lab.CleanArchitecture.Models;

namespace VK.Lab.CleanArchitecture.Services
{
    /// <summary>
    /// 製品サービスインターフェース
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// すべての製品を取得
        /// </summary>
        Task<IEnumerable<Product>> GetAllProductsAsync();

        /// <summary>
        /// クエリ可能な製品セットを取得（GraphQL用）
        /// </summary>
        IQueryable<Product> GetProductsQueryable();

        /// <summary>
        /// IDで製品を取得
        /// </summary>
        Task<Product?> GetProductByIdAsync(int id);

        /// <summary>
        /// 製品を作成
        /// </summary>
        Task<Product> CreateProductAsync(Product product);

        /// <summary>
        /// 製品を更新
        /// </summary>
        Task<bool> UpdateProductAsync(int id, Product product);

        /// <summary>
        /// 製品を削除
        /// </summary>
        Task<bool> DeleteProductAsync(int id);

        /// <summary>
        /// 製品が存在するか確認
        /// </summary>
        Task<bool> ProductExistsAsync(int id);
    }
}
