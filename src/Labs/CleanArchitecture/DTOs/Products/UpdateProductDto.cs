using System.ComponentModel.DataAnnotations;

namespace VK.Lab.CleanArchitecture.DTOs.Products
{
    /// <summary>
    /// 更新产品数据传输对象
    /// </summary>
    public class UpdateProductDto
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 产品价格
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }
    }
}
