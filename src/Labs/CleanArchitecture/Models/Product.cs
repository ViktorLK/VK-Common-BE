using System.ComponentModel.DataAnnotations;

namespace VK.Lab.CleanArchitecture.Models
{
    /// <summary>
    /// 製品エンティティ
    /// </summary>
    public class Product
    {
        /// <summary>
        /// 製品ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 製品名
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 製品価格
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
