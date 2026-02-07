using Microsoft.EntityFrameworkCore;
using VK.Lab.LayeredArchitecture.Data;
using VK.Lab.LayeredArchitecture.Models;

namespace VK.Lab.LayeredArchitecture.Repositories
{
    /// <summary>
    /// 製品リポジトリ実裁E
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> AddAsync(Product product)
        {
            _context.Products.Add(product);
            await SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(e => e.Id == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
