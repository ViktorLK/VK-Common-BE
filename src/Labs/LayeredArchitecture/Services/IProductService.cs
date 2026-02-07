using VK.Lab.LayeredArchitecture.Models;

namespace VK.Lab.LayeredArchitecture.Services
{
    /// <summary>
    /// 製品サービスインターフェース
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// すべての製品を取征E
        /// </summary>
        Task<IEnumerable<Product>> GetAllProductsAsync();

        /// <summary>
        /// IDで製品を取征E
        /// </summary>
        Task<Product?> GetProductByIdAsync(int id);

        /// <summary>
        /// 製品を作�E
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
        /// 製品が存在するか確誁E
        /// </summary>
        Task<bool> ProductExistsAsync(int id);
    }
}
