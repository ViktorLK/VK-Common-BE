using Microsoft.EntityFrameworkCore;
using VK.Lab.LayeredArchitecture.Models;

namespace VK.Lab.LayeredArchitecture.Data
{
    /// <summary>
    /// アプリケーションチE�Eタベ�EスコンチE��スチE
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 製品テーブル
        /// </summary>
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 製品エンチE��チE��の設宁E
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            });
        }
    }
}
