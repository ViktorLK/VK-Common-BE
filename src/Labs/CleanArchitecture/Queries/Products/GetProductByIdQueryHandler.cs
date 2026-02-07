using AutoMapper;
using MediatR;
using VK.Lab.CleanArchitecture.DTOs.Products;
using VK.Lab.CleanArchitecture.Services;

namespace VK.Lab.CleanArchitecture.Queries.Products
{
    /// <summary>
    /// 根据ID获取产品查询处理器
    /// </summary>
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductByIdAsync(request.Id);
            return product == null ? null : _mapper.Map<ProductDto>(product);
        }
    }
}
