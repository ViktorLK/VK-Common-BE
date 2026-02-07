using AutoMapper;
using MediatR;
using VK.Lab.CleanArchitecture.DTOs.Products;
using VK.Lab.CleanArchitecture.Services;

namespace VK.Lab.CleanArchitecture.Queries.Products
{
    /// <summary>
    /// 获取所有产品查询处理器
    /// </summary>
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public GetAllProductsQueryHandler(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productService.GetAllProductsAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}
