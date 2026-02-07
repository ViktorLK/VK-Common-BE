using MediatR;
using VK.Lab.CleanArchitecture.DTOs.Products;

namespace VK.Lab.CleanArchitecture.Queries.Products
{
    /// <summary>
    /// 获取所有产品查询
    /// </summary>
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
    {
    }
}
