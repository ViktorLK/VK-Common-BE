using MediatR;

namespace VK.Lab.CleanArchitecture.Commands.Products
{
    /// <summary>
    /// 更新产品命令
    /// </summary>
    public class UpdateProductCommand : IRequest<bool>
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
    }
}
