using MediatR;
using VK.Lab.CleanArchitecture.Services;

namespace VK.Lab.CleanArchitecture.Commands.Products
{
    /// <summary>
    /// 删除产品命令处理器
    /// </summary>
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductService _productService;

        public DeleteProductCommandHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            return await _productService.DeleteProductAsync(request.Id);
        }
    }
}
