using HotChocolate.Authorization;
using VK.Lab.CleanArchitecture.Constants;
using VK.Lab.CleanArchitecture.GraphQL.Types;
using VK.Lab.CleanArchitecture.Models;
using VK.Lab.CleanArchitecture.Services;

namespace VK.Lab.CleanArchitecture.GraphQL
{
    /// <summary>
    /// GraphQL ミューテーション定義
    /// </summary>
    public class Mutation
    {
        /// <summary>
        /// 新しい製品を作成
        /// </summary>
        /// <param name="input">製品入力データ</param>
        /// <param name="productService">製品サービス</param>
        /// <returns>作成された製品</returns>
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]
        public async Task<Product> CreateProduct(
            ProductInput input,
            [Service] IProductService productService)
        {
            var product = new Product
            {
                Name = input.Name,
                Price = input.Price,
                CreatedAt = DateTime.UtcNow
            };

            return await productService.CreateProductAsync(product);
        }

        /// <summary>
        /// 製品情報を更新
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <param name="input">更新データ</param>
        /// <param name="productService">製品サービス</param>
        /// <returns>更新された製品</returns>
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]
        public async Task<Product?> UpdateProduct(
            int id,
            ProductInput input,
            [Service] IProductService productService)
        {
            var existingProduct = await productService.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                throw new GraphQLException($"Product with ID {id} not found.");
            }

            existingProduct.Name = input.Name;
            existingProduct.Price = input.Price;

            var success = await productService.UpdateProductAsync(id, existingProduct);
            if (!success)
            {
                throw new GraphQLException($"Failed to update product with ID {id}.");
            }

            return existingProduct;
        }

        /// <summary>
        /// 製品を削除
        /// </summary>
        /// <param name="id">製品ID</param>
        /// <param name="productService">製品サービス</param>
        /// <returns>削除成功フラグ</returns>
        [Authorize(Policy = AuthenticationConstants.Policies.ApiOrB2C)]
        public async Task<bool> DeleteProduct(
            int id,
            [Service] IProductService productService)
        {
            return await productService.DeleteProductAsync(id);
        }
    }
}
