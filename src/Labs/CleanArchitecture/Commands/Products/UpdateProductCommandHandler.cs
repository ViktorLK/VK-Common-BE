using AutoMapper;
using MediatR;
using VK.Lab.CleanArchitecture.Models;
using VK.Lab.CleanArchitecture.Services;

namespace VK.Lab.CleanArchitecture.Commands.Products
{
    /// <summary>
    /// 更新产品命令处理器
    /// </summary>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(request);
            return await _productService.UpdateProductAsync(request.Id, product);
        }
    }
}
