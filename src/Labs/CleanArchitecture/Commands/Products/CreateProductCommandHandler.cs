using AutoMapper;
using MediatR;
using VK.Lab.CleanArchitecture.DTOs.Products;
using VK.Lab.CleanArchitecture.Models;
using VK.Lab.CleanArchitecture.Services;

namespace VK.Lab.CleanArchitecture.Commands.Products
{
    /// <summary>
    /// 创建产品命令处理器
    /// </summary>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            var createdProduct = await _productService.CreateProductAsync(product);
            return _mapper.Map<ProductDto>(createdProduct);
        }
    }
}
