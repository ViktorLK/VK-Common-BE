using System.ComponentModel.DataAnnotations;

namespace VK.Lab.LayeredArchitecture.DTOs
{
    /// <summary>
    /// 产品数据传输对象�E�用于API响应！E
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
        /// 创建日朁E
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
