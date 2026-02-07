using System.ComponentModel.DataAnnotations;

namespace VK.Lab.CleanArchitecture.DTOs.Products
{
    /// <summary>
    /// 产品数据传输对象（用于API响应）
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 产品价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
