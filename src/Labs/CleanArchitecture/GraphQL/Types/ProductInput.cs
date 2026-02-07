using System.ComponentModel.DataAnnotations;

namespace VK.Lab.CleanArchitecture.GraphQL.Types
{
    /// <summary>
    /// 製品作成・更新用の入力型
    /// </summary>
    public class ProductInput
    {
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
    }
}
