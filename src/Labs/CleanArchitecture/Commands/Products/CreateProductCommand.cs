using MediatR;
using VK.Lab.CleanArchitecture.DTOs.Products;

namespace VK.Lab.CleanArchitecture.Commands.Products
{
    /// <summary>
    /// 创建产品命令
    /// </summary>
    public class CreateProductCommand : IRequest<ProductDto>
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 产品价格
        /// </summary>
        public decimal Price { get; set; }
    }
}
