using HotChocolate.Data;
using Microsoft.AspNetCore.Authorization;
using VK_Common_BE.Models;
using VK_Common_BE.Services;

namespace VK_Common_BE.GraphQL
{
    /// <summary>
    /// GraphQL クエリ定義
    /// </summary>
    public class Query
    {
        /// <summary>
        /// すべての製品を取得（フィルタリング・ソート対応）
        /// </summary>
        /// <param name="productService">製品サービス</param>
        /// <returns>製品クエリ</returns>
        [AllowAnonymous]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Product> GetProducts([Service] IProductService productService)
        {
            return productService.GetProductsQueryable();
        }

        /// <summary>
        /// IDで単一の製品を取得
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <param name="productService">製品サービス</param>
        /// <returns>製品詳細</returns>
        [Authorize]
        public async Task<Product?> GetProduct(int id, [Service] IProductService productService)
        {
            return await productService.GetProductByIdAsync(id);
        }
    }
}
