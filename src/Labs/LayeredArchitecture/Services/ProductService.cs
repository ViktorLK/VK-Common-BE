using Microsoft.EntityFrameworkCore;
using VK.Lab.LayeredArchitecture.Models;
using VK.Lab.LayeredArchitecture.Repositories;

namespace VK.Lab.LayeredArchitecture.Services
{
    /// <summary>
    /// 製品サービス実裁E
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            // ここでビジネスロジチE��検証を追加可能
            // 例：製品名の重褁E��ェチE��、価格篁E��検証など

            return await _repository.AddAsync(product);
        }

        public async Task<bool> UpdateProductAsync(int id, Product product)
        {
            if (id != product.Id)
            {
                return false;
            }

            try
            {
                await _repository.UpdateAsync(product);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _repository.ExistsAsync(id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is null)
            {
                return false;
            }

            await _repository.DeleteAsync(product);
            return true;
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
