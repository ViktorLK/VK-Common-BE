using MediatR;

namespace VK.Lab.CleanArchitecture.Commands.Products
{
    /// <summary>
    /// 删除产品命令
    /// </summary>
    public class DeleteProductCommand : IRequest<bool>
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public int Id { get; set; }

        public DeleteProductCommand(int id)
        {
            Id = id;
        }
    }
}
