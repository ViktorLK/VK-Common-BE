using Microsoft.EntityFrameworkCore;
using VK.Lab.CleanArchitecture.Models;
using VK.Lab.CleanArchitecture.Repositories;

namespace VK.Lab.CleanArchitecture.Services
{
    /// <summary>
    /// 製品サービス実装
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

        public IQueryable<Product> GetProductsQueryable()
        {
            return _repository.GetQueryable();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            // ここでビジネスロジック検証を追加可能
            // 例：製品名の重複チェック、価格範囲検証など

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
